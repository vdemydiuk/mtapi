#property copyright "Vyacheslav Demidyuk"
#property link      ""

#property version   "1.4"
#property description "MtApi (MT5) connection expert"

#include <json.mqh>
#include <Trade\SymbolInfo.mqh>
#include <trade/trade.mqh>

#import "MT5Connector.dll"
   bool initExpert(int expertHandle, int port, string symbol, double bid, double ask, int isTestMode, string& err);
   bool deinitExpert(int expertHandle, string& err);
   
   bool updateQuote(int expertHandle, string symbol, double bid, double ask, string& err);
      
   bool sendIntResponse(int expertHandle, int response, string& err);
   bool sendBooleanResponse(int expertHandle, int response, string& err);
   bool sendDoubleResponse(int expertHandle, double response, string& err);
   bool sendStringResponse(int expertHandle, string response, string& err);
   bool sendVoidResponse(int expertHandle, string& err);
   bool sendDoubleArrayResponse(int expertHandle, double& values[], int size, string& err);
   bool sendIntArrayResponse(int expertHandle, int& values[], int size, string& err);   
   bool sendLongResponse(int expertHandle, long response, string& err);
   bool sendULongResponse(int expertHandle, ulong response, string& err);
   bool sendLongArrayResponse(int expertHandle, long& values[], int size, string& err);
   bool sendMqlRatesArrayResponse(int expertHandle, MqlRates& values[], int size, string& err);   
   bool sendMqlTickResponse(int expertHandle, MqlTick& response, string& err);
   bool sendMqlBookInfoArrayResponse(int expertHandle, MqlBookInfo& values[], int size, string& err);   
   bool sendErrorResponse(int expertHandle, int code, string message, string& err);
   
   bool getCommandType(int expertHandle, int& res, string& err);
   bool getIntValue(int expertHandle, int paramIndex, int& res, string& err);
   bool getUIntValue(int expertHandle, int paramIndex, uint& res, string& err);   
   bool getULongValue(int expertHandle, int paramIndex, ulong& res, string& err);
   bool getLongValue(int expertHandle, int paramIndex, long& res, string& err);
   bool getDoubleValue(int expertHandle, int paramIndex, double& res, string& err);
   bool getStringValue(int expertHandle, int paramIndex, string& res, string& err);
   bool getBooleanValue(int expertHandle, int paramIndex, bool& res, string& err);
#import

//#define __DEBUG_LOG__

input int Port = 8228;

int ExpertHandle;

string _error;
string _response_error;
bool isCrashed = false;

bool IsRemoteReadyForTesting = false;

string PARAM_SEPARATOR = ";";

int OnInit()
{
   int result = init();  
   return (result);
}

double OnTester()
{
    Print("OnTester");
    return 0;
}

void OnDeinit(const int reason)
{
   deinit();
}

void OnTick()
{
   start();
   if (IsTesting()) OnTimer();
}

int preinit()
{
   StringInit(_response_error,1000,0);

   return (0);
}

bool IsDemo()
{
   if(AccountInfoInteger(ACCOUNT_TRADE_MODE)==ACCOUNT_TRADE_MODE_DEMO)
      return(true);
   else
      return(false);
}

bool IsTesting()
{  
   bool isTesting = MQLInfoInteger(MQL_TESTER);
   return isTesting;
}

int init() 
{
   preinit();  

   if (TerminalInfoInteger(TERMINAL_DLLS_ALLOWED) == false) 
   {
      MessageBox("Dlls not allowed.", "MtApi", 0);
      isCrashed = true;
      return (1);
   }
   if (MQL5InfoInteger(MQL5_DLLS_ALLOWED) == false) 
   {
      MessageBox("Libraries not allowed.", "MtApi", 0);
      isCrashed = true;
      return (1);
   }

   if (MQL5InfoInteger(MQL5_TRADE_ALLOWED) == false) 
   {
      MessageBox("Trade not allowed.", "MtApi", 0);
      isCrashed = true;
      return (1);
   }

   long chartID = ChartID();
   ExpertHandle = (int) ChartGetInteger(chartID, CHART_WINDOW_HANDLE);
   
   MqlTick last_tick;
   SymbolInfoTick(Symbol(),last_tick);
   double Bid = last_tick.bid;
   double Ask = last_tick.ask;
   
   if (!initExpert(ExpertHandle, Port, Symbol(), Bid, Ask, IsTesting(), _error))
   {
       MessageBox(_error, "MtApi", 0);
       isCrashed = true;
       return(1);
   }
   
   if (executeCommand() == 1)
   {   
      isCrashed = true;
      return (1);
   }
   
#ifdef __DEBUG_LOG__
   PrintFormat("Expert Handle = %d", ExpertHandle);
   PrintFormat("IsTesting: %s", IsTesting() ? "true" : "false");
#endif
   
   //--- Backtesting mode
    if (IsTesting())
    {      
       Print("Waiting on remote client...");
       //wait for command (BacktestingReady) from remote side to be ready for work
       while(!IsRemoteReadyForTesting)
       {
          executeCommand();
          
          //This section uses a while loop to simulate Sleep() during Backtest.
          unsigned int viSleepUntilTick = GetTickCount() + 100; //100 milliseconds
          while(GetTickCount() < viSleepUntilTick) 
          {
             //Do absolutely nothing. Just loop until the desired tick is reached.
          }
       }
    }
   //--- 

   return (0);
}

int deinit() 
{
   if (isCrashed == 0) 
   {
      if (!deinitExpert(ExpertHandle, _error)) 
      {
         MessageBox(_error, "MtApi", 0);
         isCrashed = true;
         return (1);
      }
      Print("Expert was deinitialized.");
   }
   
   return (0);
}

int start() 
{
   MqlTick last_tick;
   SymbolInfoTick(Symbol(),last_tick);
   double Bid = last_tick.bid;
   double Ask = last_tick.ask;
   
   if (!updateQuote(ExpertHandle, Symbol(), Bid, Ask, _error)) 
   {
      Print("updateQuote: [ERROR] ", _error);
   }

   return (0);
}

void OnTimer()
{
   while(true)
   {
      int executedCommand = executeCommand();
      if (executedCommand == 0)
      {   
         return;
      }
   }
}


int executeCommand()
{
   int commandType = 0;

   if (!getCommandType(ExpertHandle, commandType, _error))
   {
      Print("[ERROR] ExecuteCommand: Failed to get command type! ", _error);
      return (0);
   }
   
#ifdef __DEBUG_LOG__
   if (commandType > 0)
   {
      Print("executeCommand: commnad type = ", commandType);
   }
#endif 
  
   switch (commandType) 
   {
   case 0:
      //NoCommand         
      break;
   case 155: //Request
      Execute_Request();
   break;
   case 1: // OrderSend
      Execute_OrderSend();
   break;
   case 63: //OrderCloseAll
      Execute_OrderCloseAll();
   break;
   case 64: //PositionClose
      Execute_PositionClose();
   break;
   case 2: // OrderCalcMargin
      Execute_OrderCalcMargin();
   break;
   case 3: //OrderCalcProfit
      Execute_OrderCalcProfit();
   break;
   case 4: //OrderCheck
      Execute_OrderCheck();
   break;
   case 6: //PositionsTotal
      Execute_PositionsTotal();
   break;
   case 7: //PositionGetSymbol
      Execute_PositionGetSymbol();
   break;
   case 8: //PositionSelect
      Execute_PositionSelect();
   break;
   case 9: //PositionGetDouble
      Execute_PositionGetDouble();
   break;
   case 10: //PositionGetInteger
      Execute_PositionGetInteger();
   break;
   case 11: //PositionGetString
      Execute_PositionGetString();
   break;
   case 12: //OrdersTotal
      Execute_OrdersTotal();
   break;
   case 13: //OrderGetTicket
      Execute_OrderGetTicket();
   break;
   case 14: //OrderSelect
      Execute_OrderSelect();
   break;
   case 15: //OrderGetDouble
      Execute_OrderGetDouble();
   break;
   case 16: //OrderGetInteger
      Execute_OrderGetInteger();
   break;
   case 17: //OrderGetString
      Execute_OrderGetString();
   break;
   case 18: //HistorySelect
      Execute_HistorySelect();
   break;
   case 19: //HistorySelectByPosition
      Execute_HistorySelectByPosition();
   break;
   case 20: //HistoryOrderSelect
      Execute_HistoryOrderSelect();
   break;
   case 21: //HistoryOrdersTotal
      Execute_HistoryOrdersTotal();
   break;
   case 22: //HistoryOrderGetTicket
      Execute_HistoryOrderGetTicket();
   break;
   case 23: //HistoryOrderGetDouble
      Execute_HistoryOrderGetDouble();
   break;
   case 24: //HistoryOrderGetInteger
      Execute_HistoryOrderGetInteger();
   break;
   case 25: //HistoryOrderGetString
      Execute_HistoryOrderGetString();
   break;
   case 26: //HistoryDealSelect
      Execute_HistoryDealSelect();
   break;
   case 27: //HistoryDealsTotal
      Execute_HistoryDealsTotal();
   break;
   case 28: //HistoryDealGetTicket
      Execute_HistoryDealGetTicket();
   break;
   case 29: //HistoryDealGetDouble
      Execute_HistoryDealGetDouble();
   break;
   case 30: //HistoryDealGetInteger
      Execute_HistoryDealGetInteger();
   break;      
   case 31: //HistoryDealGetString
      Execute_HistoryDealGetString();
   break;
   case 32: //AccountInfoDouble
      Execute_AccountInfoDouble();
   break;
   case 33: //AccountInfoInteger
      Execute_AccountInfoInteger();
   break;
   case 34: //AccountInfoString
      Execute_AccountInfoString();
   break;    
   case 35: //SeriesInfoInteger
      Execute_SeriesInfoInteger();
   break;    
   case 36: //Bars
      Execute_Bars();
   break;    
   case 1036: //Bars2
      Execute_Bars2();
   break;       
   case 37: //BarsCalculated
      Execute_BarsCalculated();
   break;    
   case 40: //CopyBuffer
      Execute_CopyBuffer();
   break;    
   case 1040: //CopyBuffer1
      Execute_CopyBuffer1();
   break;    
   case 1140: //CopyBuffer2
      Execute_CopyBuffer2();
   break;   
   case 41: //CopyRates
      Execute_CopyRates();
   break;    
   case 1041: //CopyRates1
      Execute_CopyRates1();
   break;    
   case 1141: //CopyRates2
      Execute_CopyRates2();
   break;   
   case 42: //CopyTime
      Execute_CopyTime();
   break;   
   case 1042: //CopyTime1
      Execute_CopyTime1();
   break;   
   case 1142: //CopyTime2
      Execute_CopyTime2();
   break;   
   case 43: //CopyOpen
      Execute_CopyOpen();
   break;      
   case 1043: //CopyOpen1
      Execute_CopyOpen1();
   break;        
   case 1143: //CopyOpen2
      Execute_CopyOpen2();
   break;      
   case 44: //CopyHigh
      Execute_CopyHigh();
   break;     
   case 1044: //CopyHigh1
      Execute_CopyHigh1();
   break;       
   case 1144: //CopyHigh2
      Execute_CopyHigh2();
   break;      
   case 45: //CopyLow
      Execute_CopyLow();
   break;        
   case 1045: //CopyLow1
      Execute_CopyLow1();
   break;       
   case 1145: //CopyLow2
      Execute_CopyLow2();
   break;    
   case 46: //CopyClose
      Execute_CopyClose();
   break;     
   case 1046: //CopyClose1
      Execute_CopyClose1();
   break;      
   case 1146: //CopyClose2
      Execute_CopyClose2();
   break;    
   case 47: //CopyTickVolume
      Execute_CopyTickVolume();
   break;    
   case 1047: //CopyTickVolume1
      Execute_CopyTickVolume1();
   break;          
   case 1147: //CopyTickVolume2
      Execute_CopyTickVolume2();
   break;   
   case 48: //CopyRealVolume
      Execute_CopyRealVolume();
   break;    
   case 1048: //CopyRealVolume1
      Execute_CopyRealVolume1();
   break;      
   case 1148: //CopyRealVolume2
      Execute_CopyRealVolume2();
   break;             
   case 49: //CopySpread
      Execute_CopySpread();
   break;    
   case 1049: //CopySpread1
      Execute_CopySpread1();
   break;      
   case 1149: //CopySpread2
      Execute_CopySpread2();
   break;       
   case 50: //SymbolsTotal
      Execute_SymbolsTotal();
   break;     
   case 51: //SymbolName
      Execute_SymbolName();
   break;        
   case 52: //SymbolSelect
      Execute_SymbolSelect();
   break;     
   case 53: //SymbolIsSynchronized
      Execute_SymbolIsSynchronized();
   break;      
   case 54: //SymbolInfoDouble
      Execute_SymbolInfoDouble();
   break;   
   case 55: //SymbolInfoInteger
      Execute_SymbolInfoInteger();
   break;   
   case 56: //SymbolInfoString
      Execute_SymbolInfoString();
   break;    
   case 57: //SymbolInfoTick
      Execute_SymbolInfoTick();
   break;
   case 58: //SymbolInfoSessionQuote
      Execute_SymbolInfoSessionQuote();
   break;     
   case 59: //SymbolInfoSessionTrade
      Execute_SymbolInfoSessionTrade();
   break;    
   case 60: //MarketBookAdd
      Execute_MarketBookAdd();
   break;    
   case 61: //MarketBookRelease
      Execute_MarketBookRelease();
   break;    
//   case 62: //MarketBookGet
//   break;
   case 65: //PositionOpen
      Execute_PositionOpen(false);
   break;
//   case 1065: //PositionOpenWithResult
//      Execute_PositionOpen(true);
//   break;   
   case 66: //BacktestingReady
      Execute_BacktestingReady();
   break;
   case 67: //IsTesting
      Execute_IsTesting();
   break;   
   case 68: //Print
      Execute_Print();
   break;   
   case 69: //PositionSelectByTicket
      Execute_PositionSelectByTicket();
   break;
   case 70: //ObjectCreate
      Execute_ObjectCreate();
   break;
   case 71: //ObjectName
      Execute_ObjectName();
   break;
   case 72: //ObjectDelete
      Execute_ObjectDelete();
   break;
   case 73: //ObjectsDeleteAll
      Execute_ObjectsDeleteAll();
   break;
   case 74: //ObjectFind
      Execute_ObjectFind();
   break;
   case 75: //ObjectGetTimeByValue
      Execute_ObjectGetTimeByValue();
   break;
   case 76: //ObjectGetValueByTime
      Execute_ObjectGetValueByTime();
   break;
   case 77: //ObjectMove
      Execute_ObjectMove();
   break;
   case 78: //ObjectsTotal
      Execute_ObjectsTotal();
   break;
   case 79: //ObjectGetDouble
      Execute_ObjectGetDouble();
   break;
   case 80: //ObjectGetInteger
      Execute_ObjectGetInteger();
   break;
   case 81: //ObjectGetString
      Execute_ObjectGetString();
   break;
   case 82: //ObjectSetDouble
      Execute_ObjectSetDouble();
   break;
   case 83: //ObjectSetInteger
      Execute_ObjectSetInteger();
   break;
   case 84: //ObjectSetString
      Execute_ObjectSetString();
   break;
   case 88: //iAC
      Execute_iAC();
   break;
   case 89: //iAD
      Execute_iAD();
   break;
   case 90: //iADX
      Execute_iADX();
   break;
   case 91: //iADXWilder
      Execute_iADXWilder();
   break;
   case 92: //iAlligator
      Execute_iAlligator();
   break;
   case 93: //iAMA
      Execute_iAMA();
   break;
   case 94: //iAO
      Execute_iAO();
   break;
   case 95: //iATR
      Execute_iATR();
   break;
   case 96: //iBearsPower
      Execute_iBearsPower();
   break;
   case 97: //iBands
      Execute_iBands();
   break;
   case 98: //iBullsPower
      Execute_iBullsPower();
   break;
   case 99: //iCCI
      Execute_iCCI();
   break;
   case 100: //iChaikin
      Execute_iChaikin();
   break;
//   case 101: //iCustom
//   break;
   case 102: //iDEMA
      Execute_iDEMA();
   break;
   case 103: //iDeMarker
      Execute_iDeMarker();
   break;
   case 104: //iEnvelopes
      Execute_iEnvelopes();
   break;
   case 105: //iForce
      Execute_iForce();
   break;
   case 106: //iFractals
      Execute_iFractals();
   break;
   case 107: //iFrAMA
      Execute_iFrAMA();
   break;
   case 108: //iGator
      Execute_iGator();
   break;
   case 109: //iIchimoku
      Execute_iIchimoku();
   break;
   case 110: //iBWMFI
      Execute_iBWMFI();
   break;
   case 111: //iMomentum
      Execute_iMomentum();
   break;
   case 112: //iMFI
      Execute_iMFI();
   break;
   case 113: //iMA
      Execute_iMA();
   break;
   case 114: //iOsMA
      Execute_iOsMA();
   break;
   case 115: //iMACD
      Execute_iMACD();
   break;
   case 116: //iOBV
      Execute_iOBV();
   break;
   case 117: //iSAR
      Execute_iSAR();
   break;
   case 118: //iRSI
      Execute_iRSI();
   break;
   case 119: //iRVI
      Execute_iRVI();
   break;
   case 120: //iStdDev
      Execute_iStdDev();
   break;
   case 121: //iStochastic
      Execute_iStochastic();
   break;
   case 122: //iTEMA
      Execute_iTEMA();
   break;
   case 123: //iTriX
      Execute_iTriX();
   break;
   case 124: //iWPR
      Execute_iWPR();
   break;
   case 125: //iVIDyA
      Execute_iVIDyA();
   break;
   case 126: //iVolumes
      Execute_iVolumes();
   break;
   case 127: //TimeCurrent
      Execute_TimeCurrent();
   break;
   case 128: //TimeTradeServer
      Execute_TimeTradeServer();
   break;
   case 129: //TimeLocal
      Execute_TimeLocal();
   break;
   case 130: //TimeGMT
      Execute_TimeGMT();
   break;
   case 131: //IndicatorRelease
      Execute_IndicatorRelease();
   break;
   default:
      Print("Unknown command type = ", commandType);
      sendVoidResponse(ExpertHandle, _response_error);
      break;
   } 
   
   return (commandType);
}

void Execute_Request()
{
   string request;
   StringInit(request, 1000, 0);
   
   if (!getStringValue(ExpertHandle, 0, request, _error))
   {
      PrintParamError("Request", "Request", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
      
   string response = "";
   if (request != "")
   {
#ifdef __DEBUG_LOG__
      Print("Execute_Request: incoming request = ", request);
#endif
      response = OnRequest(request);
   }
   
   if (!sendStringResponse(ExpertHandle, response, _response_error))
   {
      PrintResponseError("Request", _response_error);
   }
}

void Execute_OrderSend()
{
   MqlTradeRequest request={0};      
   ReadMqlTradeRequestFromCommand(request);
   
   MqlTradeResult result={0};
   
   bool retVal = OrderSend(request, result);
         
   if (!sendStringResponse(ExpertHandle, ResultToString(retVal, result), _response_error))
   {
      PrintResponseError("OrderSend", _response_error);
   }
}

void Execute_OrderCloseAll()
{
   if (!sendBooleanResponse(ExpertHandle, OrderCloseAll(), _response_error))
   {
      PrintResponseError("OrderCloseAll", _response_error);
   }
}

void Execute_PositionClose()
{
   ulong ticket;
   ulong deviation;
   CTrade trade;
      
   if (!getULongValue(ExpertHandle, 0, ticket, _error))
   {
      PrintParamError("PositionClose", "ticket", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getULongValue(ExpertHandle, 1, deviation, _error))
   {
      PrintParamError("PositionClose", "deviation", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
      
   if (!sendBooleanResponse(ExpertHandle, trade.PositionClose(ticket, deviation), _response_error))
   {
      PrintResponseError("PositionClose", _response_error);
   }
}

void Execute_OrderCalcMargin()
{
   int action;
   string symbol;
   double volume;
   double price;
   StringInit(symbol, 100, 0);
   
   if (!getIntValue(ExpertHandle, 0, action, _error))
   {
      PrintParamError("OrderCalcMargin", "action", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 1, symbol, _error))
   {
      PrintParamError("OrderCalcMargin", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 2, volume, _error))
   {
      PrintParamError("OrderCalcMargin", "volume", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 3, price, _error))
   {
      PrintParamError("OrderCalcMargin", "price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   double margin;
   bool retVal = OrderCalcMargin((ENUM_ORDER_TYPE)action, symbol, volume, price, margin);
               
   if (!sendStringResponse(ExpertHandle, ResultToString(retVal, margin), _response_error))
   {
      PrintResponseError("OrderCalcMargin", _response_error);
   }
}

void Execute_OrderCalcProfit()
{
   int action;
   string symbol;
   double volume;
   double price_open;            
   double price_close;
   StringInit(symbol, 100, 0);
   
   if (!getIntValue(ExpertHandle, 0, action, _error))
   {
      PrintParamError("OrderCalcProfit", "action", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }            
   if (!getStringValue(ExpertHandle, 1, symbol, _error))
   {
      PrintParamError("OrderCalcProfit", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 2, volume, _error))
   {
      PrintParamError("OrderCalcProfit", "volume", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 3, price_open, _error))
   {
      PrintParamError("OrderCalcProfit", "price_open", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 4, price_close, _error))
   {
      PrintParamError("OrderCalcProfit", "price_close", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   double profit;
   bool retVal = OrderCalcProfit((ENUM_ORDER_TYPE)action, symbol, volume, price_open, price_close, profit);
            
   if (!sendStringResponse(ExpertHandle, ResultToString(retVal, profit), _response_error))
   {
      PrintResponseError("OrderCalcProfit", _response_error);
   }
}

void Execute_OrderCheck()
{
   MqlTradeRequest request={0};      
   ReadMqlTradeRequestFromCommand(request);
   
   MqlTradeCheckResult result={0};
   
   bool retVal = OrderCheck(request, result);
         
   if (!sendStringResponse(ExpertHandle, ResultToString(retVal, result), _response_error))
   {
      PrintResponseError("OrderCheck", _response_error);
   }
}

void Execute_PositionsTotal()
{
   if (!sendIntResponse(ExpertHandle, PositionsTotal(), _response_error))
   {
      PrintResponseError("PositionsTotal", _response_error);
   }
}

void Execute_PositionGetSymbol()
{
   int index;
   if (!getIntValue(ExpertHandle, 0, index, _error))
   {
      PrintParamError("PositionGetSymbol", "index", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendStringResponse(ExpertHandle, PositionGetSymbol(index), _response_error))
   {
      PrintResponseError("PositionGetSymbol", _response_error);
   }
}

void Execute_PositionSelect()
{
   string symbol;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("PositionSelect", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle, PositionSelect(symbol), _response_error))
   {
      PrintResponseError("PositionSelect", _response_error);
   }
}

void Execute_PositionGetDouble()
{
   int property_id;
   
   if (!getIntValue(ExpertHandle, 0, property_id, _error))
   {
      PrintParamError("PositionGetDouble", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendDoubleResponse(ExpertHandle, PositionGetDouble((ENUM_POSITION_PROPERTY_DOUBLE)property_id), _response_error))
   {
      PrintResponseError("PositionGetDouble", _response_error);
   }
}

void Execute_PositionGetInteger()
{
   int property_id;
   
   if (!getIntValue(ExpertHandle, 0, property_id, _error))
   {
      PrintParamError("PositionGetInteger", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;      
   }

   if (!sendLongResponse(ExpertHandle, PositionGetInteger((ENUM_POSITION_PROPERTY_INTEGER)property_id), _response_error))
   {
      PrintResponseError("PositionGetInteger", _response_error);
   }
}

void Execute_PositionGetString()
{
   int property_id;
   
   if (!getIntValue(ExpertHandle, 0, property_id, _error))
   {
      PrintParamError("PositionGetString", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;      
   }

   if (!sendStringResponse(ExpertHandle, PositionGetString((ENUM_POSITION_PROPERTY_STRING)property_id), _response_error))
   {
      PrintResponseError("PositionGetString", _response_error);
   }
}

void Execute_OrdersTotal()
{
   if (!sendIntResponse(ExpertHandle, OrdersTotal(), _response_error))
   {
      PrintResponseError("OrdersTotal", _response_error);
   }
}

void Execute_OrderGetTicket()
{
   int index;
   if (!getIntValue(ExpertHandle, 0, index, _error))
   {
      PrintParamError("OrderGetTicket", "index", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendULongResponse(ExpertHandle, OrderGetTicket(index), _response_error))
   {
      PrintResponseError("OrderGetTicket", _response_error);
   }
}

void Execute_OrderSelect()
{
   ulong ticket;
   if (!getULongValue(ExpertHandle, 0, ticket, _error))
   {
      PrintParamError("OrderSelect", "ticket", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle, OrderSelect(ticket), _response_error))
   {
      PrintResponseError("OrderSelect", _response_error);
   }
}

void Execute_OrderGetDouble()
{
   int property_id;
   if (!getIntValue(ExpertHandle, 0, property_id, _error))
   {
      PrintParamError("OrderGetDouble", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendDoubleResponse(ExpertHandle, OrderGetDouble((ENUM_ORDER_PROPERTY_DOUBLE)property_id), _response_error))
   {
      PrintResponseError("OrderGetDouble", _response_error);
   }
}

void Execute_OrderGetInteger()
{
   int property_id;

   if (!getIntValue(ExpertHandle, 0, property_id, _error))
   {
      PrintParamError("OrderGetInteger", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendLongResponse(ExpertHandle, OrderGetInteger((ENUM_ORDER_PROPERTY_INTEGER)property_id), _response_error))
   {
      PrintResponseError("OrderGetInteger", _response_error);
   }
}

void Execute_OrderGetString()
{
   int property_id;

   if (!getIntValue(ExpertHandle, 0, property_id, _error))
   {
      PrintParamError("OrderGetString", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendStringResponse(ExpertHandle, OrderGetString((ENUM_ORDER_PROPERTY_STRING)property_id), _response_error))
   {
      PrintResponseError("OrderGetString", _response_error);
   }
}

void Execute_HistorySelect()
{
   int from_date;
   int to_date;

   if (!getIntValue(ExpertHandle, 0, from_date, _error))
   {
      PrintParamError("HistorySelect", "from_date", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!getIntValue(ExpertHandle, 1, to_date, _error))
   {
      PrintParamError("HistorySelect", "to_date", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle, HistorySelect((datetime)from_date, (datetime)to_date), _response_error))
   {
      PrintResponseError("HistorySelect", _response_error);
   }
}

void Execute_HistorySelectByPosition()
{
   long position_id;
   if (!getLongValue(ExpertHandle, 0, position_id, _error))
   {
      PrintParamError("HistorySelectByPosition", "position_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle, HistorySelectByPosition(position_id), _response_error))
   {
      PrintResponseError("HistorySelectByPosition", _response_error);
   }
}

void Execute_HistoryOrderSelect()
{
   ulong ticket;
   if (!getULongValue(ExpertHandle, 0, ticket, _error))
   {
      PrintParamError("HistoryOrderSelect", "ticket", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle, HistoryOrderSelect(ticket), _response_error))
   {
      PrintResponseError("HistoryOrderSelect", _response_error);
   }
}

void Execute_HistoryOrdersTotal()
{
   if (!sendIntResponse(ExpertHandle, HistoryOrdersTotal(), _response_error))
   {
      PrintResponseError("HistoryOrdersTotal", _response_error);
   }
}

void Execute_HistoryOrderGetTicket()
{
   int index;
   if (!getIntValue(ExpertHandle, 0, index, _error))
   {
      PrintParamError("HistoryOrderGetTicket", "index", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;   
   }
   
   if (!sendULongResponse(ExpertHandle, HistoryOrderGetTicket(index), _response_error))
   {
      PrintResponseError("HistoryOrderGetTicket", _response_error);
   }
}

void Execute_HistoryOrderGetDouble()
{
   ulong ticket_number;
   int property_id;
   
   if (!getULongValue(ExpertHandle, 0, ticket_number, _error))
   {
      PrintParamError("HistoryOrderGetDouble", "ticket_number", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, property_id, _error))
   {
      PrintParamError("HistoryOrderGetDouble", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendDoubleResponse(ExpertHandle, HistoryOrderGetDouble(ticket_number, (ENUM_ORDER_PROPERTY_DOUBLE)property_id), _response_error))
   {
      PrintResponseError("HistoryOrderGetDouble", _response_error);
   }
}

void Execute_HistoryOrderGetInteger()
{
   ulong ticket_number;
   int property_id;
   
   if (!getULongValue(ExpertHandle, 0, ticket_number, _error))
   {
      PrintParamError("HistoryOrderGetInteger", "ticket_number", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, property_id, _error))
   {
      PrintParamError("HistoryOrderGetInteger", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendLongResponse(ExpertHandle, HistoryOrderGetInteger(ticket_number, (ENUM_ORDER_PROPERTY_INTEGER)property_id), _response_error))
   {
      PrintResponseError("HistoryOrderGetInteger", _response_error);
   }
}

void Execute_HistoryOrderGetString()
{
   ulong ticket_number;
   int property_id;
   
   if (!getULongValue(ExpertHandle, 0, ticket_number, _error))
   {
      PrintParamError("HistoryOrderGetString", "ticket_number", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, property_id, _error))
   {
      PrintParamError("HistoryOrderGetString", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendStringResponse(ExpertHandle, HistoryOrderGetString(ticket_number, (ENUM_ORDER_PROPERTY_STRING)property_id), _response_error))
   {
      PrintResponseError("HistoryOrderGetString", _response_error);
   }
}

void Execute_HistoryDealSelect()
{
   ulong ticket;
         
   if (!getULongValue(ExpertHandle, 0, ticket, _error))
   {
      PrintParamError("HistoryDealSelect", "ticket", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle, HistoryDealSelect(ticket), _response_error))
   {
      PrintResponseError("HistoryDealSelect", _response_error);
   }
}

void Execute_HistoryDealsTotal()
{
   if (!sendIntResponse(ExpertHandle, HistoryDealsTotal(), _response_error))
   {
      PrintResponseError("HistoryDealsTotal", _response_error);
   }
}

void Execute_HistoryDealGetTicket()
{
   uint index;
   
   if (!getIntValue(ExpertHandle, 0, index, _error))
   {
      PrintParamError("HistoryDealGetTicket", "index", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;   
   }
   
   if (!sendULongResponse(ExpertHandle, HistoryDealGetTicket(index), _response_error))
   {
      PrintResponseError("HistoryDealGetTicket", _response_error);
   }
}

void Execute_HistoryDealGetDouble()
{
   ulong ticket_number;
   int property_id;
   
   if (!getULongValue(ExpertHandle, 0, ticket_number, _error))
   {
      PrintParamError("HistoryDealGetDouble", "ticket_number", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;    
   }
   if (!getIntValue(ExpertHandle, 1, property_id, _error))
   {
      PrintParamError("HistoryDealGetDouble", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;    
   }
   
   if (!sendDoubleResponse(ExpertHandle, HistoryDealGetDouble(ticket_number, (ENUM_DEAL_PROPERTY_DOUBLE)property_id), _response_error))
   {
      PrintResponseError("HistoryDealGetDouble", _response_error);
   }
}

void Execute_HistoryDealGetInteger()
{
   ulong ticket_number;
   int property_id;
   
   if (!getULongValue(ExpertHandle, 0, ticket_number, _error))
   {
      PrintParamError("HistoryDealGetInteger", "ticket_number", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;   
   }
   if (!getIntValue(ExpertHandle, 1, property_id, _error))
   {
      PrintParamError("HistoryDealGetInteger", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;   
   }
   
   if (!sendLongResponse(ExpertHandle, HistoryDealGetInteger(ticket_number, (ENUM_DEAL_PROPERTY_INTEGER)property_id), _response_error))
   {
      PrintResponseError("HistoryDealGetInteger", _response_error);
   }
}

void Execute_HistoryDealGetString()
{
   ulong ticket_number;
   int property_id;
   
   if (!getULongValue(ExpertHandle, 0, ticket_number, _error))
   {
      PrintParamError("HistoryDealGetString", "ticket_number", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return; 
   }
   if (!getIntValue(ExpertHandle, 1, property_id, _error))
   {
      PrintParamError("HistoryDealGetString", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return; 
   }
   
   if (!sendStringResponse(ExpertHandle, HistoryDealGetString(ticket_number, (ENUM_DEAL_PROPERTY_STRING)property_id), _response_error))
   {
      PrintResponseError("HistoryDealGetString", _response_error);
   }
}

void Execute_AccountInfoDouble()
{
   int property_id;
   
   if (!getIntValue(ExpertHandle, 0, property_id, _error))
   {
      PrintParamError("AccountInfoDouble", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return; 
   }
   
   if (!sendDoubleResponse(ExpertHandle, AccountInfoDouble((ENUM_ACCOUNT_INFO_DOUBLE)property_id), _response_error))
   {
      PrintResponseError("AccountInfoDouble", _response_error);
   }
}

void Execute_AccountInfoInteger()
{
   int property_id;
   
   if (!getIntValue(ExpertHandle, 0, property_id, _error))
   {
      PrintParamError("AccountInfoInteger", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendLongResponse(ExpertHandle, AccountInfoInteger((ENUM_ACCOUNT_INFO_INTEGER)property_id), _response_error))
   {
      PrintResponseError("AccountInfoInteger", _response_error);
   }
}

void Execute_AccountInfoString()
{
   int property_id;
   
   if (!getIntValue(ExpertHandle, 0, property_id, _error))
   {
      PrintParamError("AccountInfoString", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendStringResponse(ExpertHandle, AccountInfoString((ENUM_ACCOUNT_INFO_STRING)property_id), _response_error))
   {
      PrintResponseError("AccountInfoString", _response_error);
   }
}

void Execute_SeriesInfoInteger()
{
   string symbol;
   int timeframe;
   int prop_id;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("SeriesInfoInteger", "property_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("SeriesInfoInteger", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 2, prop_id, _error))
   {
      PrintParamError("SeriesInfoInteger", "prop_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendLongResponse(ExpertHandle, SeriesInfoInteger(symbol, (ENUM_TIMEFRAMES)timeframe, (ENUM_SERIES_INFO_INTEGER)prop_id), _response_error))
   {
      PrintResponseError("SeriesInfoInteger", _response_error);
   }
}

void Execute_Bars()
{
   string symbol;
   int timeframe;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("Bars", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("Bars", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
         
   if (!sendIntResponse(ExpertHandle, Bars(symbol, (ENUM_TIMEFRAMES)timeframe), _response_error))
   {
      PrintResponseError("Bars", _response_error);
   }
}

void Execute_Bars2()
{
   string symbol;
   int timeframe;      
   int start_time;
   int stop_time;
   StringInit(symbol, 100, 0);  
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("Bars", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("Bars", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("Bars", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, stop_time, _error))
   {
      PrintParamError("Bars", "stop_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
         
   if (!sendIntResponse(ExpertHandle, Bars(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, (datetime)stop_time), _response_error))
   {
      PrintResponseError("Bars", _response_error);
   }
}

void Execute_BarsCalculated()
{
   int indicator_handle;
   
   if (!getIntValue(ExpertHandle, 0, indicator_handle, _error))
   {
      PrintParamError("BarsCalculated", "indicator_handle", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle, BarsCalculated(indicator_handle), _response_error))
   {
      PrintResponseError("BarsCalculated", _response_error);
   }
}

void Execute_CopyBuffer()
{
   int indicator_handle;
   int buffer_num;
   int start_pos;
   int count;
   double buffer[];

   if (!getIntValue(ExpertHandle, 0, indicator_handle, _error))
   {
      PrintParamError("CopyBuffer", "indicator_handle", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, buffer_num, _error))
   {
      PrintParamError("CopyBuffer", "buffer_num", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_pos, _error))
   {
      PrintParamError("CopyBuffer", "start_pos", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyBuffer", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
         
   int copied = CopyBuffer(indicator_handle, buffer_num, start_pos, count, buffer);
   
   if (!sendDoubleArrayResponse(ExpertHandle, buffer, copied, _response_error))
   {
      PrintResponseError("CopyBuffer", _response_error);
   }
}

void Execute_CopyBuffer1()
{
   int indicator_handle;
   int buffer_num;
   int start_time;
   int count;

   if (!getIntValue(ExpertHandle, 0, indicator_handle, _error))
   {
      PrintParamError("CopyBuffer", "indicator_handle", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 1, buffer_num, _error))
   {
      PrintParamError("CopyBuffer", "buffer_num", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyBuffer", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyBuffer", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   double buffer[];
   int copied = CopyBuffer(indicator_handle, buffer_num, (datetime)start_time, count, buffer);
   
   if (!sendDoubleArrayResponse(ExpertHandle, buffer, copied, _response_error))
   {
      PrintResponseError("CopyBuffer", _response_error);
   }
}

void Execute_CopyBuffer2()
{
   int indicator_handle;
   int buffer_num;
   int start_time;
   int stop_time;

   if (!getIntValue(ExpertHandle, 0, indicator_handle, _error))
   {
      PrintParamError("CopyBuffer", "indicator_handle", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, buffer_num, _error))
   {
      PrintParamError("CopyBuffer", "buffer_num", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyBuffer", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, stop_time, _error))
   {
      PrintParamError("CopyBuffer", "stop_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
         
   double buffer[];
   int copied = CopyBuffer(indicator_handle, buffer_num, (datetime)start_time, (datetime)stop_time, buffer);
   
   if (!sendDoubleArrayResponse(ExpertHandle, buffer, copied, _response_error))
   {
      PrintResponseError("CopyBuffer", _response_error);
   }
}

void Execute_CopyRates()
{
   string symbol;
   int timeframe;
   int start_pos;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyRates", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyRates", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 2, start_pos, _error))
   {
      PrintParamError("CopyRates", "start_pos", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyRates", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   MqlRates rates[];
   int copied = CopyRates(symbol, (ENUM_TIMEFRAMES)timeframe, start_pos, count, rates);       

   if (!sendMqlRatesArrayResponse(ExpertHandle, rates, copied, _response_error))
   {
      PrintResponseError("CopyRates", _response_error);
   }
}

void Execute_CopyRates1()
{
   string symbol;
   int timeframe;
   int start_time;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyRates", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;   
   }   
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyRates", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyRates", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyRates", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   MqlRates rates[];
   int copied = CopyRates(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, count, rates);       

   if (!sendMqlRatesArrayResponse(ExpertHandle, rates, copied, _response_error))
   {
      PrintResponseError("CopyRates", _response_error);
   }
}

void Execute_CopyRates2()
{
   string symbol;
   int timeframe;
   int start_time;
   int stop_time;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyRates", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyRates", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyRates", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 3, stop_time, _error))
   {
      PrintParamError("CopyRates", "stop_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   MqlRates rates[];
   int copied = CopyRates(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, (datetime)stop_time, rates);       

   if (!sendMqlRatesArrayResponse(ExpertHandle, rates, copied, _response_error))
   {
      PrintResponseError("CopyRates", _response_error);
   }
}

void Execute_CopyTime()
{
   string symbol;
   int timeframe;
   int start_pos;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyTime", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyTime", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 2, start_pos, _error))
   {
      PrintParamError("CopyTime", "start_pos", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyTime", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   datetime time_array[];
   int copied = CopyTime(symbol, (ENUM_TIMEFRAMES)timeframe, start_pos, count, time_array);       

   if (!sendLongArrayResponse(ExpertHandle, time_array, copied, _response_error))
   {
      PrintResponseError("CopyTime", _response_error);
   }
}

void Execute_CopyTime1()
{
   string symbol;
   int timeframe;
   int start_time;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyTime", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyTime", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyTime", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyTime", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   datetime time_array[];
   int copied = CopyTime(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, count, time_array);       

   if (!sendLongArrayResponse(ExpertHandle, time_array, copied, _response_error))
   {
      PrintResponseError("CopyTime", _response_error);
   }
}

void Execute_CopyTime2()
{
   string symbol;
   int timeframe;
   int start_time;
   int stop_time;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyTime", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyTime", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyTime", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 3, stop_time, _error))
   {
      PrintParamError("CopyTime", "stop_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   datetime time_array[];
   int copied = CopyTime(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, (datetime)stop_time, time_array);       

   if (!sendLongArrayResponse(ExpertHandle, time_array, copied, _response_error))
   {
      PrintResponseError("CopyTime", _response_error);
   }
}

void Execute_CopyOpen()
{
   string symbol;
   int timeframe;
   int start_pos;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyOpen", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyOpen", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_pos, _error))
   {
      PrintParamError("CopyOpen", "start_pos", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyOpen", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   double open_array[];
   int copied = CopyOpen(symbol, (ENUM_TIMEFRAMES)timeframe, start_pos, count, open_array);       

   if (!sendDoubleArrayResponse(ExpertHandle, open_array, copied, _response_error))
   {
      PrintResponseError("CopyOpen", _response_error);
   }
}

void Execute_CopyOpen1()
{
   string symbol;
   int timeframe;
   int start_time;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyOpen", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyOpen", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyOpen", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyOpen", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   double open_array[];
   int copied = CopyOpen(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, count, open_array);       

   if (!sendDoubleArrayResponse(ExpertHandle, open_array, copied, _response_error))
   {
      PrintResponseError("CopyOpen", _response_error);
   }
}

void Execute_CopyOpen2()
{
   string symbol;
   int timeframe;
   int start_time;
   int stop_time;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyOpen", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyOpen", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyOpen", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 3, stop_time, _error))
   {
      PrintParamError("CopyOpen", "stop_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   double open_array[];
   int copied = CopyOpen(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, (datetime)stop_time, open_array);       

   if (!sendDoubleArrayResponse(ExpertHandle, open_array, copied, _response_error))
   {
      PrintResponseError("CopyOpen", _response_error);
   }
}

void Execute_CopyHigh()
{
   string symbol;
   int timeframe;
   int start_pos;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyHigh", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyHigh", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_pos, _error))
   {
      PrintParamError("CopyHigh", "start_pos", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyHigh", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   double high_array[];
   int copied = CopyHigh(symbol, (ENUM_TIMEFRAMES)timeframe, start_pos, count, high_array);       

   if (!sendDoubleArrayResponse(ExpertHandle, high_array, copied, _response_error))
   {
      PrintResponseError("CopyHigh", _response_error);
   }
}

void Execute_CopyHigh1()
{
   string symbol;
   int timeframe;
   int start_time;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyHigh", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyHigh", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyHigh", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyHigh", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   double high_array[];
   int copied = CopyHigh(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, count, high_array);       

   if (!sendDoubleArrayResponse(ExpertHandle, high_array, copied, _response_error))
   {
      PrintResponseError("CopyHigh", _response_error);
   }
}

void Execute_CopyHigh2()
{
   string symbol;
   int timeframe;
   int start_time;
   int stop_time;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyHigh", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyHigh", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyHigh", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, stop_time, _error))
   {
      PrintParamError("CopyHigh", "stop_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   double high_array[];
   int copied = CopyHigh(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, (datetime)stop_time, high_array);       

   if (!sendDoubleArrayResponse(ExpertHandle, high_array, copied, _response_error))
   {
      PrintResponseError("CopyHigh", _response_error);
   }
}

void Execute_CopyLow()
{
   string symbol;
   int timeframe;
   int start_pos;
   int count;
   StringInit(symbol, 100, 0);
      
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyLow", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyLow", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 2, start_pos, _error))
   {
      PrintParamError("CopyLow", "start_pos", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyLow", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   double low_array[];   
   int copied = CopyLow(symbol, (ENUM_TIMEFRAMES)timeframe, start_pos, count, low_array);       

   if (!sendDoubleArrayResponse(ExpertHandle, low_array, copied, _response_error))
   {
      PrintResponseError("CopyLow", _response_error);
   }
}

void Execute_CopyLow1()
{
   string symbol;
   int timeframe;
   int start_time;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyLow", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyLow", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyLow", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyLow", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   double low_array[];
   int copied = CopyLow(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, count, low_array);       

   if (!sendDoubleArrayResponse(ExpertHandle, low_array, copied, _response_error))
   {
      PrintResponseError("CopyLow", _response_error);
   }
}

void Execute_CopyLow2()
{
   string symbol;
   int timeframe;
   int start_time;
   int stop_time;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyLow", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyLow", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyLow", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, stop_time, _error))
   {
      PrintParamError("CopyLow", "stop_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   double low_array[];
   int copied = CopyLow(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, (datetime)stop_time, low_array);       

   if (!sendDoubleArrayResponse(ExpertHandle, low_array, copied, _response_error))
   {
      PrintResponseError("CopyLow", _response_error);
   }
}

void Execute_CopyClose()
{
   string symbol;
   int timeframe;
   int start_pos;
   int count;
   StringInit(symbol, 100, 0);
      
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyClose", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyClose", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_pos, _error))
   {
      PrintParamError("CopyClose", "start_pos", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyClose", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   double close_array[];
   int copied = CopyClose(symbol, (ENUM_TIMEFRAMES)timeframe, start_pos, count, close_array);       

   if (!sendDoubleArrayResponse(ExpertHandle, close_array, copied, _response_error))
   {
      PrintResponseError("CopyClose", _response_error);
   }
}

void Execute_CopyClose1()
{
   string symbol;
   int timeframe;
   int start_time;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyClose", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyClose", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyClose", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyClose", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   double close_array[];
   int copied = CopyClose(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, count, close_array);       

   if (!sendDoubleArrayResponse(ExpertHandle, close_array, copied, _response_error))
   {
      PrintResponseError("CopyClose", _response_error);
   }
}

void Execute_CopyClose2()
{
   string symbol;
   int timeframe;
   int start_time;
   int stop_time;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyClose", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyClose", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyClose", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, stop_time, _error))
   {
      PrintParamError("CopyClose", "stop_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   double close_array[];
   int copied = CopyClose(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, (datetime)stop_time, close_array);       

   if (!sendDoubleArrayResponse(ExpertHandle, close_array, copied, _response_error))
   {
      PrintResponseError("CopyClose", _response_error);
   }
}

void Execute_CopyTickVolume()
{
   string symbol;
   int timeframe;
   int start_pos;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyTickVolume", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyTickVolume", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_pos, _error))
   {
      PrintParamError("CopyTickVolume", "start_pos", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyTickVolume", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   long volume_array[];
   int copied = CopyTickVolume(symbol, (ENUM_TIMEFRAMES)timeframe, start_pos, count, volume_array);       

   if (!sendLongArrayResponse(ExpertHandle, volume_array, copied, _response_error))
   {
      PrintResponseError("CopyTickVolume", _response_error);
   }
}

void Execute_CopyTickVolume1()
{
   string symbol;
   int timeframe;
   int start_time;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyTickVolume", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyTickVolume", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyTickVolume", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _response_error))
   {
      PrintParamError("CopyTickVolume", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   long volume_array[];
   int copied = CopyTickVolume(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, count, volume_array);       

   if (!sendLongArrayResponse(ExpertHandle, volume_array, copied, _response_error))
   {
      PrintResponseError("CopyTickVolume", _response_error);
   }
}

void Execute_CopyTickVolume2()
{
   string symbol;
   int timeframe;
   int start_time;
   int stop_time;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyTickVolume", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyTickVolume", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyTickVolume", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, stop_time, _error))
   {
      PrintParamError("CopyTickVolume", "stop_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   long volume_array[];
   int copied = CopyTickVolume(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, (datetime)stop_time, volume_array);       

   if (!sendLongArrayResponse(ExpertHandle, volume_array, copied, _response_error))
   {
      PrintResponseError("CopyTickVolume", _response_error);
   }
}

void Execute_CopyRealVolume()
{
   string symbol;
   int timeframe;
   int start_pos;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyRealVolume", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyRealVolume", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_pos, _error))
   {
      PrintParamError("CopyRealVolume", "start_pos", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyRealVolume", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   long volume_array[];
   int copied = CopyRealVolume(symbol, (ENUM_TIMEFRAMES)timeframe, start_pos, count, volume_array);       

   if (!sendLongArrayResponse(ExpertHandle, volume_array, copied, _response_error))
   {
      PrintResponseError("CopyRealVolume", _response_error);
   }
}

void Execute_CopyRealVolume1()
{
   string symbol;
   int timeframe;
   int start_time;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyRealVolume", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyRealVolume", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyRealVolume", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopyRealVolume", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   long volume_array[];
   int copied = CopyRealVolume(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, count, volume_array);       

   if (!sendLongArrayResponse(ExpertHandle, volume_array, copied, _response_error))
   {
      PrintResponseError("CopyRealVolume", _response_error);
   }
}

void Execute_CopyRealVolume2()
{
   string symbol;
   int timeframe;
   int start_time;
   int stop_time;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopyRealVolume", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopyRealVolume", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopyRealVolume", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, stop_time, _error))
   {
      PrintParamError("CopyRealVolume", "stop_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   long volume_array[];
   int copied = CopyRealVolume(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, (datetime)stop_time, volume_array);       

   if (!sendLongArrayResponse(ExpertHandle, volume_array, copied, _response_error))
   {
      PrintResponseError("CopyRealVolume", _response_error);
   }
}

void Execute_CopySpread()
{
   string symbol;
   int timeframe;
   int start_pos;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopySpread", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopySpread", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_pos, _error))
   {
      PrintParamError("CopySpread", "start_pos", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopySpread", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   int spread_array[];
   int copied = CopySpread(symbol, (ENUM_TIMEFRAMES)timeframe, start_pos, count, spread_array);       

   if (!sendIntArrayResponse(ExpertHandle, spread_array, copied, _response_error))
   {
      PrintResponseError("CopySpread", _response_error);
   }
}

void Execute_CopySpread1()
{
   string symbol;
   int timeframe;
   int start_time;
   int count;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopySpread", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopySpread", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopySpread", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, count, _error))
   {
      PrintParamError("CopySpread", "count", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   int spread_array[];
   int copied = CopySpread(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, count, spread_array);       

   if (!sendIntArrayResponse(ExpertHandle, spread_array, copied, _response_error))
   {
      PrintResponseError("CopySpread", _response_error);
   }
}

void Execute_CopySpread2()
{
   string symbol;
   int timeframe;
   int start_time;
   int stop_time;
   StringInit(symbol, 100, 0);   
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("CopySpread", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, timeframe, _error))
   {
      PrintParamError("CopySpread", "timeframe", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, start_time, _error))
   {
      PrintParamError("CopySpread", "start_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, stop_time, _error))
   {
      PrintParamError("CopySpread", "stop_time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   int spread_array[];
   int copied = CopySpread(symbol, (ENUM_TIMEFRAMES)timeframe, (datetime)start_time, (datetime)stop_time, spread_array);       

   if (!sendIntArrayResponse(ExpertHandle, spread_array, copied, _response_error))
   {
      PrintResponseError("CopySpread", _response_error);
   }
}

void Execute_SymbolsTotal()
{
   bool selected;
   
   if (!getBooleanValue(ExpertHandle, 0, selected, _error))
   {
      PrintParamError("SymbolsTotal", "selected", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle, SymbolsTotal(selected), _error))
   {
      PrintResponseError("SymbolsTotal", _response_error);
   }
}

void Execute_SymbolName()
{
   bool selected; 
   int pos;     
   
   if (!getIntValue(ExpertHandle, 0, pos, _error))
   {
      PrintParamError("SymbolName", "pos", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getBooleanValue(ExpertHandle, 1, selected, _error))
   {
      PrintParamError("SymbolName", "selected", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendStringResponse(ExpertHandle, SymbolName(pos, selected), _error))
   {
      PrintResponseError("SymbolName", _response_error);
   }
}

void Execute_SymbolSelect()
{
   string symbol;
   bool select;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("SymbolSelect", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getBooleanValue(ExpertHandle, 1, select, _error))
   {
      PrintParamError("SymbolSelect", "select", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendBooleanResponse(ExpertHandle, SymbolSelect(symbol, select), _response_error))
   {
      PrintResponseError("SymbolSelect", _response_error);
   }
}

void Execute_SymbolIsSynchronized()
{
   string symbol;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("SymbolIsSynchronized", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle, SymbolIsSynchronized(symbol), _response_error))
   {
      PrintResponseError("SymbolIsSynchronized", _response_error);
   }
}

void Execute_SymbolInfoDouble()
{
   string symbol;
   int prop_id;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("SymbolInfoDouble", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, prop_id, _error))
   {
      PrintParamError("SymbolInfoDouble", "prop_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendDoubleResponse(ExpertHandle, SymbolInfoDouble(symbol, (ENUM_SYMBOL_INFO_DOUBLE)prop_id), _response_error))
   {
      PrintResponseError("SymbolInfoDouble", _response_error);
   }
}

void Execute_SymbolInfoInteger()
{
   string symbol;
   int prop_id;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("SymbolInfoInteger", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 1, prop_id, _error))
   {
      PrintParamError("SymbolInfoInteger", "prop_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendLongResponse(ExpertHandle, SymbolInfoInteger(symbol, (ENUM_SYMBOL_INFO_INTEGER)prop_id), _response_error))
   {
      PrintResponseError("SymbolInfoInteger", _response_error);
   }
}

void Execute_SymbolInfoString()
{
   string symbol;
   int prop_id;
   StringInit(symbol, 100, 0);
  
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("SymbolInfoString", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 1, prop_id, _error))
   {
      PrintParamError("SymbolInfoString", "prop_id", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendStringResponse(ExpertHandle, SymbolInfoString(symbol, (ENUM_SYMBOL_INFO_STRING)prop_id), _response_error))
   {
      PrintResponseError("SymbolInfoString", _response_error);
   }
}

void Execute_SymbolInfoTick()
{
   string symbol;
   StringInit(symbol, 100, 0);
      
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("SymbolInfoTick", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   MqlTick tick={0}; 
   bool ok = SymbolInfoTick(symbol, tick);       
   
   if (ok)
   {
      if (!sendMqlTickResponse(ExpertHandle, tick, _response_error))
      {
         PrintResponseError("SymbolInfoTick", _response_error);
      }
   }
   else
   {
      if (!sendVoidResponse(ExpertHandle, _response_error))
      {
         PrintResponseError("SymbolInfoTick", _response_error);
      }
   }
}

void Execute_SymbolInfoSessionQuote()
{
   string symbol;
   int day_of_week;
   uint session_index;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("SymbolInfoSessionQuote", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, day_of_week, _error))
   {
      PrintParamError("SymbolInfoSessionQuote", "day_of_week", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getUIntValue(ExpertHandle, 2, session_index, _error))
   {
      PrintParamError("SymbolInfoSessionQuote", "session_index", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   datetime from;
   datetime to;   
   bool retVal = SymbolInfoSessionQuote(symbol, (ENUM_DAY_OF_WEEK)day_of_week, session_index, from, to);
   
   if (!sendStringResponse(ExpertHandle, ResultToString(retVal, from, to), _response_error))
   {
      PrintResponseError("SymbolInfoSessionQuote", _response_error);
   }
}

void Execute_SymbolInfoSessionTrade()
{
   string symbol;
   int day_of_week;
   uint session_index;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("SymbolInfoSessionTrade", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, day_of_week, _error))
   {
      PrintParamError("SymbolInfoSessionTrade", "day_of_week", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getUIntValue(ExpertHandle, 2, session_index, _error))
   {
      PrintParamError("SymbolInfoSessionTrade", "session_index", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   datetime from;
   datetime to;
   bool retVal = SymbolInfoSessionTrade(symbol, (ENUM_DAY_OF_WEEK)day_of_week, session_index, from, to);       

   if (!sendStringResponse(ExpertHandle, ResultToString(retVal, from, to), _response_error))
   {
      PrintResponseError("SymbolInfoSessionTrade", _response_error);
   }
}

void Execute_MarketBookAdd()
{
   string symbol;
   StringInit(symbol, 100, 0);
         
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("MarketBookAdd", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle,  MarketBookAdd(symbol), _response_error))
   {
      PrintResponseError("MarketBookAdd", _response_error);
   }
}

void Execute_MarketBookRelease()
{
   string symbol;
   StringInit(symbol, 100, 0);
         
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("MarketBookRelease", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendBooleanResponse(ExpertHandle, MarketBookRelease(symbol), _response_error))
   {
      PrintResponseError("MarketBookRelease", _response_error);
   }
}

void Execute_PositionOpen(bool isTradeResultRequired)
{
   string symbol;
   int order_type;
   double volume;
   double price;
   double sl;
   double tp;
   string comment;
   StringInit(symbol, 100, 0);
   StringInit(comment, 1000, 0);
         
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("PositionOpen", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, order_type, _error))
   {
      PrintParamError("PositionOpen", "order_type", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 2, volume, _error))
   {
      PrintParamError("PositionOpen", "volume", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 3, price, _error))
   {
      PrintParamError("PositionOpen", "price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 4, sl, _error))
   {
      PrintParamError("PositionOpen", "sl", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 5, tp, _error))
   {
      PrintParamError("PositionOpen", "tp", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 6, comment, _error))
   {
      PrintParamError("PositionOpen", "comment", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   
#ifdef __DEBUG_LOG__   
   PrintFormat("%s: symbol = %s, order_type = %d, volume = %f, price = %f, sl = %f, tp = %f, comment = %s", 
      __FUNCTION__, symbol, order_type, volume, price, sl, tp, comment);
#endif       
   
   CTrade trade;
   bool ok = trade.PositionOpen(symbol, (ENUM_ORDER_TYPE)order_type, volume, price, sl, tp, comment);
   if (isTradeResultRequired)
   {
      MqlTradeResult tradeResult={0};
      trade.Result(tradeResult);
      if (!sendStringResponse(ExpertHandle, ResultToString(ok, tradeResult), _response_error))
      {
         PrintResponseError("PositionOpen", _response_error);
      }
   }
   else
   {
      if (!sendBooleanResponse(ExpertHandle, ok, _response_error))
      {
         PrintResponseError("PositionOpen", _response_error);
      }
   }
   
   Print("command PositionOpen: result = ", ok);
}

void Execute_BacktestingReady()
{
   bool retVal = false;
   if (IsTesting())
   {
      Print("Remote client is ready for backteting");
      IsRemoteReadyForTesting = true;
      retVal = true;
   }
   
   if (!sendBooleanResponse(ExpertHandle, retVal, _response_error))
   {
      PrintResponseError("BacktestingReady", _response_error);
   }
}

void Execute_IsTesting()
{
   if (!sendBooleanResponse(ExpertHandle, IsTesting(), _response_error))
   {
      PrintResponseError("IsTesting", _response_error);
   }
}

void Execute_Print()
{
   string printMsg;
   StringInit(printMsg, 1000, 0);

   if (!getStringValue(ExpertHandle, 0, printMsg, _error))
   {
      PrintParamError("Print", "printMsg", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
      
   Print(printMsg);      
   if (!sendBooleanResponse(ExpertHandle, true, _response_error))
   {
      PrintResponseError("Print", _response_error);
   }
}

void Execute_PositionSelectByTicket()
{
   ulong ticket;
   if (!getULongValue(ExpertHandle, 0, ticket, _error))
   {
      PrintParamError("PositionSelectByTicket", "ticket", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle, PositionSelectByTicket(ticket), _response_error))
   {
      PrintResponseError("PositionSelectByTicket", _response_error);
   }
}

void Execute_ObjectCreate()
{
   long chartId;
   string name;
   int type;
   int nwin;
   int time;
   double price;
   StringInit(name, 200, 0);
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectCreate", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 1, name, _error))
   {
      PrintParamError("ObjectCreate", "name", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, type, _error))
   {
      PrintParamError("ObjectCreate", "type", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, nwin, _error))
   {
      PrintParamError("ObjectCreate", "nwin", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 4, time, _error))
   {
      PrintParamError("ObjectCreate", "time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 5, price, _error))
   {
      PrintParamError("ObjectCreate", "price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle, ObjectCreate(chartId, name, (ENUM_OBJECT)type, nwin, (datetime)time, price), _response_error))
   {
      PrintResponseError("ObjectCreate", _response_error);
   }
}

void Execute_ObjectName()
{
   long chartId;
   int pos;
   int subWindow;
   int type;
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectName", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, pos, _error))
   {
      PrintParamError("ObjectName", "pos", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, subWindow, _error))
   {
      PrintParamError("ObjectName", "subWindow", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, type, _error))
   {
      PrintParamError("ObjectName", "type", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendStringResponse(ExpertHandle, ObjectName(chartId, pos, subWindow, type), _error))
   {
      PrintResponseError("ObjectName", _response_error);
   }
}

void Execute_ObjectDelete()
{
   long chartId;
   string name;
   StringInit(name, 200, 0);
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectDelete", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 1, name, _error))
   {
      PrintParamError("ObjectDelete", "name", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle, ObjectDelete(chartId, name), _response_error))
   {
      PrintResponseError("ObjectDelete", _response_error);
   }   
}

void Execute_ObjectsDeleteAll()
{
   long chartId;
   int subWindow;
   int type;
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectsDeleteAll", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, subWindow, _error))
   {
      PrintParamError("ObjectsDeleteAll", "subWindow", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, type, _error))
   {
      PrintParamError("ObjectsDeleteAll", "type", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendIntResponse(ExpertHandle, ObjectsDeleteAll(chartId, subWindow, type), _error))
   {
      PrintResponseError("ObjectsDeleteAll", _response_error);
   }
}

void Execute_ObjectFind()
{
   long chartId;
   string name;
   StringInit(name, 200, 0);
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectFind", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 1, name, _error))
   {
      PrintParamError("ObjectFind", "name", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendIntResponse(ExpertHandle, ObjectFind(chartId, name), _error))
   {
      PrintResponseError("ObjectFind", _response_error);
   }
}

void Execute_ObjectGetTimeByValue()
{
   long chartId;
   string name;
   double value;
   int lineId;
   StringInit(name, 200, 0);
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectGetTimeByValue", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 1, name, _error))
   {
      PrintParamError("ObjectGetTimeByValue", "name", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 2, value, _error))
   {
      PrintParamError("ObjectGetTimeByValue", "value", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, lineId, _error))
   {
      PrintParamError("ObjectGetTimeByValue", "lineId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle, (int)ObjectGetTimeByValue(chartId, name, value, lineId), _error))
   {
      PrintResponseError("ObjectGetTimeByValue", _response_error);
   }
}

void Execute_ObjectGetValueByTime()
{
   long chartId;
   string name;
   int time;
   int lineId;
   StringInit(name, 200, 0);
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectGetValueByTime", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 1, name, _error))
   {
      PrintParamError("ObjectGetValueByTime", "name", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, time, _error))
   {
      PrintParamError("ObjectGetValueByTime", "time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, lineId, _error))
   {
      PrintParamError("ObjectGetValueByTime", "lineId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendDoubleResponse(ExpertHandle, ObjectGetValueByTime(chartId, name, (datetime)time, lineId), _error))
   {
      PrintResponseError("ObjectGetValueByTime", _response_error);
   }
}

void Execute_ObjectMove()
{
   long chartId;
   string name;
   int pointIndex;
   int time;
   double price;
   StringInit(name, 200, 0);
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectMove", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 1, name, _error))
   {
      PrintParamError("ObjectMove", "name", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, pointIndex, _error))
   {
      PrintParamError("ObjectMove", "pointIndex", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, time, _error))
   {
      PrintParamError("ObjectMove", "time", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 4, price, _error))
   {
      PrintParamError("ObjectMove", "price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle, ObjectMove(chartId, name, pointIndex, (datetime)time, price), _error))
   {
      PrintResponseError("ObjectMove", _response_error);
   }
}

void Execute_ObjectsTotal()
{
   long chartId;
   int subWindow;
   int type;
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectsTotal", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, subWindow, _error))
   {
      PrintParamError("ObjectsTotal", "subWindow", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, type, _error))
   {
      PrintParamError("ObjectsTotal", "type", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendIntResponse(ExpertHandle, ObjectsTotal(chartId, subWindow, type), _error))
   {
      PrintResponseError("ObjectsTotal", _response_error);
   }
}

void Execute_ObjectGetDouble()
{
   long chartId;
   string name;
   int propId;
   StringInit(name, 200, 0);
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectGetDouble", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 1, name, _error))
   {
      PrintParamError("ObjectGetDouble", "name", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, propId, _error))
   {
      PrintParamError("ObjectGetDouble", "propId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendDoubleResponse(ExpertHandle, ObjectGetDouble(chartId, name, (ENUM_OBJECT_PROPERTY_DOUBLE)propId), _error))
   {
      PrintResponseError("ObjectGetDouble", _response_error);
   }
}

void Execute_ObjectGetInteger()
{
   long chartId;
   string name;
   int propId;
   StringInit(name, 200, 0);
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectGetInteger", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 1, name, _error))
   {
      PrintParamError("ObjectGetInteger", "name", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, propId, _error))
   {
      PrintParamError("ObjectGetInteger", "propId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendLongResponse(ExpertHandle, ObjectGetInteger(chartId, name, (ENUM_OBJECT_PROPERTY_INTEGER)propId), _error))
   {
      PrintResponseError("ObjectGetInteger", _response_error);
   }
}

void Execute_ObjectGetString()
{
   long chartId;
   string name;
   int propId;
   StringInit(name, 200, 0);
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectGetString", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 1, name, _error))
   {
      PrintParamError("ObjectGetString", "name", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, propId, _error))
   {
      PrintParamError("ObjectGetString", "propId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendStringResponse(ExpertHandle, ObjectGetString(chartId, name, (ENUM_OBJECT_PROPERTY_STRING)propId), _error))
   {
      PrintResponseError("ObjectGetString", _response_error);
   }
}

void Execute_ObjectSetDouble()
{
   long chartId;
   string name;
   int propId;
   double propValue;
   StringInit(name, 200, 0);
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectSetDouble", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 1, name, _error))
   {
      PrintParamError("ObjectSetDouble", "name", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, propId, _error))
   {
      PrintParamError("ObjectSetDouble", "propId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 3, propValue, _error))
   {
      PrintParamError("ObjectSetDouble", "propValue", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle, ObjectSetDouble(chartId, name, (ENUM_OBJECT_PROPERTY_DOUBLE)propId, propValue), _error))
   {
      PrintResponseError("ObjectSetDouble", _response_error);
   }
}

void Execute_ObjectSetInteger()
{
   long chartId;
   string name;
   int propId;
   long propValue;
   StringInit(name, 200, 0);
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectSetInteger", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 1, name, _error))
   {
      PrintParamError("ObjectSetInteger", "name", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, propId, _error))
   {
      PrintParamError("ObjectSetInteger", "propId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getLongValue(ExpertHandle, 3, propValue, _error))
   {
      PrintParamError("ObjectSetInteger", "propValue", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle, ObjectSetInteger(chartId, name, (ENUM_OBJECT_PROPERTY_INTEGER)propId, propValue), _error))
   {
      PrintResponseError("ObjectSetInteger", _response_error);
   }
}

void Execute_ObjectSetString()
{
   long chartId;
   string name;
   int propId;
   string propValue;
   StringInit(name, 200, 0);
   StringInit(propValue, 1000, 0);
   
   if (!getLongValue(ExpertHandle, 0, chartId, _error))
   {
      PrintParamError("ObjectSetString", "chartId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 1, name, _error))
   {
      PrintParamError("ObjectSetString", "name", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, propId, _error))
   {
      PrintParamError("ObjectSetString", "propId", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getStringValue(ExpertHandle, 3, propValue, _error))
   {
      PrintParamError("ObjectSetString", "propValue", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendBooleanResponse(ExpertHandle, ObjectSetString(chartId, name, (ENUM_OBJECT_PROPERTY_STRING)propId, propValue), _error))
   {
      PrintResponseError("ObjectSetString", _response_error);
   }
}

void Execute_iAC()
{
   string symbol;
   int period;   
   StringInit(symbol, 200, 0);
 
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iAC", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iAC", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle, iAC(symbol, (ENUM_TIMEFRAMES)period), _error))
   {
      PrintResponseError("iAC", _response_error);
   }
}

void Execute_iAD()
{
   string symbol;
   int period;
   int applied_volume;
   StringInit(symbol, 200, 0);
 
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iAD", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iAD", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, applied_volume, _error))
   {
      PrintParamError("iAD", "applied_volume", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendIntResponse(ExpertHandle, 
      iAD(symbol, (ENUM_TIMEFRAMES)period, (ENUM_APPLIED_VOLUME)applied_volume), 
         _error))
   {
      PrintResponseError("iAD", _response_error);
   }
}

void Execute_iADX()
{
   string symbol;
   int period;
   int adx_period;
   StringInit(symbol, 200, 0);
 
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iADX", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iADX", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, adx_period, _error))
   {
      PrintParamError("iADX", "adx_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendIntResponse(ExpertHandle, 
      iADX(symbol, (ENUM_TIMEFRAMES)period, adx_period), 
      _error))
   {
      PrintResponseError("iADX", _response_error);
   }
}

void Execute_iADXWilder()
{
   string symbol;
   int period;
   int adx_period;
   StringInit(symbol, 200, 0);
 
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iADXWilder", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iADXWilder", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, adx_period, _error))
   {
      PrintParamError("iADXWilder", "adx_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendIntResponse(ExpertHandle, 
      iADXWilder(symbol, (ENUM_TIMEFRAMES)period, adx_period), 
         _error))
   {
      PrintResponseError("iADXWilder", _response_error);
   }
}

void Execute_iAlligator()
{
   string symbol;
   int period;
   int jaw_period;
   int jaw_shift;
   int teeth_period;
   int teeth_shift;
   int lips_period;
   int lips_shift;
   int ma_method;
   int applied_price;
   StringInit(symbol, 200, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iAlligator", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iAlligator", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, jaw_period, _error))
   {
      PrintParamError("iAlligator", "jaw_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, jaw_shift, _error))
   {
      PrintParamError("iAlligator", "jaw_shift", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 4, teeth_period, _error))
   {
      PrintParamError("iAlligator", "teeth_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 5, teeth_shift, _error))
   {
      PrintParamError("iAlligator", "teeth_shift", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 6, lips_period, _error))
   {
      PrintParamError("iAlligator", "lips_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 7, lips_shift, _error))
   {
      PrintParamError("iAlligator", "lips_shift", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 8, ma_method, _error))
   {
      PrintParamError("iAlligator", "ma_method", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 9, applied_price, _error))
   {
      PrintParamError("iAlligator", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendIntResponse(ExpertHandle, 
      iAlligator(symbol, (ENUM_TIMEFRAMES)period, jaw_period, jaw_shift, teeth_period, teeth_shift, 
         lips_period, lips_shift, (ENUM_MA_METHOD)ma_method, (ENUM_APPLIED_PRICE)applied_price), 
      _error))
   {
      PrintResponseError("iAlligator", _response_error);
   }
}

void Execute_iAMA()
{
   string symbol;
   int period;
   int ama_period;
   int fast_ma_period;
   int slow_ma_period;
   int ama_shift;
   int applied_price;
   StringInit(symbol, 200, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iAMA", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iAMA", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ama_period, _error))
   {
      PrintParamError("iAMA", "ama_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, fast_ma_period, _error))
   {
      PrintParamError("iAMA", "fast_ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 4, slow_ma_period, _error))
   {
      PrintParamError("iAMA", "slow_ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 5, ama_shift, _error))
   {
      PrintParamError("iAMA", "ama_shift", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 6, applied_price, _error))
   {
      PrintParamError("iAMA", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendIntResponse(ExpertHandle, 
      iAMA(symbol, (ENUM_TIMEFRAMES)period, ama_period, fast_ma_period, slow_ma_period, ama_shift, (ENUM_APPLIED_PRICE)applied_price), 
      _error))
   {
      PrintResponseError("iAMA", _response_error);
   }
}

void Execute_iAO()
{
   string symbol;
   int period;
   StringInit(symbol, 200, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iAO", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iAO", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle, 
      iAO(symbol, (ENUM_TIMEFRAMES)period), 
         _error))
   {
      PrintResponseError("iAO", _response_error);
   }
}

void Execute_iATR()
{
   string symbol;
   int period;
   int ma_period;
   StringInit(symbol, 200, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iATR", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iATR", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iATR", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }

   if (!sendIntResponse(ExpertHandle, 
      iATR(symbol, (ENUM_TIMEFRAMES)period, ma_period), 
         _error))
   {
      PrintResponseError("iATR", _response_error);
   }
}

void Execute_iBearsPower()
{
   string symbol;
   int period;
   int ma_period;
   StringInit(symbol, 200, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iBearsPower", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iBearsPower", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iBearsPower", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }

   if (!sendIntResponse(ExpertHandle, 
      iBearsPower(symbol, (ENUM_TIMEFRAMES)period, ma_period), 
         _error))
   {
      PrintResponseError("iBearsPower", _response_error);
   }
}

void Execute_iBands()
{
   string symbol;
   int period;
   int bands_period;
   int bands_shift;
   double deviation;
   int applied_price;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iBands", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iBands", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, bands_period, _error))
   {
      PrintParamError("iBands", "bands_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   if (!getIntValue(ExpertHandle, 3, bands_shift, _error))
   {
      PrintParamError("iBands", "bands_shift", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   if (!getDoubleValue(ExpertHandle, 4, deviation, _error))
   {
      PrintParamError("iBands", "deviation", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   if (!getIntValue(ExpertHandle, 5, applied_price, _error))
   {
      PrintParamError("iBands", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   
   if (!sendIntResponse(ExpertHandle, 
      iBands(symbol, (ENUM_TIMEFRAMES)period, bands_period, bands_shift, deviation, (ENUM_APPLIED_PRICE)applied_price), 
         _error))
   {
      PrintResponseError("iBands", _response_error);
   }   
}

void Execute_iBullsPower()
{
   string symbol;
   int period;
   int ma_period;
   StringInit(symbol, 200, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iBullsPower", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iBullsPower", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iBullsPower", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }

   if (!sendIntResponse(ExpertHandle, 
      iBearsPower(symbol, (ENUM_TIMEFRAMES)period, ma_period), 
         _error))
   {
      PrintResponseError("iBullsPower", _response_error);
   }
}

void Execute_iCCI()
{
   string symbol;
   int period;
   int ma_period;
   int applied_price;
   StringInit(symbol, 200, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iCCI", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iCCI", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iCCI", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   if (!getIntValue(ExpertHandle, 3, applied_price, _error))
   {
      PrintParamError("iCCI", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }

   if (!sendIntResponse(ExpertHandle, 
      iCCI(symbol, (ENUM_TIMEFRAMES)period, ma_period, (ENUM_APPLIED_PRICE) applied_price), 
         _error))
   {
      PrintResponseError("iCCI", _response_error);
   }
}

void Execute_iChaikin()
{
   string symbol;
   int period;
   int fast_ma_period;
   int slow_ma_period;
   int ma_period;
   int applied_volume;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iChaikin", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iChaikin", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, fast_ma_period, _error))
   {
      PrintParamError("iChaikin", "fast_ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, slow_ma_period, _error))
   {
      PrintParamError("iChaikin", "slow_ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 4, ma_period, _error))
   {
      PrintParamError("iChaikin", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   if (!getIntValue(ExpertHandle, 5, applied_volume, _error))
   {
      PrintParamError("iChaikin", "applied_volume", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   
   if (!sendIntResponse(ExpertHandle, 
      iChaikin(symbol, (ENUM_TIMEFRAMES)period, fast_ma_period, slow_ma_period, (ENUM_MA_METHOD)ma_period, (ENUM_APPLIED_VOLUME) applied_volume), 
         _error))
   {
      PrintResponseError("iChaikin", _response_error);
   }   
}

void Execute_iDEMA()
{
   string symbol;
   int period;
   int ma_period;
   int ma_shift;
   int applied_price;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iDEMA", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iDEMA", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iDEMA", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   if (!getIntValue(ExpertHandle, 3, ma_shift, _error))
   {
      PrintParamError("iDEMA", "ma_shift", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   if (!getIntValue(ExpertHandle, 4, applied_price, _error))
   {
      PrintParamError("iDEMA", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }

   if (!sendIntResponse(ExpertHandle, 
      iDEMA(symbol, (ENUM_TIMEFRAMES)period, ma_period, ma_shift, (ENUM_APPLIED_PRICE) applied_price), 
         _error))
   {
      PrintResponseError("iDEMA", _response_error);
   }
}

void Execute_iDeMarker()
{
   string symbol;
   int period;
   int ma_period;
   StringInit(symbol, 200, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iDeMarker", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iDeMarker", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iDeMarker", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   
   if (!sendIntResponse(ExpertHandle, 
      iDeMarker(symbol, (ENUM_TIMEFRAMES)period, ma_period), 
         _error))
   {
      PrintResponseError("iDeMarker", _response_error);
   }
}

void Execute_iEnvelopes()
{
   string symbol;
   int period;
   int ma_period;
   int ma_shift;
   int ma_method;
   int applied_price;
   double deviation;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iEnvelopes", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iEnvelopes", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iEnvelopes", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   if (!getIntValue(ExpertHandle, 3, ma_shift, _error))
   {
      PrintParamError("iEnvelopes", "ma_shift", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   if (!getIntValue(ExpertHandle, 4, ma_method, _error))
   {
      PrintParamError("iEnvelopes", "ma_method", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   if (!getIntValue(ExpertHandle, 5, applied_price, _error))
   {
      PrintParamError("iEnvelopes", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   if (!getDoubleValue(ExpertHandle, 6, deviation, _error))
   {
      PrintParamError("iEnvelopes", "deviation", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }

   if (!sendIntResponse(ExpertHandle, 
      iEnvelopes(symbol, (ENUM_TIMEFRAMES)period, ma_period, ma_shift, (ENUM_MA_METHOD)ma_method, (ENUM_APPLIED_PRICE)applied_price, deviation), 
         _error))
   {
      PrintResponseError("iEnvelopes", _response_error);
   }
}

void Execute_iForce()
{
   string symbol;
   int period;
   int ma_period;
   int ma_method;
   int applied_volume;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iForce", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iForce", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iForce", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   if (!getIntValue(ExpertHandle, 3, ma_method, _error))
   {
      PrintParamError("iForce", "ma_method", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }
   if (!getIntValue(ExpertHandle, 4, applied_volume, _error))
   {
      PrintParamError("iForce", "applied_volume", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
   }

   if (!sendIntResponse(ExpertHandle, 
      iForce(symbol, (ENUM_TIMEFRAMES)period, ma_period, (ENUM_MA_METHOD)ma_method, (ENUM_APPLIED_VOLUME)applied_volume), 
         _error))
   {
      PrintResponseError("iForce", _response_error);
   }
}

void Execute_iFractals()
{
   string symbol;
   int period;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iFractals", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iFractals", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iFractals(symbol, (ENUM_TIMEFRAMES)period), 
         _error))
   {
      PrintResponseError("iFractals", _response_error);
   }
}

void Execute_iFrAMA()
{
   string symbol;
   int period;
   int ma_period;
   int ma_shift;
   int applied_price;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iFrAMA", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iFrAMA", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iFrAMA", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, ma_shift, _error))
   {
      PrintParamError("iFrAMA", "ma_shift", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 4, applied_price, _error))
   {
      PrintParamError("iFrAMA", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iFrAMA(symbol, (ENUM_TIMEFRAMES)period, ma_period, ma_shift, (ENUM_APPLIED_PRICE)applied_price), 
         _error))
   {
      PrintResponseError("iFrAMA", _response_error);
   }
}
   
void Execute_iGator()
{
   string symbol;
   int period;
   int jaw_period;
   int jaw_shift;
   int teeth_period;
   int teeth_shift;
   int lips_period;
   int lips_shift;
   int ma_method;
   int applied_price;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iGator", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iGator", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, jaw_period, _error))
   {
      PrintParamError("iGator", "jaw_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, jaw_shift, _error))
   {
      PrintParamError("iGator", "jaw_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 4, teeth_period, _error))
   {
      PrintParamError("iGator", "teeth_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 5, teeth_shift, _error))
   {
      PrintParamError("iGator", "teeth_shift", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 6, lips_period, _error))
   {
      PrintParamError("iGator", "lips_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 7, lips_shift, _error))
   {
      PrintParamError("iGator", "lips_shift", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 8, ma_method, _error))
   {
      PrintParamError("iGator", "ma_method", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 9, applied_price, _error))
   {
      PrintParamError("iGator", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iGator(symbol, (ENUM_TIMEFRAMES)period, jaw_period, jaw_shift, teeth_period, teeth_shift, lips_period, lips_shift, (ENUM_MA_METHOD)ma_method, (ENUM_APPLIED_PRICE)applied_price), 
         _error))
   {
      PrintResponseError("iGator", _response_error);
   }
}

void Execute_iIchimoku()
{
   string symbol;
   int period;
   int tenkan_sen;
   int kijun_sen;
   int senkou_span_b;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iIchimoku", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iIchimoku", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, tenkan_sen, _error))
   {
      PrintParamError("iIchimoku", "tenkan_sen", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, kijun_sen, _error))
   {
      PrintParamError("iIchimoku", "kijun_sen", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 4, senkou_span_b, _error))
   {
      PrintParamError("iIchimoku", "senkou_span_b", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iIchimoku(symbol, (ENUM_TIMEFRAMES)period, tenkan_sen, kijun_sen, senkou_span_b), 
         _error))
   {
      PrintResponseError("iIchimoku", _response_error);
   }
}

void Execute_iBWMFI()
{
   string symbol;
   int period;
   int applied_volume;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iBWMFI", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iBWMFI", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, applied_volume, _error))
   {
      PrintParamError("iBWMFI", "applied_volume", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iBWMFI(symbol, (ENUM_TIMEFRAMES)period, (ENUM_APPLIED_VOLUME)applied_volume), 
         _error))
   {
      PrintResponseError("iBWMFI", _response_error);
   }
}

void Execute_iMomentum()
{
   string symbol;
   int period;
   int mom_period;
   int applied_price;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iMomentum", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iMomentum", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, mom_period, _error))
   {
      PrintParamError("iMomentum", "mom_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, applied_price, _error))
   {
      PrintParamError("iMomentum", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iMomentum(symbol, (ENUM_TIMEFRAMES)period, mom_period, (ENUM_APPLIED_PRICE)applied_price), 
         _error))
   {
      PrintResponseError("iMomentum", _response_error);
   }
}

void Execute_iMFI()
{
   string symbol;
   int period;
   int ma_period;
   int applied_volume;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iMFI", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iMFI", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iMFI", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, applied_volume, _error))
   {
      PrintParamError("iMFI", "applied_volume", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iMFI(symbol, (ENUM_TIMEFRAMES)period, ma_period, (ENUM_APPLIED_VOLUME)applied_volume), 
         _error))
   {
      PrintResponseError("iMFI", _response_error);
   }
}

void Execute_iMA()
{
   string symbol;
   int period;
   int ma_period;
   int ma_shift;
   int ma_method;
   int applied_price;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iMA", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iMA", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iMA", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 3, ma_shift, _error))
   {
      PrintParamError("iMA", "ma_shift", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 4, ma_method, _error))
   {
      PrintParamError("iMA", "ma_method", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 5, applied_price, _error))
   {
      PrintParamError("iMA", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iMA(symbol, (ENUM_TIMEFRAMES)period, ma_period, ma_shift, (ENUM_MA_METHOD)ma_method, (ENUM_APPLIED_PRICE)applied_price), 
         _error))
   {
      PrintResponseError("iMA", _response_error);
   }
}

void Execute_iOsMA()
{
   string symbol;
   int period;
   int fast_ema_period;
   int slow_ema_period;
   int signal_period;
   int applied_price;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iOsMA", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iOsMA", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, fast_ema_period, _error))
   {
      PrintParamError("iOsMA", "fast_ema_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, slow_ema_period, _error))
   {
      PrintParamError("iOsMA", "slow_ema_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 4, signal_period, _error))
   {
      PrintParamError("iOsMA", "signal_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 5, applied_price, _error))
   {
      PrintParamError("iOsMA", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iOsMA(symbol, (ENUM_TIMEFRAMES)period, fast_ema_period, slow_ema_period, signal_period, (ENUM_APPLIED_PRICE)applied_price), 
         _error))
   {
      PrintResponseError("iOsMA", _response_error);
   }
}

void Execute_iMACD()
{
   string symbol;
   int period;
   int fast_ema_period;
   int slow_ema_period;
   int signal_period;
   int applied_price;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iMACD", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iMACD", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, fast_ema_period, _error))
   {
      PrintParamError("iMACD", "fast_ema_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, slow_ema_period, _error))
   {
      PrintParamError("iMACD", "slow_ema_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 4, signal_period, _error))
   {
      PrintParamError("iMACD", "signal_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 5, applied_price, _error))
   {
      PrintParamError("iMACD", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iMACD(symbol, (ENUM_TIMEFRAMES)period, fast_ema_period, slow_ema_period, signal_period, (ENUM_APPLIED_PRICE)applied_price), 
         _error))
   {
      PrintResponseError("iMACD", _response_error);
   }
}

void Execute_iOBV()
{
   string symbol;
   int period;
   int applied_volume;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iOBV", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iOBV", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, applied_volume, _error))
   {
      PrintParamError("iOBV", "applied_volume", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendIntResponse(ExpertHandle,
      iOBV(symbol, (ENUM_TIMEFRAMES)period, (ENUM_APPLIED_VOLUME)applied_volume), 
         _error))
   {
      PrintResponseError("iOBV", _response_error);
   }
}

void Execute_iSAR()
{
   string symbol;
   int period;
   double step;
   double maximum;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iSAR", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iSAR", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 2, step, _error))
   {
      PrintParamError("iSAR", "step", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getDoubleValue(ExpertHandle, 3, maximum, _error))
   {
      PrintParamError("iSAR", "maximum", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iSAR(symbol, (ENUM_TIMEFRAMES)period, step, maximum), 
         _error))
   {
      PrintResponseError("iSAR", _response_error);
   }
}

void Execute_iRSI()
{
   string symbol;
   int period;
   int ma_period;
   int applied_price;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iRSI", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iRSI", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iRSI", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, applied_price, _error))
   {
      PrintParamError("iRSI", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendIntResponse(ExpertHandle,
      iRSI(symbol, (ENUM_TIMEFRAMES)period, ma_period, (ENUM_APPLIED_PRICE)applied_price), 
         _error))
   {
      PrintResponseError("iRSI", _response_error);
   }   
}

void Execute_iRVI()
{
   string symbol;
   int period;
   int ma_period;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iRVI", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iRVI", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iRVI", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendIntResponse(ExpertHandle,
      iRVI(symbol, (ENUM_TIMEFRAMES)period, ma_period), 
         _error))
   {
      PrintResponseError("iRVI", _response_error);
   } 
}

void Execute_iStdDev()
{
   string symbol;
   int period;
   int ma_period;
   int ma_shift;
   int ma_method;
   int applied_price;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iStdDev", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iStdDev", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iStdDev", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, ma_shift, _error))
   {
      PrintParamError("iStdDev", "ma_shift", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 4, ma_method, _error))
   {
      PrintParamError("iStdDev", "ma_method", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 5, applied_price, _error))
   {
      PrintParamError("iStdDev", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iStdDev(symbol, (ENUM_TIMEFRAMES)period, ma_period, ma_shift, (ENUM_MA_METHOD)ma_method, (ENUM_APPLIED_PRICE)applied_price), 
         _error))
   {
      PrintResponseError("iStdDev", _response_error);
   }
}

void Execute_iStochastic()
{
   string symbol;
   int period;
   int Kperiod;
   int Dperiod;
   int slowing;
   int ma_method;
   int price_field;
   StringInit(symbol, 200, 0);

   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iStochastic", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iStochastic", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, Kperiod, _error))
   {
      PrintParamError("iStochastic", "Kperiod", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, Dperiod, _error))
   {
      PrintParamError("iStochastic", "Dperiod", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 4, slowing, _error))
   {
      PrintParamError("iStochastic", "slowing", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 5, ma_method, _error))
   {
      PrintParamError("iStochastic", "ma_method", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 6, price_field, _error))
   {
      PrintParamError("iStochastic", "price_field", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iStochastic(symbol, (ENUM_TIMEFRAMES)period, Kperiod, Dperiod, slowing, (ENUM_MA_METHOD)slowing, (ENUM_STO_PRICE)price_field), 
         _error))
   {
      PrintResponseError("iStochastic", _response_error);
   }
}

void Execute_iTEMA()
{
   string symbol;
   int period;
   int ma_period;
   int ma_shift;
   int applied_price;
   StringInit(symbol, 200, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iTEMA", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iTEMA", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iTEMA", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, ma_shift, _error))
   {
      PrintParamError("iTEMA", "ma_shift", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 4, applied_price, _error))
   {
      PrintParamError("iTEMA", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iTEMA(symbol, (ENUM_TIMEFRAMES)period, ma_period, ma_shift, (ENUM_APPLIED_PRICE)applied_price), 
         _error))
   {
      PrintResponseError("iTEMA", _response_error);
   }
}

void Execute_iTriX()
{
   string symbol;
   int period;
   int ma_period;
   int applied_price;
   StringInit(symbol, 200, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iTriX", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iTriX", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, ma_period, _error))
   {
      PrintParamError("iTriX", "ma_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 3, applied_price, _error))
   {
      PrintParamError("iTriX", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iTriX(symbol, (ENUM_TIMEFRAMES)period, ma_period, (ENUM_APPLIED_PRICE)applied_price), 
         _error))
   {
      PrintResponseError("iTriX", _response_error);
   }
}

void Execute_iWPR()
{
   string symbol;
   int period;
   int calc_period;
   StringInit(symbol, 200, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iWPR", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iWPR", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, calc_period, _error))
   {
      PrintParamError("iWPR", "calc_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   if (!sendIntResponse(ExpertHandle,
      iWPR(symbol, (ENUM_TIMEFRAMES)period, calc_period), 
         _error))
   {
      PrintResponseError("iWPR", _response_error);
   }
}

void Execute_iVIDyA()
{
   string symbol;
   int period;
   int cmo_period;
   int ema_period;
   int ma_shift;
   int applied_price;
   StringInit(symbol, 200, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iVIDyA", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iVIDyA", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, cmo_period, _error))
   {
      PrintParamError("iVIDyA", "cmo_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }   
   if (!getIntValue(ExpertHandle, 3, ema_period, _error))
   {
      PrintParamError("iVIDyA", "ema_period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }  
   if (!getIntValue(ExpertHandle, 4, ma_shift, _error))
   {
      PrintParamError("iVIDyA", "ma_shift", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 5, applied_price, _error))
   {
      PrintParamError("iVIDyA", "applied_price", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iVIDyA(symbol, (ENUM_TIMEFRAMES)period, cmo_period, ema_period, ma_shift, (ENUM_APPLIED_PRICE)applied_price), 
         _error))
   {
      PrintResponseError("iVIDyA", _response_error);
   }
}

void Execute_iVolumes()
{
   string symbol;
   int period;
   int applied_volume;
   StringInit(symbol, 200, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("iVolumes", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 1, period, _error))
   {
      PrintParamError("iVolumes", "period", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   if (!getIntValue(ExpertHandle, 2, applied_volume, _error))
   {
      PrintParamError("iVolumes", "applied_volume", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }

   if (!sendIntResponse(ExpertHandle,
      iVolumes(symbol, (ENUM_TIMEFRAMES)period, (ENUM_APPLIED_VOLUME)applied_volume), 
         _error))
   {
      PrintResponseError("iVolumes", _response_error);
   }
}

void Execute_TimeCurrent()
{
   if (!sendLongResponse(ExpertHandle, TimeCurrent(), _response_error))
   {
      PrintResponseError("TimeCurrent", _response_error);
   }
}

void Execute_TimeTradeServer()
{
   if (!sendLongResponse(ExpertHandle, TimeTradeServer(), _response_error))
   {
      PrintResponseError("TimeTradeServer", _response_error);
   }
}

void Execute_TimeLocal()
{
   if (!sendLongResponse(ExpertHandle, TimeLocal(), _response_error))
   {
      PrintResponseError("TimeLocal", _response_error);
   }
}

void Execute_TimeGMT()
{
   if (!sendLongResponse(ExpertHandle, TimeGMT(), _response_error))
   {
      PrintResponseError("TimeGMT", _response_error);
   }
}

#define GET_VALUE_OR_RETURN_WITH_SENDING_ERROR(get_func, argument_id, argument, cmd_name, param_name) if (!get_func(ExpertHandle, argument_id, argument, _error)) \
   {                                                                                                                                                              \
      PrintParamError(cmd_name, param_name, _error);                                                                                                              \
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);                                                                                               \
      return;                                                                                                                                                     \
   }                                                                                                                                                              \

#define GET_INTEGER_VALUE(argument_id, argument, cmd_name, param_name) GET_VALUE_OR_RETURN_WITH_SENDING_ERROR(getIntValue, argument_id, argument, cmd_name, param_name)

void Execute_IndicatorRelease()
{
   int indicator_handle;
   GET_INTEGER_VALUE(0, indicator_handle, "IndicatorRelease", "indicator_handle");
   
#ifdef __DEBUG_LOG__
   PrintFormat("%s: indicator_handle = %d", __FUNCTION__, indicator_handle);
#endif 
   
   if (!sendBooleanResponse(ExpertHandle, IndicatorRelease(indicator_handle), _error))
   {
      PrintResponseError("IndicatorRelease", _response_error);
   }
}
   
void PrintParamError(string paramName)
{
   Print("[ERROR] parameter: ", paramName);
}

void PrintParamError(string commandName, string paramName, string error)
{
   PrintFormat("[ERROR] Command: %s, parameter: %s. %s", commandName, paramName, error);
}

void PrintResponseError(string commandName, string error = "")
{
   PrintFormat("[ERROR] response: %s. %s", commandName, error);
}

void ReadMqlTradeRequestFromCommand(MqlTradeRequest& request)
{
   string symbol;
   string comment;
   int tmpEnumValue;
   ulong magic = 0;
   StringInit(symbol, 100, 0);
   StringInit(comment, 1000, 0);
               
   getIntValue(ExpertHandle, 0, tmpEnumValue, _error);
   request.action = (ENUM_TRADE_REQUEST_ACTIONS)tmpEnumValue;      
   
   getULongValue(ExpertHandle, 1, magic, _error);     
   request.magic = magic;
   getULongValue(ExpertHandle, 2, request.order, _error);     
   getStringValue(ExpertHandle, 3, symbol, _error);
   request.symbol = symbol;
   getDoubleValue(ExpertHandle, 4, request.volume, _error);
   getDoubleValue(ExpertHandle, 5, request.price, _error);
   getDoubleValue(ExpertHandle, 6, request.stoplimit, _error);
   getDoubleValue(ExpertHandle, 7, request.sl, _error);
   getDoubleValue(ExpertHandle, 8, request.tp, _error);
   getULongValue(ExpertHandle, 9, request.deviation, _error);
   getIntValue(ExpertHandle, 10, tmpEnumValue, _error);      
   request.type = (ENUM_ORDER_TYPE)tmpEnumValue;      
   getIntValue(ExpertHandle, 11, tmpEnumValue, _error);
   request.type_filling = (ENUM_ORDER_TYPE_FILLING)tmpEnumValue;
   getIntValue(ExpertHandle, 12, tmpEnumValue, _error);
   request.type_time = (ENUM_ORDER_TYPE_TIME)tmpEnumValue;
   getIntValue(ExpertHandle, 13, tmpEnumValue, _error);
   request.expiration = (datetime)tmpEnumValue;  
   getStringValue(ExpertHandle, 14, comment, _error);         
   request.comment = comment;
   getULongValue(ExpertHandle, 15, request.position, _error);
   getULongValue(ExpertHandle, 16, request.position_by, _error);
}

string BoolToString(bool value)
{
   return value ? "1" : "0";
}

string ResultToString(bool retVal, MqlTradeResult& result)
{
   string strResult;
      
   StringConcatenate(strResult
      , BoolToString(retVal), PARAM_SEPARATOR
      , result.retcode, PARAM_SEPARATOR
      , result.deal, PARAM_SEPARATOR
      , result.order, PARAM_SEPARATOR
      , result.volume, PARAM_SEPARATOR
      , result.price, PARAM_SEPARATOR
      , result.bid, PARAM_SEPARATOR
      , result.ask, PARAM_SEPARATOR
      , result.comment, PARAM_SEPARATOR
      , result.request_id);
      
      return strResult;
}

string ResultToString(bool retVal, MqlTradeCheckResult& result)
{
   string strResult;
      
   StringConcatenate(strResult
      , BoolToString(retVal), PARAM_SEPARATOR
      , result.retcode, PARAM_SEPARATOR
      , result.balance, PARAM_SEPARATOR
      , result.equity, PARAM_SEPARATOR
      , result.profit, PARAM_SEPARATOR
      , result.margin, PARAM_SEPARATOR
      , result.margin_free, PARAM_SEPARATOR
      , result.margin_level, PARAM_SEPARATOR
      , result.comment, PARAM_SEPARATOR);
      
      return strResult;
}

string ResultToString(bool retVal, double result)
{
   string strResult;
      
   StringConcatenate(strResult
      , BoolToString(retVal), PARAM_SEPARATOR
      , result);
      
      return strResult;
}

string ResultToString(bool retVal, datetime from, datetime to)
{
   string strResult;
      
   StringConcatenate(strResult
      , BoolToString(retVal), PARAM_SEPARATOR
      , (int)from, PARAM_SEPARATOR
      , (int)to);
      
      return strResult;
}

bool OrderCloseAll()
{
   CTrade trade;
   int i = PositionsTotal()-1;
   while (i >= 0)
   {
      if (trade.PositionClose(PositionGetSymbol(i))) i--;
   }
   return true;
}

string OnRequest(string json)
{
   string response = "";

   JSONParser *parser = new JSONParser();
   JSONValue *jv = parser.parse(json);
   
   if(jv == NULL) 
   {   
      PrintFormat("%s [ERROR]: %d - %s", __FUNCTION__, (string)parser.getErrorCode(), parser.getErrorMessage());
   }
   else
   {
      if(jv.isObject()) 
      {
         JSONObject *jo = jv;
         int requestType = jo.getInt("RequestType");
     
         switch(requestType)
         {
            case 1: //CopyTicks
               response = ExecuteRequest_CopyTicks(jo);
               break;
            case 2: //iCustom
               response = ExecuteRequest_iCustom(jo);
               break;
            case 3: //OrderSend
               response = ExecuteRequest_OrderSend(jo);
               break;
            case 4: //PositionOpen
               response = ExecuteRequest_PositionOpen(jo);
               break;
            case 5: //OrderCheck
               response = ExecuteRequest_OrderCheck(jo);
               break;
            case 6: //MarketBookGet
               response = ExecuteRequest_MarketBookGet(jo);
               break;
            case 7: //IndicatorCreate
               response = ExecuteRequest_IndicatorCreate(jo);
               break;
            default:
               PrintFormat("%s [WARNING]: Unknown request type %d", __FUNCTION__, requestType);
               response = CreateErrorResponse(-1, "Unknown request type");
               break;
        }
      }
      
      delete jv;
   }   
   
   delete parser;
   
   return response;
}

string CreateErrorResponse(int code, string message_er)
{
   JSONValue* jsonError;
   if (code == 0)
      jsonError = new JSONString("0");
   else
      jsonError = new JSONNumber((long)code);
      
   JSONObject *joResponse = new JSONObject();
   joResponse.put("ErrorCode", jsonError);
   joResponse.put("ErrorMessage", new JSONString(message_er));
   
   string result = joResponse.toString();
   delete joResponse;
   return result;
}

string CreateSuccessResponse(string responseName, JSONValue* responseBody)
{
   JSONObject *joResponse = new JSONObject();
   joResponse.put("ErrorCode", new JSONString("0"));
      
   if (responseBody != NULL)
   {
      joResponse.put(responseName, responseBody);   
   }
   
   string result = joResponse.toString();   
   delete joResponse;   
   return result;
}

JSONObject* Serialize(MqlTick& tick)
{
    JSONObject *jo = new JSONObject();
    jo.put("MtTime", new JSONNumber(tick.time));
    jo.put("bid", new JSONNumber(tick.bid));
    jo.put("ask", new JSONNumber(tick.ask));
    jo.put("last", new JSONNumber(tick.last));
    jo.put("volume", new JSONNumber(tick.volume));
    return jo;
}

string ExecuteRequest_CopyTicks(JSONObject *jo)
{
   if (jo.getValue("SymbolName") == NULL)
      return CreateErrorResponse(-1, "Undefinded mandatory parameter SymbolName");
   if (jo.getValue("Flags") == NULL)
      return CreateErrorResponse(-1, "Undefinded mandatory parameter Flags");
   if (jo.getValue("From") == NULL)
      return CreateErrorResponse(-1, "Undefinded mandatory parameter From");
   if (jo.getValue("Count") == NULL)
      return CreateErrorResponse(-1, "Undefinded mandatory parameter Count");      

   string symbol = jo.getString("SymbolName");
   uint flags = jo.getInt("Flags");
   int from = jo.getInt("From");
   int count = jo.getInt("Count");
   
   MqlTick ticks[];
   int received = CopyTicks(symbol, ticks, flags, from, count); 
   if(received == -1)
      return CreateErrorResponse(GetLastError(), "CopyTicks failed");
   
   JSONArray* jaTicks = new JSONArray();
   for(int i = 0; i < received; i++)
   {
      jaTicks.put(i, Serialize(ticks[i]));
   }
        
   return CreateSuccessResponse("Value", jaTicks);
}

string ExecuteRequest_iCustom(JSONObject *jo)
{
   if (jo.getValue("Symbol") == NULL)
      return CreateErrorResponse(-1, "Undefinded mandatory parameter Symbol");
   if (jo.getValue("Timeframe") == NULL)
      return CreateErrorResponse(-1, "Undefinded mandatory parameter Timeframe");
   if (jo.getValue("Name") == NULL)
      return CreateErrorResponse(-1, "Undefinded mandatory parameter Name");
   
   string symbol = jo.getString("Symbol");   
   int timeframe = jo.getInt("Timeframe");
   string name = jo.getString("Name");

   int result;
   
   if (jo.getValue("Params") == NULL)
   {
      result = iCustom(symbol, (ENUM_TIMEFRAMES)timeframe, name);
   }
   else
   {
      JSONArray *jaParams = jo.getArray("Params");
      int size = jaParams.size();

      if (size < 0 || size > 10)
         return CreateErrorResponse(-1, "Parameter's count is out of range.");

      if (jo.getValue("ParamsType") == NULL)
         return CreateErrorResponse(-1, "Undefinded mandatory parameter ParamsType");

      int paramsType =  jo.getInt("ParamsType");
      switch (paramsType)
      {
      case 0: //Int
      {
         int intParams[];
         ArrayResize(intParams, size);
         for (int i = 0; i < size; i++)
         {
            intParams[i] = jaParams.getInt(i);
         }
         result = iCustomT(symbol, (ENUM_TIMEFRAMES)timeframe, name, intParams, size);
      }
      break;
      case 1: //Double
      {
         int doubleParams[];
         ArrayResize(doubleParams, size);
         result = iCustomT(symbol, (ENUM_TIMEFRAMES)timeframe, name, doubleParams, size);
      }
      break;
      case 2: //String
      {
         string stringParams[];
         ArrayResize(stringParams, size);
         result = iCustomT(symbol, (ENUM_TIMEFRAMES)timeframe, name, stringParams, size);
      }
      break;
      case 3: //Boolean
      {
         string boolParams[];
         ArrayResize(boolParams, size);
         result = iCustomT(symbol, (ENUM_TIMEFRAMES)timeframe, name, boolParams, size);
      }
      break;
      default:
         return CreateErrorResponse(-1, "Unsupported type of iCustom parameters.");
      }
   }

   return CreateSuccessResponse("Value", new JSONNumber((long)result));
}

template<typename T>
int iCustomT(string symbol, ENUM_TIMEFRAMES timeframe, string name, T &p[], int count)
{
   switch(count)
   {
   case 0:
      return iCustom(symbol, timeframe, name);
   case 1:
      return iCustom(symbol, timeframe, name, p[0]);
   case 2:
      return iCustom(symbol, timeframe, name, p[0], p[1]);
   case 3:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2]);
   case 4:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], p[3]);
   case 5:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], p[3], p[4]);
   case 6:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], p[3], p[4], p[5]);
   case 7:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], p[3], p[4], p[5], p[6]);
   case 8:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7]);
   case 9:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8]);
   case 10:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9]);
   default:
         return 0;
   }
}

#define PRINT_MSG_AND_RETURN_VALUE(msg,value) PrintFormat("%s: %s",__FUNCTION__,msg);return value
#define CHECK_JSON_VALUE(jo, name_value, return_fail_result) if (jo.getValue(name_value) == NULL) { PRINT_MSG_AND_RETURN_VALUE(StringFormat("failed to get %s from JSON!", name_value), return_fail_result); }

bool JsonToMqlTradeRequest(JSONObject *jo, MqlTradeRequest& request)
{
   //Action
   CHECK_JSON_VALUE(jo, "Action", false);
   request.action = (ENUM_TRADE_REQUEST_ACTIONS) jo.getInt("Action");
   
   //Magic
   CHECK_JSON_VALUE(jo, "Magic", false);
   request.magic = jo.getLong("Magic");
   
   //Order
   CHECK_JSON_VALUE(jo, "Order", false);
   request.order = jo.getLong("Magic");
   
   //Symbol
   CHECK_JSON_VALUE(jo, "Symbol", false);
   StringInit(request.symbol, 100, 0);
   request.symbol = jo.getString("Symbol");
   
   //Volume
   CHECK_JSON_VALUE(jo, "Volume", false);
   request.volume = jo.getDouble("Volume");

   //Price
   CHECK_JSON_VALUE(jo, "Price", false);
   request.price = jo.getDouble("Price");
   
   //Stoplimit
   CHECK_JSON_VALUE(jo, "Stoplimit", false);
   request.stoplimit = jo.getDouble("Stoplimit");
   
   //Sl
   CHECK_JSON_VALUE(jo, "Sl", false);
   request.sl = jo.getDouble("Sl");
   
   //Tp
   CHECK_JSON_VALUE(jo, "Tp", false);
   request.tp = jo.getDouble("Tp");

   //Deviation
   CHECK_JSON_VALUE(jo, "Deviation", false);
   request.deviation = jo.getLong("Deviation");

   //Type
   CHECK_JSON_VALUE(jo, "Type", false);
   request.type = (ENUM_ORDER_TYPE)jo.getInt("Type");
   
   //Type_filling
   CHECK_JSON_VALUE(jo, "Type_filling", false);
   request.type_filling = (ENUM_ORDER_TYPE_FILLING)jo.getInt("Type_filling");

   //Type_time
   CHECK_JSON_VALUE(jo, "Type_time", false);
   request.type_time = (ENUM_ORDER_TYPE_TIME)jo.getInt("Type_time");
   
   //MtExpiration
   CHECK_JSON_VALUE(jo, "MtExpiration", false);
   request.expiration = (datetime)jo.getInt("MtExpiration");

   //Comment
   if (jo.getValue("Comment") != NULL)
   {
      StringInit(request.comment, 1000, 0);
      request.comment = jo.getString("Comment");
   }
   
   //Position
   CHECK_JSON_VALUE(jo, "Position", false);
   request.position = jo.getLong("Position");

   //PositionBy
   CHECK_JSON_VALUE(jo, "PositionBy", false);
   request.position_by = jo.getLong("PositionBy");
   
   return true;
}

JSONObject* MqlTradeResultToJson(MqlTradeResult& result)
{
   JSONObject* jo = new JSONObject();
   
   jo.put("Retcode", new JSONNumber(result.retcode));
   jo.put("Deal", new JSONNumber(result.deal));
   jo.put("Order", new JSONNumber(result.order));
   jo.put("Volume", new JSONNumber(result.volume));
   jo.put("Price", new JSONNumber(result.price));
   jo.put("Bid", new JSONNumber(result.bid));
   jo.put("Ask", new JSONNumber(result.ask));
   jo.put("Comment", new JSONString(result.comment));
   jo.put("Request_id", new JSONNumber(result.request_id));
   
   return jo;
}

string ExecuteRequest_OrderSend(JSONObject *jo)
{
   CHECK_JSON_VALUE(jo, "TradeRequest", CreateErrorResponse(-1, "Undefinded mandatory parameter TradeRequest"));
   JSONObject* trade_request_jo = jo.getObject("TradeRequest");
      
   MqlTradeRequest trade_request = {0};
   JsonToMqlTradeRequest(trade_request_jo, trade_request);
   
   MqlTradeResult trade_result = {0};   
   bool ok = OrderSend(trade_request, trade_result);
   
   JSONObject* result_value_jo = new JSONObject();
   result_value_jo.put("RetVal", new JSONBool(ok));
   result_value_jo.put("TradeResult", MqlTradeResultToJson(trade_result));
   
#ifdef __DEBUG_LOG__   
   PrintFormat("%s: return value = %s", __FUNCTION__, ok ? "true" : "false");
#endif    
      
   return CreateSuccessResponse("Value", result_value_jo);
}

string ExecuteRequest_PositionOpen(JSONObject *jo)
{   
   //Symbol
   CHECK_JSON_VALUE(jo, "Symbol", CreateErrorResponse(-1, "Undefinded mandatory parameter Symbol"));
   string symbol = jo.getString("Symbol");
     
   //OrderType 
   CHECK_JSON_VALUE(jo, "OrderType", CreateErrorResponse(-1, "Undefinded mandatory parameter OrderType"));
   ENUM_ORDER_TYPE order_type = (ENUM_ORDER_TYPE) jo.getInt("OrderType");
   
   //Volume
   CHECK_JSON_VALUE(jo, "Volume", CreateErrorResponse(-1, "Undefinded mandatory parameter Volume"));
   double volume = jo.getDouble("Volume");

   //Price
   CHECK_JSON_VALUE(jo, "Price", CreateErrorResponse(-1, "Undefinded mandatory parameter Price"));
   double price = jo.getDouble("Price");

   //Sl
   CHECK_JSON_VALUE(jo, "Sl", CreateErrorResponse(-1, "Undefinded mandatory parameter Sl"));
   double sl = jo.getDouble("Sl");
   
   //Tp
   CHECK_JSON_VALUE(jo, "Tp", CreateErrorResponse(-1, "Undefinded mandatory parameter Tp"));
   double tp = jo.getDouble("Tp");
   
   //Comment
   string comment;
   if (jo.getValue("Comment") != NULL)
   {
      comment = jo.getString("Comment");
   }   

#ifdef __DEBUG_LOG__
   PrintFormat("%s: symbol = %s, order_type = %d, volume = %f, price = %f, sl = %f, tp = %f, comment = %s", 
      __FUNCTION__, symbol, order_type, volume, price, sl, tp, comment);
#endif

   CTrade trade;
   bool ok = trade.PositionOpen(symbol, order_type, volume, price, sl, tp, comment);

   MqlTradeResult trade_result={0};
   trade.Result(trade_result);

   JSONObject* result_value_jo = new JSONObject();
   result_value_jo.put("RetVal", new JSONBool(ok));
   result_value_jo.put("TradeResult", MqlTradeResultToJson(trade_result));

   return CreateSuccessResponse("Value", result_value_jo);   
}

JSONObject* MqlTradeCheckResultToJson(MqlTradeCheckResult& result)
{
   JSONObject* jo = new JSONObject();
   
   jo.put("Retcode", new JSONNumber(result.retcode));
   jo.put("Balance", new JSONNumber(result.balance));
   jo.put("Equity", new JSONNumber(result.equity));
   jo.put("Profit", new JSONNumber(result.profit));
   jo.put("Margin", new JSONNumber(result.margin));
   jo.put("Margin_free", new JSONNumber(result.margin_free));
   jo.put("Margin_level", new JSONNumber(result.margin_level));
   jo.put("Comment", new JSONString(result.comment));
   
   return jo;
}

string ExecuteRequest_OrderCheck(JSONObject *jo)
{
   CHECK_JSON_VALUE(jo, "TradeRequest", CreateErrorResponse(-1, "Undefinded mandatory parameter TradeRequest"));
   JSONObject* trade_request_jo = jo.getObject("TradeRequest");
      
   MqlTradeRequest trade_request = {0};
   JsonToMqlTradeRequest(trade_request_jo, trade_request);
   
   MqlTradeCheckResult trade_check_result = {0};   
   bool ok = OrderCheck(trade_request, trade_check_result);
   
#ifdef __DEBUG_LOG__   
   PrintFormat("%s: return value = %s", __FUNCTION__, ok ? "true" : "false");
#endif    

   JSONObject* result_value_jo = new JSONObject();
   result_value_jo.put("RetVal", new JSONBool(ok));
   result_value_jo.put("TradeCheckResult", MqlTradeCheckResultToJson(trade_check_result));
   
   return CreateSuccessResponse("Value", result_value_jo);   
}

JSONObject* MqlBookInfoToJson(MqlBookInfo& info)
{
    JSONObject *jo = new JSONObject();
    jo.put("type", new JSONNumber((int)info.type));
    jo.put("price", new JSONNumber(info.price));
    jo.put("volume", new JSONNumber(info.volume));
    return jo;
}

string ExecuteRequest_MarketBookGet(JSONObject *jo)
{
   CHECK_JSON_VALUE(jo, "Symbol", CreateErrorResponse(-1, "Undefinded mandatory parameter Symbol"));
   string symbol = jo.getString("Symbol");
   
   MqlBookInfo info_array[];
   bool ok = MarketBookGet(symbol, info_array); 

#ifdef __DEBUG_LOG__   
   PrintFormat("%s: return value = %s.", __FUNCTION__, ok ? "true" : "false");
#endif
   
   if (!ok)
      return CreateErrorResponse(GetLastError(), "MarketBookGet failed");
   
   int size = ArraySize(info_array);
   JSONArray* book_ja = new JSONArray();
   for(int i = 0; i < size; i++)
   {
      book_ja.put(i, MqlBookInfoToJson(info_array[i]));
   }

#ifdef __DEBUG_LOG__   
   PrintFormat("%s: array size = %d.", __FUNCTION__, size);
#endif   
        
   return CreateSuccessResponse("Value", book_ja);
}

string ExecuteRequest_IndicatorCreate(JSONObject *jo)
{
   //Symbol
   string symbol;
   if (jo.getValue("Symbol") != NULL)
   {
      symbol = jo.getString("Symbol");
   }
   
   CHECK_JSON_VALUE(jo, "Period", CreateErrorResponse(-1, "Undefinded mandatory parameter Period"));
   ENUM_TIMEFRAMES period = (ENUM_TIMEFRAMES) jo.getInt("Period");
   
   CHECK_JSON_VALUE(jo, "IndicatorType", CreateErrorResponse(-1, "Undefinded mandatory parameter IndicatorType"));
   ENUM_INDICATOR indicator_type = (ENUM_INDICATOR) jo.getInt("IndicatorType");
   
   int indicator_handle = -1;
   if (jo.getValue("Parameters") != NULL)
   {
      JSONArray parameters_ja = jo.getArray("Parameters");
      int size = parameters_ja.size();
      if (size > 0)
      {
         MqlParam parameters[];
         ArrayResize(parameters, size);
         
         for (int i = 0; i < size; i++)
         {
            JSONObject param_jo = parameters_ja.getObject(i);
            
            parameters[i].type = (ENUM_DATATYPE)param_jo.getInt("DataType");
            if (param_jo.getValue("IntegerValue") != NULL)
            {
               parameters[i].integer_value = param_jo.getLong("IntegerValue");
            }
            if (param_jo.getValue("DoubleValue") != NULL)
            {
               parameters[i].double_value = param_jo.getDouble("DoubleValue");
            }
            if (param_jo.getValue("StringValue") != NULL)
            {
               parameters[i].string_value = param_jo.getString("StringValue");
            }
         }
         
#ifdef __DEBUG_LOG__   
   PrintFormat("%s: symbol = %s, period = %d, indicator_type = %d, size = %d.", __FUNCTION__, symbol, period, indicator_type, size);
#endif   
         
         indicator_handle = IndicatorCreate(symbol, period, indicator_type, size, parameters);
      }
   }
   else
   {
#ifdef __DEBUG_LOG__   
      PrintFormat("%s: symbol = %s, period = %d, indicator_type = %d.", __FUNCTION__, symbol, period, indicator_type);
#endif
   
      indicator_handle = IndicatorCreate(symbol, period, indicator_type);
   }
   
#ifdef __DEBUG_LOG__   
      PrintFormat("%s: result indicator handle = %d", __FUNCTION__, indicator_handle);
#endif
   
   return CreateSuccessResponse("Value", new JSONNumber(indicator_handle));
}