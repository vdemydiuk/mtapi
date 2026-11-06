#include "pch.h"

#include "MtExpert.h"
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

    OnDeinit(handle_);
}

void MtExpert::SendEvent(int event_type, const std::string& payload)
{
    log_.Trace("%s: handle = %d, event_type = %d, payload = %s", __FUNCTION__, handle_, event_type, payload.c_str());

    OnEvent(handle_, event_type, payload);
}

void MtExpert::SendResponse(const std::string& payload)
{
    log_.Trace("%s: handle = %d, payload = %s", __FUNCTION__, handle_, payload.c_str());

    if (current_task_)
    {
        current_task_->ProcessResponse(handle_, payload);
        current_task_.reset();
    }
}

void MtExpert::ProcessCommand(uint64_t command_id, int command_type, std::string&& payload, TaskCallback&& callback)
{
    log_.Debug("%s: handle = %d, command id = %llu, command type = %d, payload = %s", __FUNCTION__,
        handle_, static_cast<unsigned long long>(command_id), command_type, payload.c_str());
    auto task = std::make_unique<CommandTask>(command_id, command_type, std::move(payload), std::move(callback));
    tasks_.push(std::move(task));
    mt_handler_->SendTickToMetaTrader();
}

int MtExpert::GetCommandType()
{
    if (tasks_.size() == 0)
        return MT_COMMAND_TYPE_EMPTY;

    current_task_ = std::move(tasks_.front());
    tasks_.pop();

    return current_task_->getCommandType();
}

std::string MtExpert::GetCommandPayload()
{
    return current_task_ ? current_task_->getPayload() : "";
}