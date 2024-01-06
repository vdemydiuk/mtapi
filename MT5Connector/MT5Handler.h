//MT5Handler.h

#pragma once

#include <Windows.h>
#include <MetaTraderHandler.h>

class MT5Handler: public MetaTraderHandler
{
public:
    MT5Handler(int handle)
        : handle_(handle)
    {
        msgId = WM_TIMER;
    }

    ~MT5Handler() override = default;

private:
    void SendTickToMetaTrader() override
    {
        PostMessage((HWND)handle_, msgId, 0, 0);
    }

    int handle_;
    unsigned int msgId;
};