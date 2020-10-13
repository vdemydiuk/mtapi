// ReSharper disable InconsistentNaming

namespace MtApi5.Requests
{
    internal enum RequestType
    {
        Unknown             = 0,
        CopyTicks           = 1,
        iCustom             = 2,
        OrderSend           = 3,
        PositionOpen        = 4,
        OrderCheck          = 5,
        MarketBookGet       = 6,
        IndicatorCreate     = 7,
        SymbolInfoString    = 8,
        ChartTimePriceToXY  = 9,
        ChartXYToTimePrice  = 10,
        PositionClose       = 11,
        SymbolInfoTick      = 12,
        Buy                 = 13,
        Sell                = 14,
        OrderSendAsync      = 15
    }
}