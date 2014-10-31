//MT4Handler.h

#pragma once

#include <Windows.h>

using namespace MTApiService;

ref class MT4Handler: IMetaTraderHandler
{
public:
	MT4Handler()
	{
		msgId = RegisterWindowMessage("MetaTrader4_Internal_Message");
	}

	virtual void SendTickToMetaTrader(int handle)
	{
		PostMessage((HWND)handle, msgId, 2, 1);
	}

private:
	unsigned int msgId;
};
