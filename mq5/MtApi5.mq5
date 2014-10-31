#property copyright "Vyacheslav Demidyuk"
#property link      ""

#include <Trade\SymbolInfo.mqh>

#import "MT5Connector.dll"
   bool initExpert(int expertHandle, string connectionProfile, string symbol, double bid, double ask, string& err);
   bool deinitExpert(int expertHandle, string& err);
   
   bool updateQuote(int expertHandle, string symbol, double bid, double ask, string& err);
      
   bool sendIntResponse(int expertHandle, int response);
   bool sendBooleanResponse(int expertHandle, int response);
   bool sendDoubleResponse(int expertHandle, double response);
   bool sendStringResponse(int expertHandle, string response);
   bool sendVoidResponse(int expertHandle);
   bool sendDoubleArrayResponse(int expertHandle, double& values[], int size);
   bool sendIntArrayResponse(int expertHandle, int& values[], int size);   
   bool sendLongResponse(int expertHandle, long response);
   bool sendULongResponse(int expertHandle, ulong response);
   bool sendLongArrayResponse(int expertHandle, long& values[], int size);
   bool sendMqlRatesArrayResponse(int expertHandle, MqlRates& values[], int size);   
   bool sendMqlTickResponse(int expertHandle, MqlTick& response);
   bool sendMqlBookInfoArrayResponse(int expertHandle, MqlBookInfo& values[], int size);   
   
   bool getCommandType(int expertHandle, int& res);
   bool getIntValue(int expertHandle, int paramIndex, int& res);
   bool getUIntValue(int expertHandle, int paramIndex, uint& res);   
   bool getULongValue(int expertHandle, int paramIndex, ulong& res);
   bool getLongValue(int expertHandle, int paramIndex, long& res);
   bool getDoubleValue(int expertHandle, int paramIndex, double& res);
   bool getStringValue(int expertHandle, int paramIndex, string& res);
   bool getBooleanValue(int expertHandle, int paramIndex, bool& res);
   
   void verify(bool isDemo, string accountName, long accountNumber);   
#import

input string ConnectionProfile = "Local5";

int ExpertHandle;

string message;
bool isCrashed = false;

string symbolValue;
string commentValue;

double myBid;
double myAsk;

string PARAM_SEPARATOR = ";";

int OnInit()
{
   int result = init();  
   return (result);
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
   message        = "111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111" + "";
   symbolValue    = "222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222" + "";
   commentValue   = "333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333" + "";

   return (0);
}

bool IsDemo()
{
   if(AccountInfoInteger(ACCOUNT_TRADE_MODE)==ACCOUNT_TRADE_MODE_DEMO)
      return(true);
   else
      return(false);
}

int init() 
{
   preinit();
   
   verify(IsDemo(), AccountInfoString(ACCOUNT_NAME), AccountInfoInteger(ACCOUNT_LOGIN));

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

   long chartID= ChartID();
   ExpertHandle = ChartGetInteger(chartID, CHART_WINDOW_HANDLE);
   
   MqlTick last_tick;
   SymbolInfoTick(Symbol(),last_tick);
   double Bid = last_tick.bid;
   double Ask = last_tick.ask;
   
   myBid = Bid;
   myAsk = Ask;

   if (!initExpert(ExpertHandle, ConnectionProfile, Symbol(), Bid, Ask, message))
   {
       MessageBox(message, "MtApi", 0);
       isCrashed = true;
       return(1);
   }
   
   if (executeCommand() == 1)
   {   
      isCrashed = true;
      return (1);
   }

   return (0);
}

int deinit() 
{
   if (isCrashed == 0) 
   {
      if (!deinitExpert(ExpertHandle, message)) 
      {
         MessageBox(message, "MtApi", 0);
         isCrashed = true;
         return (1);
      }
   }
   
   return (0);
}

int start() 
{
   if (isCrashed == 0) 
   {   
      MqlTick last_tick;
      SymbolInfoTick(Symbol(),last_tick);
      double Bid = last_tick.bid;
      double Ask = last_tick.ask;
   
      if (executeCommand() != 0)
      {  
         CSymbolInfo mysymbol;
         mysymbol.Name (_Symbol);
         mysymbol.RefreshRates();

         Bid = mysymbol.Bid();
         Ask =  mysymbol.Ask();
                           
         if (myBid == Bid && myAsk == Ask)
         {         
            return (0);
         }
      }      
      
      if (!updateQuote(ExpertHandle, Symbol(), Bid, Ask, message)) 
      {
         MessageBox(message, "MtApi", 0);
         isCrashed = true;
         return (1);
      }
      
      myBid = Bid;
      myAsk = Ask;
   }

   return (0);
}

int executeCommand()
{
   int commandType = 0;

   if (!getCommandType(ExpertHandle, commandType))
   {
      return (0);
   }            
  
   switch (commandType) 
   {
   case 0:
      //NoCommand         
      break;
      
   case 1: // OrderSend
   {
      MqlTradeRequest request={0};      
      ReadMqlTradeRequestFromCommand(request);
      
      MqlTradeResult result={0};
      
      bool retVal = OrderSend(request, result);
            
      sendStringResponse(ExpertHandle, ResultToString(retVal, result));
   }
   break;   
      
   case 2: // OrderCalcMargin
   {
      ENUM_ORDER_TYPE action;
      double volume;
      double price;      
      int tmpEnumValue;
      double margin;
      bool retVal;
      
      getIntValue(ExpertHandle, 0, tmpEnumValue);
      action = (ENUM_ORDER_TYPE)tmpEnumValue;   
               
      getStringValue(ExpertHandle, 1, symbolValue);
      getDoubleValue(ExpertHandle, 2, volume);
      getDoubleValue(ExpertHandle, 3, price);      
      
      retVal = OrderCalcMargin(action, symbolValue, volume, price, margin);
               
      sendStringResponse(ExpertHandle, ResultToString(retVal, margin));
   }
   break;
   
   case 3: //OrderCalcProfit
   {
      int tmpEnumValue;
      ENUM_ORDER_TYPE action;
      double volume;
      double price_open;            
      double price_close;
      double profit;
      bool retVal;
      
      getIntValue(ExpertHandle, 0, tmpEnumValue);
      action = (ENUM_ORDER_TYPE)tmpEnumValue;   
               
      getStringValue(ExpertHandle, 1, symbolValue);
      getDoubleValue(ExpertHandle, 2, volume);
      getDoubleValue(ExpertHandle, 3, price_open);
      getDoubleValue(ExpertHandle, 4, price_close);
      
      retVal = OrderCalcProfit(action, symbolValue, volume, price_open, price_close, profit);
               
      sendStringResponse(ExpertHandle, ResultToString(retVal, profit));
   }
   break;
   
   case 4: //OrderCheck
   {
      MqlTradeRequest request={0};      
      ReadMqlTradeRequestFromCommand(request);
      
      MqlTradeCheckResult result={0};
      
      bool retVal = OrderCheck(request, result);
            
      sendStringResponse(ExpertHandle, ResultToString(retVal, result));
   }
   break;  
   
   case 6: //PositionsTotal
   {
      sendIntResponse(ExpertHandle, PositionsTotal());
   }
   break;  
   
   case 7: //PositionGetSymbol
   {
      int index;
      getIntValue(ExpertHandle, 0, index);
   
      sendStringResponse(ExpertHandle, PositionGetSymbol(index));
   }
   
   case 8: //PositionSelect
   {
      getStringValue(ExpertHandle, 0, symbolValue);
   
      sendBooleanResponse(ExpertHandle, PositionSelect(symbolValue));
   }
   break;  
   
   case 9: //PositionGetDouble
   {
      ENUM_POSITION_PROPERTY_DOUBLE property_id;
      int tmpEnumValue;
      
      getIntValue(ExpertHandle, 0, tmpEnumValue);      
      property_id = (ENUM_POSITION_PROPERTY_DOUBLE) tmpEnumValue;
   
      sendDoubleResponse(ExpertHandle, PositionGetDouble(property_id));
   }
   
   case 10: //PositionGetInteger
   {
      ENUM_POSITION_PROPERTY_INTEGER property_id;
      int tmpEnumValue;
      
      getIntValue(ExpertHandle, 0, tmpEnumValue);      
      property_id = (ENUM_POSITION_PROPERTY_INTEGER) tmpEnumValue;
   
      sendLongResponse(ExpertHandle, PositionGetInteger(property_id));
   }
   break;  
   
   case 11: //PositionGetString
   {
      ENUM_POSITION_PROPERTY_STRING property_id;
      int tmpEnumValue;
      
      getIntValue(ExpertHandle, 0, tmpEnumValue);      
      property_id = (ENUM_POSITION_PROPERTY_STRING) tmpEnumValue;
   
      sendStringResponse(ExpertHandle, PositionGetString(property_id));
   }
   break;  
   
   case 12: //OrdersTotal
   {
      sendIntResponse(ExpertHandle, OrdersTotal());
   }
   break;
   
   case 13: //OrderGetTicket
   {
      int index;
      getIntValue(ExpertHandle, 0, index);
      
      sendULongResponse(ExpertHandle, OrderGetTicket(index));
   }
   break;
   
   case 14: //OrderSelect
   {
      ulong ticket;
      getULongValue(ExpertHandle, 0, ticket);
      
      sendBooleanResponse(ExpertHandle, OrderSelect(ticket));
   }
   break;
   
   case 15: //OrderGetDouble
   {
      ENUM_ORDER_PROPERTY_DOUBLE property_id;
      int tmpEnumValue;

      getIntValue(ExpertHandle, 0, tmpEnumValue);      
      property_id = (ENUM_ORDER_PROPERTY_DOUBLE) tmpEnumValue;
      
      sendDoubleResponse(ExpertHandle, OrderGetDouble(property_id));
   }
   break;
   
   case 16: //OrderGetInteger
   {
      ENUM_ORDER_PROPERTY_INTEGER property_id;
      int tmpEnumValue;

      getIntValue(ExpertHandle, 0, tmpEnumValue);      
      property_id = (ENUM_ORDER_PROPERTY_INTEGER) tmpEnumValue;
      
      sendLongResponse(ExpertHandle, OrderGetInteger(property_id));
   }
   break;
   
   case 17: //OrderGetString
   {
      ENUM_ORDER_PROPERTY_STRING property_id;
      int tmpEnumValue;

      getIntValue(ExpertHandle, 0, tmpEnumValue);      
      property_id = (ENUM_ORDER_PROPERTY_STRING) tmpEnumValue;
      
      sendStringResponse(ExpertHandle, OrderGetString(property_id));
   }
   break;
   
   case 18: //HistorySelect
   {
      datetime from_date;
      datetime to_date;
      int tmpDateValue;

      getIntValue(ExpertHandle, 0, tmpDateValue);       
      from_date = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 1, tmpDateValue);      
      to_date = (datetime)tmpDateValue;
      
      sendBooleanResponse(ExpertHandle, HistorySelect(from_date, to_date));
   }
   break;
   
   case 19: //HistorySelectByPosition
   {
      long position_id;
      getLongValue(ExpertHandle, 0, position_id);
      
      sendBooleanResponse(ExpertHandle, HistorySelectByPosition(position_id));
   }
   break;
   
   case 20: //HistoryOrderSelect
   {
      ulong ticket;
      getULongValue(ExpertHandle, 0, ticket);
      
      sendBooleanResponse(ExpertHandle, HistoryOrderSelect(ticket));
   }
   break;
   
   case 21: //HistoryOrdersTotal
   {
      sendIntResponse(ExpertHandle, HistoryOrdersTotal());
   }
   break;
   
   case 22: //HistoryOrderGetTicket
   {
      int index;
      getIntValue(ExpertHandle, 0, index);
      
      sendULongResponse(ExpertHandle, HistoryOrderGetTicket(index));
   }
   break;
   
   case 23: //HistoryOrderGetDouble
   {
      ulong ticket_number;
      ENUM_ORDER_PROPERTY_DOUBLE property_id;
      int tmpEnumValue;
      
      getULongValue(ExpertHandle, 0, ticket_number);
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      property_id = (ENUM_ORDER_PROPERTY_DOUBLE) tmpEnumValue;
      
      sendDoubleResponse(ExpertHandle, HistoryOrderGetDouble(ticket_number, property_id));
   }
   break;
   
   case 24: //HistoryOrderGetInteger
   {
      ulong ticket_number;
      ENUM_ORDER_PROPERTY_INTEGER property_id;
      int tmpEnumValue;
      
      getULongValue(ExpertHandle, 0, ticket_number);
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      property_id = (ENUM_ORDER_PROPERTY_INTEGER) tmpEnumValue;
      
      sendULongResponse(ExpertHandle, HistoryOrderGetInteger(ticket_number, property_id));
   }
   break;
      
   case 25: //HistoryOrderGetString
   {
      ulong ticket_number;
      ENUM_ORDER_PROPERTY_STRING property_id;
      int tmpEnumValue;
      
      getULongValue(ExpertHandle, 0, ticket_number);
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      property_id = (ENUM_ORDER_PROPERTY_STRING) tmpEnumValue;
      
      sendStringResponse(ExpertHandle, HistoryOrderGetString(ticket_number, property_id));
   }
   break;
   
   case 26: //HistoryDealSelect
   {
      ulong ticket;
            
      getULongValue(ExpertHandle, 0, ticket);
      
      sendBooleanResponse(ExpertHandle, HistoryDealSelect(ticket));
   }
   break;
   
   case 27: //HistoryDealsTotal
   {
      sendIntResponse(ExpertHandle, HistoryDealsTotal());
   }
   break;   
   
   case 28: //HistoryDealGetTicket
   {
      uint index;            
      getIntValue(ExpertHandle, 0, index);
      
      sendULongResponse(ExpertHandle, HistoryDealGetTicket(index));
   }
   break;   
   
   case 29: //HistoryDealGetDouble
   {
      ulong ticket_number;
      ENUM_DEAL_PROPERTY_DOUBLE property_id;
      int tmpEnumValue;
      
      getULongValue(ExpertHandle, 0, ticket_number);
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      property_id = (ENUM_DEAL_PROPERTY_DOUBLE) tmpEnumValue;
      
      sendDoubleResponse(ExpertHandle, HistoryDealGetDouble(ticket_number, property_id));
   }
   break;   
   
   case 30: //HistoryDealGetInteger
   {
      ulong ticket_number;
      ENUM_DEAL_PROPERTY_INTEGER property_id;
      int tmpEnumValue;
      
      getULongValue(ExpertHandle, 0, ticket_number);
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      property_id = (ENUM_DEAL_PROPERTY_INTEGER) tmpEnumValue;
      
      sendLongResponse(ExpertHandle, HistoryDealGetInteger(ticket_number, property_id));
   }
   break;   
   
   case 31: //HistoryDealGetString
   {
      ulong ticket_number;
      ENUM_DEAL_PROPERTY_STRING property_id;
      int tmpEnumValue;
      
      getULongValue(ExpertHandle, 0, ticket_number);
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      property_id = (ENUM_DEAL_PROPERTY_STRING) tmpEnumValue;
      
      sendStringResponse(ExpertHandle, HistoryDealGetString(ticket_number, property_id));
   }
   break;      

   case 32: //AccountInfoDouble
   {
      ENUM_ACCOUNT_INFO_DOUBLE property_id;
      int tmpEnumValue;
      
      getIntValue(ExpertHandle, 0, tmpEnumValue);      
      property_id = (ENUM_ACCOUNT_INFO_DOUBLE) tmpEnumValue;
      
      sendDoubleResponse(ExpertHandle, AccountInfoDouble(property_id));
   }
   break;      

   case 33: //AccountInfoInteger
   {
      ENUM_ACCOUNT_INFO_INTEGER property_id;
      int tmpEnumValue;
      
      getIntValue(ExpertHandle, 0, tmpEnumValue);      
      property_id = (ENUM_ACCOUNT_INFO_INTEGER) tmpEnumValue;
      
      sendLongResponse(ExpertHandle, AccountInfoInteger(property_id));
   }
   break;   

   case 34: //AccountInfoString
   {
      ENUM_ACCOUNT_INFO_STRING property_id;
      int tmpEnumValue;
      
      getIntValue(ExpertHandle, 0, tmpEnumValue);      
      property_id = (ENUM_ACCOUNT_INFO_STRING) tmpEnumValue;
      
      sendStringResponse(ExpertHandle, AccountInfoString(property_id));
   }
   break; 
   
   case 35: //SeriesInfoInteger
   {
      ENUM_TIMEFRAMES timeframe;
      ENUM_SERIES_INFO_INTEGER prop_id;
      int tmpEnumValue;
      
      getStringValue(ExpertHandle, 0, symbolValue);      
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpEnumValue);
      prop_id = (ENUM_SERIES_INFO_INTEGER) tmpEnumValue;
      
      sendLongResponse(ExpertHandle, SeriesInfoInteger(symbolValue, timeframe, prop_id));
   }
   break; 
   
   case 36: //Bars
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      
      getStringValue(ExpertHandle, 0, symbolValue);      
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
            
      sendIntResponse(ExpertHandle, Bars(symbolValue, timeframe));
   }
   break; 
   
   case 1036: //Bars2
   {
      ENUM_TIMEFRAMES timeframe;      
      datetime start_time;
      datetime stop_time;
      int tmpEnumValue;
      int tmpDateValue;      
      
      getStringValue(ExpertHandle, 0, symbolValue);      
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);       
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, tmpDateValue);      
      stop_time = (datetime)tmpDateValue;

            
      sendIntResponse(ExpertHandle, Bars(symbolValue, timeframe, start_time, stop_time));
   }
   break; 
      
   case 37: //BarsCalculated
   {
      int indicator_handle;
      
      getIntValue(ExpertHandle, 0, indicator_handle);
            
      sendIntResponse(ExpertHandle, BarsCalculated(indicator_handle));
   }
   break; 
   
   case 40: //CopyBuffer
   {
      int indicator_handle;
      int buffer_num;
      int start_pos;
      int count;
      double buffer[];

      getIntValue(ExpertHandle, 0, indicator_handle);
      getIntValue(ExpertHandle, 1, buffer_num);
      getIntValue(ExpertHandle, 2, start_pos);
      getIntValue(ExpertHandle, 3, count);
            
      int copied = CopyBuffer(indicator_handle, buffer_num, start_pos, count, buffer);
      
      sendDoubleArrayResponse(ExpertHandle, buffer, copied);
   }
   break; 
   
   case 1040: //CopyBuffer1
   {
      int indicator_handle;
      int buffer_num;
      datetime start_time;
      int count;
      double buffer[];
      int tmpDateValue;

      getIntValue(ExpertHandle, 0, indicator_handle);
      getIntValue(ExpertHandle, 1, buffer_num);
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      getIntValue(ExpertHandle, 3, count);
            
      int copied = CopyBuffer(indicator_handle, buffer_num, start_time, count, buffer);
      
      sendDoubleArrayResponse(ExpertHandle, buffer, copied);
   }
   break; 
   
   case 1140: //CopyBuffer2
   {
      int indicator_handle;
      int buffer_num;
      datetime start_time;
      datetime stop_time;
      double buffer[];
      int tmpDateValue;

      getIntValue(ExpertHandle, 0, indicator_handle);
      getIntValue(ExpertHandle, 1, buffer_num);
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      getIntValue(ExpertHandle, 3, tmpDateValue);
      stop_time = (datetime)tmpDateValue;
            
      int copied = CopyBuffer(indicator_handle, buffer_num, start_time, stop_time, buffer);
      
      sendDoubleArrayResponse(ExpertHandle, buffer, copied);
   }
   break;
   
   case 41: //CopyRates
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      int start_pos;
      int count;
      MqlRates rates[];
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, start_pos);            
      getIntValue(ExpertHandle, 3, count);         
      
      int copied = CopyRates(symbolValue, timeframe, start_pos, count, rates);       

      sendMqlRatesArrayResponse(ExpertHandle, rates, copied);
   }
   break; 
   
   case 1041: //CopyRates1
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      int count;
      MqlRates rates[];
      int tmpDateValue;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, count);         
      
      int copied = CopyRates(symbolValue, timeframe, start_time, count, rates);       

      sendMqlRatesArrayResponse(ExpertHandle, rates, copied);
   }
   break; 
   
   case 1141: //CopyRates2
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      datetime stop_time;
      MqlRates rates[];
      int tmpDateValue;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, tmpDateValue);
      stop_time = (datetime)tmpDateValue;        
      
      int copied = CopyRates(symbolValue, timeframe, start_time, stop_time, rates);       

      sendMqlRatesArrayResponse(ExpertHandle, rates, copied);
   }
   break;
   
   case 42: //CopyTime
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      int start_pos;
      int count;
      datetime time_array[];
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, start_pos);            
      getIntValue(ExpertHandle, 3, count);       
      
      int copied = CopyTime(symbolValue, timeframe, start_pos, count, time_array);       

      sendLongArrayResponse(ExpertHandle, time_array, copied);
   }
   break;
   
   case 1042: //CopyTime1
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      int count;
      datetime time_array[];
      int tmpDateValue;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, count);    
      
      int copied = CopyTime(symbolValue, timeframe, start_time, count, time_array);       

      sendLongArrayResponse(ExpertHandle, time_array, copied);
   }
   break;
   
   case 1142: //CopyTime2
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      datetime stop_time;
      datetime time_array[];
      int tmpDateValue;     
       
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, tmpDateValue);
      stop_time = (datetime)tmpDateValue;    
      
      int copied = CopyTime(symbolValue, timeframe, start_time, stop_time, time_array);       

      sendLongArrayResponse(ExpertHandle, time_array, copied);
   }
   break;
   
   case 43: //CopyOpen
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      int start_pos;
      int count;
      double open_array[];
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, start_pos);            
      getIntValue(ExpertHandle, 3, count);       
      
      int copied = CopyOpen(symbolValue, timeframe, start_pos, count, open_array);       

      sendDoubleArrayResponse(ExpertHandle, open_array, copied);
   }
   break;   
   
   case 1043: //CopyOpen1
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      int count;
      double open_array[];
      int tmpDateValue;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, count); 
      
      int copied = CopyOpen(symbolValue, timeframe, start_time, count, open_array);       

      sendDoubleArrayResponse(ExpertHandle, open_array, copied);
   }
   break;     
   
   case 1143: //CopyOpen2
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      datetime stop_time;
      double open_array[];
      int tmpDateValue;  
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, tmpDateValue);
      stop_time = (datetime)tmpDateValue;  
      
      int copied = CopyOpen(symbolValue, timeframe, start_time, stop_time, open_array);       

      sendDoubleArrayResponse(ExpertHandle, open_array, copied);
   }
   break;   
   
   case 44: //CopyHigh
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      int start_pos;
      int count;
      double high_array[];
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, start_pos);            
      getIntValue(ExpertHandle, 3, count);   
      
      int copied = CopyHigh(symbolValue, timeframe, start_pos, count, high_array);       

      sendDoubleArrayResponse(ExpertHandle, high_array, copied);
   }
   break;  
   
   case 1044: //CopyHigh1
   {      
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      int count;
      double high_array[];
      int tmpDateValue;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, count);   
      
      int copied = CopyHigh(symbolValue, timeframe, start_time, count, high_array);       

      sendDoubleArrayResponse(ExpertHandle, high_array, copied);
   }
   break;    
   
   case 1144: //CopyHigh2
   {      
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      datetime stop_time;
      double high_array[];
      int tmpDateValue;  
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, tmpDateValue);
      stop_time = (datetime)tmpDateValue;    
      
      int copied = CopyHigh(symbolValue, timeframe, start_time, stop_time, high_array);       

      sendDoubleArrayResponse(ExpertHandle, high_array, copied);
   }
   break;   
   
   case 45: //CopyLow
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      int start_pos;
      int count;
      double low_array[];
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, start_pos);            
      getIntValue(ExpertHandle, 3, count);   
      
      int copied = CopyLow(symbolValue, timeframe, start_pos, count, low_array);       

      sendDoubleArrayResponse(ExpertHandle, low_array, copied);
   }
   break;     
   
   case 1045: //CopyLow1
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      int count;
      double low_array[];
      int tmpDateValue;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, count);   
      
      int copied = CopyLow(symbolValue, timeframe, start_time, count, low_array);       

      sendDoubleArrayResponse(ExpertHandle, low_array, copied);
   }
   break;    
   
   case 1145: //CopyLow2
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      datetime stop_time;
      double low_array[];
      int tmpDateValue;  
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, tmpDateValue);
      stop_time = (datetime)tmpDateValue;     
      
      int copied = CopyLow(symbolValue, timeframe, start_time, stop_time, low_array);       

      sendDoubleArrayResponse(ExpertHandle, low_array, copied);
   }
   break; 
   
   case 46: //CopyClose
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      int start_pos;
      int count;
      double close_array[];
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, start_pos);            
      getIntValue(ExpertHandle, 3, count);   
      
      int copied = CopyClose(symbolValue, timeframe, start_pos, count, close_array);       

      sendDoubleArrayResponse(ExpertHandle, close_array, copied);
   }
   break;  
   
   case 1046: //CopyClose1
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      int count;
      double close_array[];
      int tmpDateValue;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, count);    
      
      int copied = CopyClose(symbolValue, timeframe, start_time, count, close_array);       

      sendDoubleArrayResponse(ExpertHandle, close_array, copied);
   }
   break;   
   
   case 1146: //CopyClose2
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      datetime stop_time;
      double close_array[];
      int tmpDateValue;  
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, tmpDateValue);
      stop_time = (datetime)tmpDateValue;      
      
      int copied = CopyClose(symbolValue, timeframe, start_time, stop_time, close_array);       

      sendDoubleArrayResponse(ExpertHandle, close_array, copied);
   }
   break; 
   
  case 47: //CopyTickVolume
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      int start_pos;
      int count;
      long volume_array[];
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, start_pos);            
      getIntValue(ExpertHandle, 3, count);   
      
      int copied = CopyTickVolume(symbolValue, timeframe, start_pos, count, volume_array);       

      sendLongArrayResponse(ExpertHandle, volume_array, copied);
   }
   break; 
   
  case 1047: //CopyTickVolume1
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      int count;
      long volume_array[];
      int tmpDateValue;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, count);    
      
      int copied = CopyTickVolume(symbolValue, timeframe, start_time, count, volume_array);       

      sendLongArrayResponse(ExpertHandle, volume_array, copied);
   }
   break;       
   
  case 1147: //CopyTickVolume2
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      datetime stop_time;
      long volume_array[];
      int tmpDateValue;  
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, tmpDateValue);
      stop_time = (datetime)tmpDateValue;     
      
      int copied = CopyTickVolume(symbolValue, timeframe, start_time, stop_time, volume_array);       

      sendLongArrayResponse(ExpertHandle, volume_array, copied);
   }
   break;
   
  case 48: //CopyRealVolume
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      int start_pos;
      int count;
      long volume_array[];
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, start_pos);            
      getIntValue(ExpertHandle, 3, count);   
      
      int copied = CopyRealVolume(symbolValue, timeframe, start_pos, count, volume_array);       

      sendLongArrayResponse(ExpertHandle, volume_array, copied);
   }
   break; 
   
  case 1048: //CopyRealVolume1
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      int count;
      long volume_array[];
      int tmpDateValue;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, count);    
      
      int copied = CopyRealVolume(symbolValue, timeframe, start_time, count, volume_array);       

      sendLongArrayResponse(ExpertHandle, volume_array, copied);
   }
   break;   
   
  case 1148: //CopyRealVolume2
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      datetime stop_time;
      long volume_array[];
      int tmpDateValue;  
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, tmpDateValue);
      stop_time = (datetime)tmpDateValue;     
      
      int copied = CopyRealVolume(symbolValue, timeframe, start_time, stop_time, volume_array);       

      sendLongArrayResponse(ExpertHandle, volume_array, copied);
   }
   break;          
   
  case 49: //CopySpread
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      int start_pos;
      int count;
      int spread_array[];
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, start_pos);            
      getIntValue(ExpertHandle, 3, count);   
      
      int copied = CopySpread(symbolValue, timeframe, start_pos, count, spread_array);       

      sendIntArrayResponse(ExpertHandle, spread_array, copied);
   }
   break; 
   
  case 1049: //CopySpread1
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      int count;
      int spread_array[];
      int tmpDateValue;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, count);    
      
      int copied = CopySpread(symbolValue, timeframe, start_time, count, spread_array);       

      sendIntArrayResponse(ExpertHandle, spread_array, copied);
   }
   break;   
   
  case 1149: //CopySpread2
   {
      ENUM_TIMEFRAMES timeframe;
      int tmpEnumValue;
      datetime start_time;
      datetime stop_time;
      int spread_array[];
      int tmpDateValue;  
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      timeframe = (ENUM_TIMEFRAMES) tmpEnumValue;
      
      getIntValue(ExpertHandle, 2, tmpDateValue);
      start_time = (datetime)tmpDateValue;
      
      getIntValue(ExpertHandle, 3, tmpDateValue);
      stop_time = (datetime)tmpDateValue;     
      
      int copied = CopySpread(symbolValue, timeframe, start_time, stop_time, spread_array);       

      sendIntArrayResponse(ExpertHandle, spread_array, copied);
   }
   break;       

   case 50: //SymbolsTotal
   {
      bool selected;
      int retVal;
      
      getBooleanValue(ExpertHandle, 0, selected);      
      
      retVal = SymbolsTotal(selected);       

      sendIntResponse(ExpertHandle, retVal);
   }
   break;  
   
   case 51: //SymbolName
   {
      bool selected; 
      int pos;     
      
      getIntValue(ExpertHandle, 0, pos);  
      getBooleanValue(ExpertHandle, 1, selected);  
      
      symbolValue = SymbolName(pos, selected);       

      sendStringResponse(ExpertHandle, symbolValue);
   }
   break;     
   
   case 52: //SymbolSelect
   {
      bool select;
      bool retVal;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      getBooleanValue(ExpertHandle, 1, select);     
      
      retVal = SymbolSelect(symbolValue, select);       

      sendBooleanResponse(ExpertHandle, retVal);
   }
   break;  
   
   case 53: //SymbolIsSynchronized
   {
      bool retVal;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      retVal = SymbolIsSynchronized(symbolValue);       

      sendBooleanResponse(ExpertHandle, retVal);
   }
   break;   
   
   case 54: //SymbolInfoDouble
   {
      ENUM_SYMBOL_INFO_DOUBLE prop_id;
      int tmpEnumValue;
      double retVal;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      prop_id = (ENUM_SYMBOL_INFO_DOUBLE) tmpEnumValue;
      
      retVal = SymbolInfoDouble(symbolValue, prop_id);       

      sendDoubleResponse(ExpertHandle, retVal);
   }
   break;
   
   case 55: //SymbolInfoInteger
   {
      ENUM_SYMBOL_INFO_INTEGER prop_id;
      int tmpEnumValue;
      long retVal;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      prop_id = (ENUM_SYMBOL_INFO_INTEGER) tmpEnumValue;
      
      retVal = SymbolInfoInteger(symbolValue, prop_id);       

      sendLongResponse(ExpertHandle, retVal);
   }
   break;
   
   case 56: //SymbolInfoString
   {
      ENUM_SYMBOL_INFO_STRING prop_id;
      int tmpEnumValue;
      string retVal;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      prop_id = (ENUM_SYMBOL_INFO_STRING) tmpEnumValue;
      
      retVal = SymbolInfoString(symbolValue, prop_id);       

      sendStringResponse(ExpertHandle, retVal);
   }
   break; 
   
   case 57: //SymbolInfoTick
   {
      MqlTick tick={0}; 
      bool retVal;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      retVal = SymbolInfoTick(symbolValue, tick);       
      
      if (retVal == true)
      {
         sendMqlTickResponse(ExpertHandle, tick);
      }
      else
      {
         sendVoidResponse(ExpertHandle);
      }
   }
   break;     

   case 58: //SymbolInfoSessionQuote
   {
      ENUM_DAY_OF_WEEK day_of_week;
      uint session_index;
      
      datetime from;
      datetime to;
      bool retVal;
      
      int tmpEnumValue;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      day_of_week = (ENUM_DAY_OF_WEEK) tmpEnumValue;
      
      getUIntValue(ExpertHandle, 2, session_index);
      
      retVal = SymbolInfoSessionQuote(symbolValue, day_of_week, session_index, from, to);       

      string res = ResultToString(retVal, from, to);      
      sendStringResponse(ExpertHandle, res);      
   }
   break;  
   
   case 59: //SymbolInfoSessionTrade
   {
      ENUM_DAY_OF_WEEK day_of_week;
      uint session_index;
      
      datetime from;
      datetime to;
      bool retVal;
      
      int tmpEnumValue;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      getIntValue(ExpertHandle, 1, tmpEnumValue);      
      day_of_week = (ENUM_DAY_OF_WEEK) tmpEnumValue;
      
      getUIntValue(ExpertHandle, 2, session_index);
      
      retVal = SymbolInfoSessionTrade(symbolValue, day_of_week, session_index, from, to);       

      string res = ResultToString(retVal, from, to);      
      sendStringResponse(ExpertHandle, res);      
   }
   break; 
   
   case 60: //MarketBookAdd
   {
      bool retVal;
            
      getStringValue(ExpertHandle, 0, symbolValue);
      
      retVal = MarketBookAdd(symbolValue);       

      sendBooleanResponse(ExpertHandle, retVal);      
   }
   break; 
   
   case 61: //MarketBookRelease
   {
      bool retVal;
            
      getStringValue(ExpertHandle, 0, symbolValue);
      
      retVal = MarketBookRelease(symbolValue);       

      sendBooleanResponse(ExpertHandle, retVal);      
   }
   break; 
   
   case 62: //MarketBookGet
   {
      MqlBookInfo book[];
      bool retVal;
      
      getStringValue(ExpertHandle, 0, symbolValue);
      
      retVal = MarketBookGet(symbolValue, book); 
      
      if(retVal)
      {
         int size = ArraySize(book);
         sendMqlBookInfoArrayResponse(ExpertHandle, book, size);
      }
      else
      {
         sendVoidResponse(ExpertHandle);
      }
   }
   break;

   default:
      Print("Unknown command type = ", commandType);
      sendVoidResponse(ExpertHandle);
      break;
   } 
   
   return (commandType);
}

void PrintParamError(string paramName)
{
   Print("[ERROR] parameter: ", paramName);
}

void PrintResponseError(string commandName)
{
   Print("[ERROR] response: ", commandName);
}

void ReadMqlTradeRequestFromCommand(MqlTradeRequest& request)
{
      int tmpEnumValue;
      ulong m_magicValue = 0;
                  
      getIntValue(ExpertHandle, 0, tmpEnumValue);
      request.action = (ENUM_TRADE_REQUEST_ACTIONS)tmpEnumValue;      
      
      getULongValue(ExpertHandle, 1, m_magicValue);     
      request.magic = m_magicValue;
      getULongValue(ExpertHandle, 2, request.order);     
      getStringValue(ExpertHandle, 3, symbolValue);
      request.symbol = symbolValue;
      getDoubleValue(ExpertHandle, 4, request.volume);
      getDoubleValue(ExpertHandle, 5, request.price);
      getDoubleValue(ExpertHandle, 6, request.stoplimit);
      getDoubleValue(ExpertHandle, 7, request.sl);
      getDoubleValue(ExpertHandle, 8, request.tp);
      getULongValue(ExpertHandle, 9, request.deviation);
      getIntValue(ExpertHandle, 10, tmpEnumValue);      
      request.type = (ENUM_ORDER_TYPE)tmpEnumValue;      
      getIntValue(ExpertHandle, 11, tmpEnumValue);
      request.type_filling = (ENUM_ORDER_TYPE_FILLING)tmpEnumValue;
      getIntValue(ExpertHandle, 12, tmpEnumValue);
      request.type_time = (ENUM_ORDER_TYPE_TIME)tmpEnumValue;
      getIntValue(ExpertHandle, 13, tmpEnumValue);
      request.expiration = (datetime)tmpEnumValue;  
      getStringValue(ExpertHandle, 14, commentValue);         
      request.comment = commentValue;
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