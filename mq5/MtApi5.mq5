#property copyright "Vyacheslav Demidyuk"
#property link      ""

#property version   "1.3"
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

input int Port = 8228;

int ExpertHandle;

//string message;
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
   
   PrintFormat("Expert Handle = %d", ExpertHandle);
   
   //--- Backtesting mode
   if (false)
   {
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
   
   if (commandType > 0)
   {
      Print("executeCommand: commnad type = ", commandType);
   }
  
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
   case 62: //MarketBookGet
      Execute_MarketBookGet();
   break;   
   case 65: //PositionOpen
      Execute_PositionOpen();
   break;   
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
      Print("Execute_Request: incoming request = ", request);
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
   
   if (!sendULongResponse(ExpertHandle, HistoryOrderGetInteger(ticket_number, (ENUM_ORDER_PROPERTY_INTEGER)property_id), _response_error))
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

void Execute_MarketBookGet()
{
   string symbol;
   StringInit(symbol, 100, 0);
   
   if (!getStringValue(ExpertHandle, 0, symbol, _error))
   {
      PrintParamError("MarketBookGet", "symbol", _error);
      sendErrorResponse(ExpertHandle, -1, _error, _response_error);
      return;
   }
   
   MqlBookInfo book[];
   bool retVal = MarketBookGet(symbol, book); 
   
   if(retVal)
   {
      int size = ArraySize(book);
      if (!sendMqlBookInfoArrayResponse(ExpertHandle, book, size, _response_error))
      {
         PrintResponseError("MarketBookGet", _response_error);
      }
   }
   else
   {
      if (!sendVoidResponse(ExpertHandle, _response_error))
      {
         PrintResponseError("MarketBookGet", _response_error);
      }
   }
}

void Execute_PositionOpen()
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
   
   PrintFormat("Execute_PositionOpen: symbol = %s, order_type = %d, volume = %f, price = %f, sl = %f, tp = %f, comment = %s", 
      symbol, order_type, volume, price, sl, tp, comment);
   
   CTrade trade;
   bool result = trade.PositionOpen(symbol, (ENUM_ORDER_TYPE)order_type, volume, price, sl, tp, comment);
   if (!sendBooleanResponse(ExpertHandle, result, _response_error))
   {
      PrintResponseError("PositionOpen", _response_error);
   }
   
   Print("command PositionOpen: result = ", result);
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
      PrintFormat("OnRequest [ERROR]: %d - %s", (string)parser.getErrorCode(), parser.getErrorMessage());
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
            default:
               PrintFormat("OnRequest [WARNING]: Unknown request type %d", requestType);
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
        
   return CreateSuccessResponse("Ticks", jaTicks);;
}