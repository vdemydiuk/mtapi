// MtService.cpp : Defines the functions for the static library.
//

#include "pch.h"
#include "framework.h"
#include "MtService.h"

#include "MtExpert.h"
#include "LogConfigurator.h"
#include "MtServer.h"

#include <boost/asio.hpp>
#include <iostream>
#include <mutex>
#include <stdexcept>

//--------------------------------------------------------------------------------
// MtService
//--------------------------------------------------------------------------------

class MtServiceImpl
{
public:
    MtServiceImpl();
    ~MtServiceImpl();

    void InitExpert(int port, int handle, std::unique_ptr<MetaTraderHandler> mt_handler);
    void DeinitExpert(int handle);

    void SendEvent(int handle, int event_type, std::string payload);
    void SendResponse(int handle, std::string payload);

    int GetCommandType(int handle);
    std::string GetCommandPayload(int handle);

    void LogError(const std::string& error);

private:
    // Max number of events one drain handler sends before it yields to other io handlers
    // (socket reads and writes, commands, responses).
    static constexpr std::size_t EventBatchSize = 100;

    std::shared_ptr<MtExpert> FindExpert(int handle) const;
    void PostDrainEvents(std::shared_ptr<MtExpert> expert);
    void OnServerStopped(unsigned short port);
    void ThreadProc();

    Logger log_{ "MtService" };
    boost::asio::io_context context_;
    std::unique_ptr<boost::asio::io_context::work> work_;
    std::thread thread_;

    // Accessed both from terminal threads (command fetching, which must not wait for the io thread)
    // and from the io thread. An expert is removed from the map on the terminal thread and
    // deinitialized on the io thread; shared ownership keeps it alive until both are done with it.
    mutable std::mutex experts_mutex_;
    std::unordered_map<int, std::shared_ptr<MtExpert>> experts_;

    // Accessed only on the io thread.
    std::unordered_map<unsigned short, std::shared_ptr<MtServer>> servers_;
};

MtServiceImpl::MtServiceImpl()
    : work_(new boost::asio::io_context::work(context_))
    , thread_(&MtServiceImpl::ThreadProc, this)
{
    log_.Debug("%s: service created", __FUNCTION__);
}

MtServiceImpl::~MtServiceImpl()
{
    log_.Trace("%s: destructor started", __FUNCTION__);

    std::unordered_map<int, std::shared_ptr<MtExpert>> experts;
    {
        std::lock_guard<std::mutex> lock(experts_mutex_);
        experts.swap(experts_);
    }

    boost::asio::post(context_, [experts = std::move(experts)]() {
        for (auto& e : experts)
            e.second->Deinit();
    });
    work_.reset();
    if (thread_.joinable())
        thread_.join();

    log_.Debug("%s: service destroyed", __FUNCTION__);
}

std::shared_ptr<MtExpert> MtServiceImpl::FindExpert(int handle) const
{
    std::lock_guard<std::mutex> lock(experts_mutex_);
    auto it = experts_.find(handle);
    return (it != experts_.end()) ? it->second : nullptr;
}

void MtServiceImpl::InitExpert(int port, int handle, std::unique_ptr<MetaTraderHandler> mt_handler)
{
    log_.Info("%s: port = %d, handle = %d", __FUNCTION__, port, handle);

    // The expert is registered synchronously, so that it is visible to GetCommandType right away.
    auto expert = std::make_shared<MtExpert>(handle, std::move(mt_handler));
    {
        std::lock_guard<std::mutex> lock(experts_mutex_);
        if (!experts_.emplace(handle, expert).second)
            throw std::runtime_error("MtService: expert with handle " + std::to_string(handle) + " is already initialized");
    }

    boost::asio::post(context_, [port, handle, expert = std::move(expert), this]() {
        auto it = servers_.find(port);
        if (it == servers_.end())
        {
            auto server = std::make_shared<MtServer>(context_, port);
            server->OnStopped.connect(boost::bind(&MtServiceImpl::OnServerStopped, this, std::placeholders::_1));
            it = servers_.emplace(port, std::move(server)).first;
        }

        it->second->AddExpert(handle, expert.get());
    });
}

void MtServiceImpl::DeinitExpert(int handle)
{
    log_.Info("%s: handle = %d", __FUNCTION__, handle);

    std::shared_ptr<MtExpert> expert;
    {
        std::lock_guard<std::mutex> lock(experts_mutex_);
        auto it = experts_.find(handle);
        if (it == experts_.end())
            return;
        expert = std::move(it->second);
        experts_.erase(it);
    }

    // Deinit runs on the io thread after all previously posted responses of the expert;
    // it sends the events that are still queued in the expert first.
    boost::asio::post(context_, [expert = std::move(expert)]() {
        expert->Deinit();
    });
}

void MtServiceImpl::SendEvent(int handle, int event_type, std::string payload)
{
    log_.Trace("%s: handle = %d, event_type = %d, payload = %s", __FUNCTION__, handle, event_type, payload.c_str());

    auto expert = FindExpert(handle);
    if (!expert)
        return;

    // Events are not posted one by one: when the expert produces them faster than the io thread
    // handles them, the io queue would grow without limit and socket reads and writes would wait
    // behind the whole backlog. The expert queues them, and a single drain handler sends them.
    if (expert->PushEvent(event_type, std::move(payload)))
        PostDrainEvents(std::move(expert));
}

void MtServiceImpl::PostDrainEvents(std::shared_ptr<MtExpert> expert)
{
    boost::asio::post(context_, [expert = std::move(expert), this]() mutable {
        // Send a bounded batch, then go to the back of the io queue so that other handlers run in between.
        if (expert->DrainEvents(EventBatchSize))
            PostDrainEvents(std::move(expert));
    });
}

void MtServiceImpl::SendResponse(int handle, std::string payload)
{
    log_.Trace("%s: handle = %d, payload = %s", __FUNCTION__, handle, payload.c_str());

    auto expert = FindExpert(handle);
    if (!expert)
        return;

    // The task is detached right here on the terminal thread: the next GetCommandType call
    // must not see it as the current task, even if the io thread has not handled the response yet.
    auto task = expert->TakeCurrentTask();
    if (!task)
    {
        log_.Warning("%s: expert %d has no command to respond to", __FUNCTION__, handle);
        return;
    }

    // Callbacks touch connections, so they are invoked on the io thread.
    boost::asio::post(context_, [handle, task = std::move(task), payload = std::move(payload)]() {
        task->ProcessResponse(handle, payload);
    });
}

// Called on the terminal thread. Reads the expert's command queue directly instead of
// going through the io thread, where it would wait behind every queued event.
int MtServiceImpl::GetCommandType(int handle)
{
    auto expert = FindExpert(handle);
    auto command_type = expert ? expert->GetCommandType() : 0;
    log_.Trace("%s: handle = %d, command_type = %d", __FUNCTION__, handle, command_type);

    return command_type;
}

std::string MtServiceImpl::GetCommandPayload(int handle)
{
    auto expert = FindExpert(handle);
    auto payload = expert ? expert->GetCommandPayload() : std::string();
    log_.Trace("%s: handle = %d, payload = %s", __FUNCTION__, handle, payload.c_str());

    return payload;
}

void MtServiceImpl::LogError(const std::string& error)
{
    log_.Error("%s: %s", __FUNCTION__, error.c_str());
}

void MtServiceImpl::OnServerStopped(unsigned short port)
{
    log_.Trace("%s: port = %d", __FUNCTION__, port);

    servers_.erase(port);
}

void MtServiceImpl::ThreadProc()
{
    std::thread::id this_id = std::this_thread::get_id();
    std::stringstream ss;
    ss << this_id;

    log_.Debug("%s: thread %s started", __FUNCTION__, ss.str().c_str());
    context_.run();
    log_.Debug("%s: thread %s stopped", __FUNCTION__, ss.str().c_str());
}

//--------------------------------------------------------------------------------
// MtService
//--------------------------------------------------------------------------------

MtService::MtService()
{
#ifdef NDEBUG
    // nondebug
    auto log_level = LogLevel::Info;
#else
    // debug code
    auto log_level = LogLevel::Trace;
#endif

    LogConfigurator::Setup(log_level, OutputType::Console | OutputType::File, "MtApiService");
    impl_ = std::make_unique<MtServiceImpl>();
}

MtService::~MtService()
{
}

MtService& MtService::GetInstance()
{
    static MtService instance;
    return instance;
}

void MtService::InitExpert(int port, int handle, std::unique_ptr<MetaTraderHandler> mt_handler)
{
     impl_->InitExpert(port, handle, std::move(mt_handler));
}

void MtService::DeinitExpert(int handle)
{
     impl_->DeinitExpert(handle);
}

void MtService::SendEvent(int handle, int event_type, std::string payload)
{
     impl_->SendEvent(handle, event_type, std::move(payload));
}

void MtService::SendResponse(int handle, std::string payload)
{
     impl_->SendResponse(handle, std::move(payload));
}

int MtService::GetCommandType(int handle)
{
     return impl_->GetCommandType(handle);
}

std::string MtService::GetCommandPayload(int handle)
{
    return impl_->GetCommandPayload(handle);
}

void MtService::LogError(const std::string& error)
{
    impl_->LogError(error);
}