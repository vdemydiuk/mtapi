#pragma once

#include <functional>

typedef std::function<void(int handle, uint64_t command_id, const std::string& payload)> TaskCallback;

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
        callback_(handle, command_id_, response);
    }

private:
    uint64_t command_id_;
    int command_type_;
    std::string payload_;
    TaskCallback callback_;
};