#pragma once

#include <cstdint>
#include <functional>
#include <string>

// Outcome of a command task. Kept independent of the SBE generated types (namespace MtService),
// because this header is included where class MtService is defined.
enum class TaskResult
{
    Completed,      // MetaTrader executed the command, payload contains its response
    ExpertRemoved   // expert was removed before the command was executed, payload contains error message
};

typedef std::function<void(int handle, uint64_t command_id, TaskResult result, const std::string& payload)> TaskCallback;

class CommandTask
{
public:
    CommandTask(uint64_t command_id, int command_type, std::string&& payload, TaskCallback&& callback)
        : command_id_(command_id)
        , command_type_(command_type)
        , payload_(std::move(payload))
        , callback_(std::move(callback))
    {
    }

    int getCommandType() const { return command_type_; }
    std::string getPayload() const { return payload_; }

    void ProcessResponse(int handle, const std::string& response)
    {
        callback_(handle, command_id_, TaskResult::Completed, response);
    }

    void Cancel(int handle, const std::string& reason)
    {
        callback_(handle, command_id_, TaskResult::ExpertRemoved, reason);
    }

private:
    uint64_t command_id_;
    int command_type_;
    std::string payload_;
    TaskCallback callback_;
};