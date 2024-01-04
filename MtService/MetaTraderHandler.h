#pragma once

class MetaTraderHandler
{
public:
    virtual ~MetaTraderHandler() = default;
    virtual void SendTickToMetaTrader() = 0;
};