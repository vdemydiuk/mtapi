//MT5Handler.h

#pragma once

#include <Windows.h>

using namespace MTApiService;

ref class MT5Handler: IMetaTraderHandler
{
public:
    MT5Handler()
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