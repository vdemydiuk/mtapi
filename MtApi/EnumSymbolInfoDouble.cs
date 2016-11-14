namespace MtApi
{
    // https://docs.mql4.com/constants/environment_state/marketinfoconstants#enum_symbol_info_double
    public enum EnumSymbolInfoDouble
    {
        SYMBOL_BID                          = 1, // Bid - best sell offer
        SYMBOL_BIDHIGH                      = 2, // Not supported
        SYMBOL_BIDLOW                       = 3, // Not supported
        SYMBOL_ASK                          = 4, // Ask - best buy offer
        SYMBOL_ASKHIGH                      = 5, // Not supported
        SYMBOL_ASKLOW                       = 6, // Not supported
        SYMBOL_LAST                         = 7, // Not supported
        SYMBOL_LASTHIGH                     = 8, // Not supported
        SYMBOL_LASTLOW                      = 9, // Not supported
        SYMBOL_POINT                        = 16, // Symbol point value
        SYMBOL_TRADE_TICK_VALUE             = 26, // Value of SYMBOL_TRADE_TICK_VALUE_PROFIT
        SYMBOL_TRADE_TICK_VALUE_PROFIT      = 53, // Not supported
        SYMBOL_TRADE_TICK_VALUE_LOSS        = 54, // Not supported
        SYMBOL_TRADE_TICK_SIZE              = 27, // Minimal price change
        SYMBOL_TRADE_CONTRACT_SIZE          = 28, // Trade contract size
        SYMBOL_VOLUME_MIN                   = 34, // Minimal volume for a deal
        SYMBOL_VOLUME_MAX                   = 35, // Maximal volume for a deal
        SYMBOL_VOLUME_STEP                  = 36, // Minimal volume change step for deal execution
        SYMBOL_VOLUME_LIMIT                 = 55, // Not supported
        SYMBOL_SWAP_LONG                    = 38, // Buy order swap value
        SYMBOL_SWAP_SHORT                   = 39, // Sell order swap value
        SYMBOL_MARGIN_INITIAL               = 42, // Initial margin means the amount in the margin currency required for opening an order with the volume of one lot. It is used for checking a client's assets when he or she enters the market.
        SYMBOL_MARGIN_MAINTENANCE           = 43, // The maintenance margin. If it is set, it sets the margin amount in the margin currency of the symbol, charged from one lot. It is used for checking a client's assets when his/her account state changes. If the maintenance margin is equal to 0, the initial margin is used.
        SYMBOL_MARGIN_LONG                  = 44, // Not supported
        SYMBOL_MARGIN_SHORT                 = 45, // Not supported
        SYMBOL_MARGIN_LIMIT                 = 46, // Not supported
        SYMBOL_MARGIN_STOP                  = 47, // Not supported
        SYMBOL_MARGIN_STOPLIMIT             = 48, // Not supported
        SYMBOL_SESSION_VOLUME               = 57, // Not supported
        SYMBOL_SESSION_TURNOVER             = 58, // Not supported
        SYMBOL_SESSION_INTEREST             = 59, // Not supported
        SYMBOL_SESSION_BUY_ORDERS_VOLUME    = 61, // Not supported
        SYMBOL_SESSION_SELL_ORDERS_VOLUME   = 63, // Not supported
        SYMBOL_SESSION_OPEN                 = 64, // Not supported
        SYMBOL_SESSION_CLOSE                = 65, // Not supported
        SYMBOL_SESSION_AW                   = 66, // Not supported
        SYMBOL_SESSION_PRICE_SETTLEMENT     = 67, // Not supported
        SYMBOL_SESSION_PRICE_LIMIT_MIN      = 68, // Not supported
        SYMBOL_SESSION_PRICE_LIMIT_MAX      = 69, // Not supported
    }
}