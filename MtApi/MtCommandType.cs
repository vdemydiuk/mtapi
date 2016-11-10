﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtApi
{
    enum MtCommandType
    {
        //NoCommand = 0

        //trade operations
//        OrderSend                           = 1,
//        OrderSendBuy                        = 1001,
//        OrderSendSell                       = 1002,
//        OrderClose                          = 2,
//        OrderCloseByCurrentPrice            = 152,

//        OrderCloseBy                        = 3,
        OrderClosePrice                     = 4,
        OrderClosePriceByTicket             = 1004,
        OrderCloseTime                      = 5,
        OrderCloseAll                       = 151,
        OrderComment                        = 6,
        OrderCommission                     = 7,
        OrderDelete                         = 8,
        OrderExpiration                     = 9,
        OrderLots                           = 10,
        OrderMagicNumber                    = 11,
        OrderModify                         = 12,
        OrderOpenPrice                      = 13,
        OrderOpenPriceByTicket              = 1013,
        OrderOpenTime                       = 14,
        OrderPrint                          = 15,
        OrderProfit                         = 16,
        OrderSelect                         = 17,
        OrdersHistoryTotal                  = 18,
        OrderStopLoss                       = 19,
        OrdersTotal                         = 20,
        OrderSwap                           = 21,
        OrderSymbol                         = 22,
        OrderTakeProfit                     = 23,
        OrderTicket                         = 24,
        OrderType                           = 25,

        //Check Status
        GetLastError                        = 26,
        IsConnected                         = 27,
        IsDemo                              = 28,
        IsDllsAllowed                       = 29,
        IsExpertEnabled                     = 30,
        IsLibrariesAllowed                  = 31,
        IsOptimization                      = 32,
        IsStopped                           = 33,
        IsTesting                           = 34,
        IsTradeAllowed                      = 35,
        IsTradeContextBusy                  = 36,
        IsVisualMode                        = 37,
        UninitializeReason                  = 38,
        ErrorDescription                    = 39,

        //Account Information
        AccountBalance                      = 40,
        AccountCredit                       = 41,
        AccountCompany                      = 42,
        AccountCurrency                     = 43,
        AccountEquity                       = 44,
        AccountFreeMargin                   = 45,
        AccountFreeMarginCheck              = 46,
        AccountFreeMarginMode               = 47,
        AccountLeverage                     = 48,
        AccountMargin                       = 49,
        AccountName                         = 50,
        AccountNumber                       = 51,
        AccountProfit                       = 52,
        AccountServer                       = 53,
        AccountStopoutLevel                 = 54,
        AccountStopoutMode                  = 55,

        //Common Commands
        Alert                               = 56,
        Comment                             = 57,
        GetTickCount                        = 58,
        MarketInfo                          = 59,
        MessageBox                          = 60,
        MessageBoxA                         = 61,
        PlaySound                           = 62,
        Print                               = 63,
        SendFTP                             = 64,
        SendFTPA                            = 65,
        SendMail                            = 66,
        Sleep                               = 67,

        //Client Terminal
        TerminalCompany                     = 68,
        TerminalName                        = 69,
        TerminalPath                        = 70,

        //Date and Time
        Day                                 = 71,
        DayOfWeek                           = 72,
        DayOfYear                           = 73,
        Hour                                = 74,
        Minute                              = 75,
        Month                               = 76,
        Seconds                             = 77,
        TimeCurrent                         = 78,
        TimeDay                             = 79,
        TimeDayOfWeek                       = 80,
        TimeDayOfYear                       = 81,
        TimeHour                            = 82,
        TimeLocal                           = 83,
        TimeMinute                          = 84,
        TimeMonth                           = 85,
        TimeSeconds                         = 86,
        TimeYear                            = 87,
        Year                                = 88,

        //Global Variables
        GlobalVariableCheck                 = 89,
        GlobalVariableDel                   = 90,
        GlobalVariableGet                   = 91,
        GlobalVariableName                  = 92,
        GlobalVariableSet                   = 93,
        GlobalVariableSetOnCondition        = 94,
        GlobalVariablesDeleteAll            = 95,
        GlobalVariablesTotal                = 96,

        //Technical Indicators
        iAC                                 = 97,
        iAD                                 = 98,
        iAlligator                          = 99,
        iADX                                = 100,
        iATR                                = 101,
        iAO                                 = 102,
        iBearsPower                         = 103,
        iBands                              = 104,
        iBandsOnArray                       = 105,
        iBullsPower                         = 106,
        iCCI                                = 107,
        iCCIOnArray                         = 108,
        iCustom                             = 109,
        iCustom_d                           = 10109,
        iDeMarker                           = 110,
        iEnvelopes                          = 111,
        iEnvelopesOnArray                   = 112,
        iForce                              = 113,
        iFractals                           = 114,
        iGator                              = 115,
        iIchimoku                           = 116,
        iBWMFI                              = 117,
        iMomentum                           = 118,
        iMomentumOnArray                    = 119,
        iMFI                                = 120,
        iMA                                 = 121,
        iMAOnArray                          = 122,
        iOsMA                               = 123,
        iMACD                               = 124,
        iOBV                                = 125,
        iSAR                                = 126,
        iRSI                                = 127,
        iRSIOnArray                         = 128,
        iRVI                                = 129,
        iStdDev                             = 130,
        iStdDevOnArray                      = 131,
        iStochastic                         = 132,
        iWPR                                = 133,

        //Timeseries access
        iBars                               = 134,
        iBarShift                           = 135,
        iClose                              = 136,
        iHigh                               = 137,
        iHighest                            = 138,
        iLow                                = 139,
        iLowest                             = 140,
        iOpen                               = 141,
        iTime                               = 142,
        iVolume                             = 143,

        iCloseArray                         = 144,
        iHighArray                          = 145,
        iLowArray                           = 146,
        iOpenArray                          = 147,
        iVolumeArray                        = 148,
        iTimeArray                          = 149,

        //
        RefreshRates                        = 150,
        //
        TerminalInfoString                  = 153,
        SymbolInfoString                    = 154,

        //Requests
        MtRequest                           = 155,

        //Backtesting
        BacktestingReady                    = 156,

        //Symbols
        SymbolsTotal                        = 200,
        SymbolName                          = 201,
        SymbolSelect                        = 202,
        SymbolInfoInteger                   = 203
    }
}
