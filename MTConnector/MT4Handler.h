//MT4Handler.h

#pragma once

#include <Windows.h>

using namespace MTApiService;

ref class MT4Handler: IMetaTraderHandler
{
public:
    MT4Handler()
    {
        msgId = WM_TIMER;
    }

    virtual void SendTickToMetaTrader(int handle)
    {
        PostMessage((HWND)handle, msgId, 0, 0);
    }

private:
    unsigned int msgId;
};
