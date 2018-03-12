// ReSharper disable InconsistentNaming
namespace MtApi5
{
    internal enum Mt5CommandType
    {
        //NoCommand = 0

        //trade operations
        //OrderSend                           = 1,
        OrderCalcMargin                     = 2,
        OrderCalcProfit                     = 3,
        //OrderSendAsync                      = 5,
        PositionsTotal                      = 6,
        PositionGetSymbol                   = 7,
        PositionSelect                      = 8,
        PositionGetDouble                   = 9,
        PositionGetInteger                  = 10,
        PositionGetString                   = 11,
        PositionGetTicket                   = 4,
        OrdersTotal                         = 12,
        OrderGetTicket                      = 13,
        OrderSelect                         = 14,
        OrderGetDouble                      = 15,
        OrderGetInteger                     = 16,
        OrderGetString                      = 17,
        HistorySelect                       = 18,
        HistorySelectByPosition             = 19,
        HistoryOrderSelect                  = 20,
        HistoryOrdersTotal                  = 21,
        HistoryOrderGetTicket               = 22,
        HistoryOrderGetDouble               = 23,
        HistoryOrderGetInteger              = 24,
        HistoryOrderGetString               = 25,
        HistoryDealSelect                   = 26,
        HistoryDealsTotal                   = 27,
        HistoryDealGetTicket                = 28,
        HistoryDealGetDouble                = 29,
        HistoryDealGetInteger               = 30,
        HistoryDealGetString                = 31,

        //Account Information
        AccountInfoDouble                   = 32,
        AccountInfoInteger                  = 33,
        AccountInfoString                   = 34,

        //Access to Timeseries and Indicator Data
        SeriesInfoInteger                   = 35,
        Bars                                = 36,
        Bars2                               = 1036,
        BarsCalculated                      = 37,
//        IndicatorCreate                     = 38,
//        IndicatorParameters                 = 38,
//        IndicatorRelease                    = 39,
        CopyBuffer                          = 40,
        CopyBuffer1                         = 1040,
        CopyBuffer2                         = 1140,
        CopyRates                           = 41,
        CopyRates1                          = 1041,
        CopyRates2                          = 1141,
        CopyTime                            = 42,
        CopyTime1                           = 1042,
        CopyTime2                           = 1142,
        CopyOpen                            = 43,
        CopyOpen1                           = 1043,
        CopyOpen2                           = 1143,
        CopyHigh                            = 44,
        CopyHigh1                           = 1044,
        CopyHigh2                           = 1144,
        CopyLow                             = 45,
        CopyLow1                            = 1045,
        CopyLow2                            = 1145,
        CopyClose                           = 46,
        CopyClose1                          = 1046,
        CopyClose2                          = 1146,
        CopyTickVolume                      = 47,
        CopyTickVolume1                     = 1047,
        CopyTickVolume2                     = 1147,
        CopyRealVolume                      = 48,
        CopyRealVolume1                     = 1048,
        CopyRealVolume2                     = 1148,
        CopySpread                          = 49,
        CopySpread1                         = 1049,
        CopySpread2                         = 1149,

        //Market Information
        SymbolsTotal                        = 50,
        SymbolName                          = 51,
        SymbolSelect                        = 52,
        SymbolIsSynchronized                = 53,
        SymbolInfoDouble                    = 54,
        SymbolInfoInteger                   = 55,
        SymbolInfoString                    = 56,
        SymbolInfoTick                      = 57,
        SymbolInfoSessionQuote              = 58,
        SymbolInfoSessionTrade              = 59,
        MarketBookAdd                       = 60,
        MarketBookRelease                   = 61,
        //MarketBookGet                       = 62,
        OrderCloseAll                       = 63,

        //CTrade
        PositionClose                       = 64,
        PositionOpen                        = 65,
        //PositionOpenWithResult              = 1065,

        //Backtesting
        BacktestingReady                    = 66,
        IsTesting                           = 67,

        //Requests
        MtRequest                           = 155,

        PositionSelectByTicket              = 69,

        ObjectCreate                        = 70,
        ObjectName                          = 71,
        ObjectDelete                        = 72,
        ObjectsDeleteAll                    = 73,
        ObjectFind                          = 74,
        ObjectGetTimeByValue                = 75,
        ObjectGetValueByTime                = 76,
        ObjectMove                          = 77,
        ObjectsTotal                        = 78,
        ObjectGetDouble                     = 79,
        ObjectGetInteger                    = 80,
        ObjectGetString                     = 81,
        ObjectSetDouble                     = 82,
        ObjectSetInteger                    = 83,
        ObjectSetString                     = 84,
        //TextSetFont                         = 85,
        //TextOut                             = 86,
        //TextGetSize                         = 87,

        iAC                                 = 88,
        iAD                                 = 89,
        iADX                                = 90,
        iADXWilder                          = 91,
        iAlligator                          = 92,
        iAMA                                = 93,
        iAO                                 = 94,
        iATR                                = 95,
        iBearsPower                         = 96,
        iBands                              = 97,
        iBullsPower                         = 98,
        iCCI                                = 99,
        iChaikin                            = 100,
        //iCustom                             = 101,
        iDEMA                               = 102,
        iDeMarker                           = 103,
        iEnvelopes                          = 104,
        iForce                              = 105,
        iFractals                           = 106,
        iFrAMA                              = 107,
        iGator                              = 108,
        iIchimoku                           = 109,
        iBWMFI                              = 110,
        iMomentum                           = 111,
        iMFI                                = 112,
        iMA                                 = 113,
        iOsMA                               = 114,
        iMACD                               = 115,
        iOBV                                = 116,
        iSAR                                = 117,
        iRSI                                = 118,
        iRVI                                = 119,
        iStdDev                             = 120,
        iStochastic                         = 121,
        iTEMA                               = 122,
        iTriX                               = 123,
        iWPR                                = 124,
        iVIDyA                              = 125,
        iVolumes                            = 126,

        //Date and Time
        TimeCurrent                         = 127,
        TimeTradeServer                     = 128,
        TimeLocal                           = 129,
        TimeGMT                             = 130,

        IndicatorRelease                    = 131,



        //Chart Operations
        ChartId = 206,
        ChartRedraw = 207,
        ChartApplyTemplate = 236,
        ChartSaveTemplate = 237,
        ChartWindowFind = 238,
        ChartTimePriceToXY = 239,
        ChartXYToTimePrice = 240,
        ChartOpen = 241,
        ChartFirst = 242,
        ChartNext = 243,
        ChartClose = 244,
        ChartSymbol = 245,
        ChartPeriod = 246,
        ChartSetDouble = 247,
        ChartSetInteger = 248,
        ChartSetString = 249,
        ChartGetDouble = 250,
        ChartGetInteger = 251,
        ChartGetString = 252,
        ChartNavigate = 253,
        ChartIndicatorDelete = 254,
        ChartIndicatorName = 255,
        ChartIndicatorsTotal = 256,
        ChartWindowOnDropped = 257,
        ChartPriceOnDropped = 258,
        ChartTimeOnDropped = 259,
        ChartXOnDropped = 260,
        ChartYOnDropped = 261,
        ChartSetSymbolPeriod = 262,
        ChartScreenShot = 263,
        
        // Windows Operations
        WindowBarsPerChart = 264,
        WindowExpertName = 265,
        WindowFind = 266,
        WindowFirstVisibleBar = 267,
        WindowHandle = 268,
        WindowIsVisible = 269,
        WindowOnDropped = 270,
        WindowPriceMax = 271,
        WindowPriceMin = 272,
        WindowPriceOnDropped = 273,
        WindowRedraw = 274,
        WindowScreenShot = 275,
        WindowTimeOnDropped = 276,
        WindowsTotal = 277,
        WindowXOnDropped = 278,
        WindowYOnDropped = 279,

        // Terminal Operations
        TerminalCompany = 68,
        TerminalName = 69,
        TerminalPath = 70,

        //Checkup
        GetLastError                        = 132,
        TerminalInfoString                  = 153, //TODO
        TerminalInfoInteger                 = 204, //TODO
        TerminalInfoDouble                  = 205, //TODO

        //Common Functions
        Alert                               = 136,  //TODO
        Comment                             = 137,  //TODO
        GetTickCount                        = 138,  //TODO
        GetMicrosecondCount                 = 139,  //TODO
        MessageBox                          = 140,  //TODO
        PeriodSeconds                       = 141,  //TODO
        PlaySound                           = 142,  //TODO
        Print                               = 68,
        ResetLastError                      = 143,
        SendNotification                    = 144,  //TODO
        SendMail                            = 145   //TODO
    }
}
