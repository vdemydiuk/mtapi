using System;

namespace MtApi5
{
    public interface IMqlTradeTransaction
    {
        ulong Deal { get; set; }
        ENUM_DEAL_TYPE DealType { get; set; }
        int MtTimeExpiration { get; }
        ulong Order { get; set; }
        ENUM_ORDER_STATE OrderState { get; set; }
        ENUM_ORDER_TYPE OrderType { get; set; }
        ulong Position { get; set; }
        ulong PositionBy { get; set; }
        double Price { get; set; }
        double PriceSl { get; set; }
        double PriceTp { get; set; }
        double PriceTrigger { get; set; }
        string Symbol { get; set; }
        DateTime TimeExpiration { get; set; }
        ENUM_ORDER_TYPE_TIME TimeType { get; set; }
        ENUM_TRADE_TRANSACTION_TYPE Type { get; set; }
        double Volume { get; set; }
    }
}