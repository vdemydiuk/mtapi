#include "pch.h"

#include "MtExpert.h"
#include <algorithm>
#include <iostream>

namespace
{
constexpr int MT_COMMAND_TYPE_EMPTY = 0;
}

MtExpert::MtExpert(int handle, std::unique_ptr<MetaTraderHandler> mt_handler)
    : handle_(handle)
    , mt_handler_(std::move(mt_handler))
{
    log_.Debug("%s: expert %d created.", __FUNCTION__, handle_);
}

MtExpert::~MtExpert()
{
    log_.Debug("%s: expert %d destroyed.", __FUNCTION__, handle_);
}

void MtExpert::Deinit()
{
    log_.Debug("%s: handle = %d", __FUNCTION__, handle_);

    // Events queued before the expert was removed are sent before the MtExpertRemoved notification.
    // The queue is bounded by MaxQueuedEvents, and no new events arrive: the expert is no longer
    // reachable from the terminal thread.
    DrainEvents(MaxQueuedEvents);

    CancelTasks();
    OnDeinit(handle_);
}

void MtExpert::CancelTasks()
{
    std::unique_ptr<CommandTask> current_task;
    std::queue<std::unique_ptr<CommandTask>> tasks;
    {
        std::lock_guard<std::mutex> lock(mutex_);
        current_task = std::move(current_task_);
        std::swap(tasks, tasks_);
    }

    // Reply to every command that will never be executed, so that clients don't wait for timeout.
    // Callbacks are invoked outside the lock.
    const std::string reason = "MtService: expert " + std::to_string(handle_) + " was removed before the command was executed";

    if (current_task)
    {
        log_.Warning("%s: cancel current command of expert %d", __FUNCTION__, handle_);
        current_task->Cancel(handle_, reason);
    }

    if (!tasks.empty())
        log_.Warning("%s: cancel %zu queued command(s) of expert %d", __FUNCTION__, tasks.size(), handle_);

    while (!tasks.empty())
    {
        tasks.front()->Cancel(handle_, reason);
        tasks.pop();
    }
}

bool MtExpert::PushEvent(int event_type, std::string payload)
{
    std::lock_guard<std::mutex> lock(events_mutex_);

    if (events_.size() >= MaxQueuedEvents)
    {
        events_.pop_front();
        ++dropped_events_;

        if (dropped_events_ == 1)
            log_.Warning("%s: expert %d: events are produced faster than the service sends them, "
                "more than %zu events are queued. Dropping the oldest events.",
                __FUNCTION__, handle_, MaxQueuedEvents);
        else if (dropped_events_ % 10000 == 0)
            log_.Warning("%s: expert %d: %zu events dropped so far.", __FUNCTION__, handle_, dropped_events_);
    }

    events_.push_back({ event_type, std::move(payload) });

    if (drain_pending_)
        return false;
    drain_pending_ = true;
    return true;
}

bool MtExpert::DrainEvents(std::size_t max_count)
{
    std::vector<QueuedEvent> batch;
    bool more;
    {
        std::lock_guard<std::mutex> lock(events_mutex_);

        const auto count = std::min(max_count, events_.size());
        batch.reserve(count);
        for (std::size_t i = 0; i < count; ++i)
        {
            batch.push_back(std::move(events_.front()));
            events_.pop_front();
        }

        more = !events_.empty();
        if (!more)
        {
            drain_pending_ = false;
            if (dropped_events_ > 0)
            {
                log_.Warning("%s: expert %d: event queue drained, %zu events were dropped.",
                    __FUNCTION__, handle_, dropped_events_);
                dropped_events_ = 0;
            }
        }
    }

    // Handlers are invoked outside the lock, so the terminal thread is not blocked while events are encoded.
    for (const auto& e : batch)
    {
        log_.Trace("%s: handle = %d, event_type = %d, payload = %s", __FUNCTION__, handle_, e.event_type, e.payload.c_str());
        OnEvent(handle_, e.event_type, e.payload);
    }

    return more;
}

void MtExpert::ProcessCommand(uint64_t command_id, int command_type, std::string&& payload, TaskCallback&& callback)
{
    log_.Debug("%s: handle = %d, command id = %llu, command type = %d, payload = %s", __FUNCTION__,
        handle_, static_cast<unsigned long long>(command_id), command_type, payload.c_str());
    auto task = std::make_unique<CommandTask>(command_id, command_type, std::move(payload), std::move(callback));
    {
        std::lock_guard<std::mutex> lock(mutex_);
        tasks_.push(std::move(task));
    }
    mt_handler_->SendTickToMetaTrader();
}

int MtExpert::GetCommandType()
{
    std::lock_guard<std::mutex> lock(mutex_);

    if (tasks_.empty())
        return MT_COMMAND_TYPE_EMPTY;

    if (current_task_)
        log_.Warning("%s: expert %d did not respond to the previous command, it is dropped", __FUNCTION__, handle_);

    current_task_ = std::move(tasks_.front());
    tasks_.pop();

    return current_task_->getCommandType();
}

std::string MtExpert::GetCommandPayload()
{
    std::lock_guard<std::mutex> lock(mutex_);
    return current_task_ ? current_task_->TakePayload() : std::string();
}

std::unique_ptr<CommandTask> MtExpert::TakeCurrentTask()
{
    std::lock_guard<std::mutex> lock(mutex_);
    return std::move(current_task_);
}
