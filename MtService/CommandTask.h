#pragma once

#include <functional>
#include "MtMessage.h"

typedef std::function<void(const MtResponse&)> TaskCallback;

class CommandTask
{
public:
    CommandTask(std::unique_ptr<MtCommand> command, TaskCallback callback)
        : command_(std::move(command))
        , callback_(std::move(callback))
    {}

    MtCommand& getCommand() const { return *command_; }

    void ProcessResponse(int handle, const std::string& payload)
    {
        MtResponse respone(handle, command_->getCommandId(), payload);
        callback_(respone);
    }

private:
    std::unique_ptr<MtCommand> command_;
    TaskCallback callback_;
};