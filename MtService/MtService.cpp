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

    void SendEvent(int handle, int event_type, const std::string& payload);
    void SendResponse(int handle, const std::string& payload);

    int GetCommandType(int handle);
    std::string GetCommandPayload(int handle);

    void LogError(const std::string& error);

private:
    void OnServerStopped(unsigned short port);
    void ThreadProc();

    Logger log_{ "MtService" };
    boost::asio::io_context context_;
    std::unique_ptr<boost::asio::io_context::work> work_;
    std::thread thread_;

    std::unordered_map<int, std::unique_ptr<MtExpert>> experts_;
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

    boost::asio::post(context_, [this]() {
        if (experts_.size() > 0)
        {
            for (auto& e : experts_)
                e.second->Deinit();
            experts_.clear();
        }
    });
    work_.reset();
    if (thread_.joinable())
        thread_.join();

    log_.Debug("%s: service destroyed", __FUNCTION__);
}

void MtServiceImpl::InitExpert(int port, int handle, std::unique_ptr<MetaTraderHandler> mt_handler)
{
    log_.Debug("%s: port = %d, handle = %d", __FUNCTION__, port, handle);

    boost::asio::post(context_, [port, handle, mt_h = std::move(mt_handler), this]() mutable {
        auto expert = std::make_unique<MtExpert>(handle, std::move(mt_h));
        if (servers_.count(port) == 0)
        {
            auto server = std::make_shared<MtServer>(context_, port);
            server->OnStopped.connect(boost::bind(&MtServiceImpl::OnServerStopped, this, std::placeholders::_1));
            servers_[port] = server;
        }

        servers_[port]->AddExpert(handle, expert.get());
        experts_[handle] = std::move(expert);
    });
}

void MtServiceImpl::DeinitExpert(int handle)
{
    log_.Debug("%s: handle = %d", __FUNCTION__, handle);

    boost::asio::post(context_, [handle, this]() {
        if (experts_.count(handle) > 0)
        {
            experts_[handle]->Deinit();
            experts_.erase(handle);
        }
    });
}

void MtServiceImpl::SendEvent(int handle, int event_type, const std::string& payload)
{
    log_.Trace("%s: handle = %d, event_type = %d, payload = %s", __FUNCTION__, handle, event_type, payload.c_str());

    boost::asio::post(context_, [handle, event_type, payload = payload, this]() {
        if (experts_.count(handle) > 0)
            experts_[handle]->SendEvent(event_type, payload);
    });
}

void MtServiceImpl::SendResponse(int handle, const std::string& payload)
{
    log_.Trace("%s: handle = %d, payload = %s", __FUNCTION__, handle, payload.c_str());

    boost::asio::post(context_, [handle, payload = payload, this]() {
        if (experts_.count(handle) > 0)
            experts_[handle]->SendResponse(payload);
    });
}

int MtServiceImpl::GetCommandType(int handle)
{
    std::packaged_task<int()> task([handle, this]() {
        return (experts_.count(handle) > 0) ? experts_[handle]->GetCommandType() : 0;
    });
    auto f = task.get_future();
    boost::asio::post(context_, std::bind(std::move(task)));

    auto command_type = f.get();
    log_.Trace("%s: handle = %d, command_type = %d", __FUNCTION__, handle, command_type);
    
    return command_type;
}

std::string MtServiceImpl::GetCommandPayload(int handle)
{
    std::packaged_task<std::string()> task([handle, this]() {
        return (experts_.count(handle) > 0) ? experts_[handle]->GetCommandPayload() : "";
    });
    auto f = task.get_future();
    boost::asio::post(context_, std::bind(std::move(task)));

    auto payload = f.get();
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

    if (servers_.count(port) > 0)
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
    LogConfigurator::Setup(LogLevel::Trace, OutputType::Console | OutputType::File, "MtApiService");
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

void MtService::SendEvent(int handle, int event_type, const std::string& payload)
{
     impl_->SendEvent(handle, event_type, payload);
}

void MtService::SendResponse(int handle, const std::string& payload)
{
     impl_->SendResponse(handle, payload);
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