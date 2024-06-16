//MT4Handler.h

#pragma once

#include <Windows.h>
#include <MetaTraderHandler.h>

class MT4Handler: public MetaTraderHandler
{
public:
    MT4Handler(int handle)
        : handle_(handle)
    {
        msg_id_ = WM_TIMER;
    }

    virtual void SendTickToMetaTrader(int handle)
    {
        PostMessage((HWND)handle, msg_id_, 0, 0);
    }

private:
    void SendTickToMetaTrader() override
    {
        PostMessage((HWND)handle_, msg_id_, 0, 0);
    }

    int handle_;
    unsigned int msg_id_;
};
