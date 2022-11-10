// ReSharper disable InconsistentNaming
using System;

namespace MtApi5
{
    public interface IMqlTradeRequest
    {
        ENUM_TRADE_REQUEST_ACTIONS Action { get; set; }
        string Comment { get; set; }
        ulong Deviation { get; set; }
        DateTime Expiration { get; set; }
        ulong Magic { get; set; }
        int MtExpiration { get; }
        ulong Order { get; set; }
        ulong Position { get; set; }
        ulong PositionBy { get; set; }
        double Price { get; set; }
        double Sl { get; set; }
        double Stoplimit { get; set; }
        string Symbol { get; set; }
        double Tp { get; set; }
        ENUM_ORDER_TYPE Type { get; set; }
        ENUM_ORDER_TYPE_FILLING Type_filling { get; set; }
        ENUM_ORDER_TYPE_TIME Type_time { get; set; }
        double Volume { get; set; }
    }
}