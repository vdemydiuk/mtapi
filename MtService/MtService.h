#pragma once

#include <string>
#include <thread>
#include <unordered_map>

#include "MetaTraderHandler.h"

class MtServiceImpl;

class MtService
{
private:
    MtService();
    ~MtService();

public:
    static MtService& GetInstance();

    void InitExpert(int port, int handle, std::unique_ptr<MetaTraderHandler> mt_handler);
    void DeinitExpert(int handle);

    void SendEvent(int handle, int event_type, const std::string& payload);
    void SendResponse(int handle, const std::string& payload);

    int GetCommandType(int handle);
    std::string GetCommandPayload(int handle);

    void LogError(const std::string& error);

private:
    std::unique_ptr<MtServiceImpl> impl_;
};