#pragma once

#include <boost/signals2.hpp>
#include <functional>
#include <memory>
#include <queue>

#include "CommandTask.h"
#include "Logger.h"
#include "MetaTraderHandler.h"
#include "MtMessage.h"

class MtExpert
{
public:
    MtExpert(int handle, std::unique_ptr<MetaTraderHandler> mt_handler);
    ~MtExpert();

    void Deinit();

    void SendEvent(int event_type, const std::string& payload);
    void SendResponse(const std::string& payload);

    void Process(std::unique_ptr<MtCommand> command, TaskCallback callback);

    int GetCommandType();
    std::string GetCommandPayload();

    boost::signals2::signal<void(const MtEvent& event)> OnEvent;
    boost::signals2::signal<void(int)> OnDeinit;

private:
    Logger log_{ "MtExpert" };
    int handle_;
    std::unique_ptr<MetaTraderHandler> mt_handler_;
    std::queue<std::unique_ptr<CommandTask>> tasks_;
    std::unique_ptr<CommandTask> current_task_;
};