#pragma once

#include <boost/signals2.hpp>
#include <functional>
#include <memory>
#include <mutex>
#include <queue>

#include "CommandTask.h"
#include "Logger.h"
#include "MetaTraderHandler.h"

// Threading model:
// - ProcessCommand, SendEvent and Deinit are called on the io thread.
// - GetCommandType, GetCommandPayload and TakeCurrentTask are called directly on the terminal thread,
//   so that fetching a command never waits behind events queued on the io thread.
// The command queue and the current task are shared between these threads and guarded by mutex_.
// Task callbacks (ProcessResponse/Cancel) must be invoked only on the io thread.
class MtExpert
{
public:
    MtExpert(int handle, std::unique_ptr<MetaTraderHandler> mt_handler);
    ~MtExpert();

    void Deinit();

    void SendEvent(int event_type, const std::string& payload);

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
};
