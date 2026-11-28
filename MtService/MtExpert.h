#pragma once

#include <boost/signals2.hpp>
#include <deque>
#include <functional>
#include <memory>
#include <mutex>
#include <queue>
#include <string>
#include <vector>

#include "CommandTask.h"
#include "Logger.h"
#include "MetaTraderHandler.h"

// Threading model:
// - ProcessCommand, DrainEvents and Deinit are called on the io thread.
// - GetCommandType, GetCommandPayload and TakeCurrentTask are called directly on the terminal thread,
//   so that fetching a command never waits behind events queued on the io thread.
// - PushEvent is called on the terminal thread.
// The command queue and the current task are shared between these threads and guarded by mutex_.
// The event queue is guarded by events_mutex_.
// Task callbacks (ProcessResponse/Cancel) must be invoked only on the io thread.
class MtExpert
{
public:
    // Max number of events waiting in the expert's queue for the io thread. The oldest ones are
    // dropped when the io thread handles events slower than the expert produces them.
    static constexpr std::size_t MaxQueuedEvents = 3000;

    MtExpert(int handle, std::unique_ptr<MetaTraderHandler> mt_handler);
    ~MtExpert();

    // Sends the events that are still queued, cancels pending commands and fires OnDeinit.
    void Deinit();

    // Queues an event. Returns true if the caller must schedule DrainEvents on the io thread:
    // only one drain is pending at a time, so the io queue holds at most one event handler
    // per expert, however fast events are produced.
    bool PushEvent(int event_type, std::string payload);
    // Fires OnEvent for up to max_count queued events, in order. Returns true if more events
    // remain; the caller must then schedule DrainEvents again.
    bool DrainEvents(std::size_t max_count);

    void ProcessCommand(uint64_t command_id, int command_type, std::string&& payload, TaskCallback&& callback);

    int GetCommandType();
    std::string GetCommandPayload();
    // Detaches the command that MetaTrader is responding to. The caller must invoke
    // CommandTask::ProcessResponse on the io thread.
    std::unique_ptr<CommandTask> TakeCurrentTask();

    boost::signals2::signal<void(int handle, int event_type, const std::string& payload)> OnEvent;
    boost::signals2::signal<void(int)> OnDeinit;

private:
    void CancelTasks();

    Logger log_{ "MtExpert" };
    int handle_;
    std::unique_ptr<MetaTraderHandler> mt_handler_;

    std::mutex mutex_;
    std::queue<std::unique_ptr<CommandTask>> tasks_;
    std::unique_ptr<CommandTask> current_task_;

    struct QueuedEvent
    {
        int event_type;
        std::string payload;
    };

    std::mutex events_mutex_;
    std::deque<QueuedEvent> events_;
    bool drain_pending_ = false;    // a DrainEvents call is scheduled or running
    std::size_t dropped_events_ = 0; // events dropped since the last overflow began
};
