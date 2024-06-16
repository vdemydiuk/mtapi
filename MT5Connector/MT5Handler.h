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
        msg_id_ = WM_TIMER;
    }

    ~MT5Handler() override = default;

private:
    void SendTickToMetaTrader() override
    {
        PostMessage((HWND)handle_, msg_id_, 0, 0);
    }

    int handle_;
    unsigned int msg_id_;
};