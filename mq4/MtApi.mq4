#property copyright "Vyacheslav Demidyuk"
#property link      "http://mtapi4.net/"
#property version   "2.00"
#property description "MtApi connection expert"

#include <WinUser32.mqh>
#include <stdlib.mqh>
#include <json.mqh>
#include <mql4-auth.mqh>

#import "MTConnector.dll"
   bool initExpert(int expertHandle, int port, string& err);
   bool deinitExpert(int expertHandle, string& err);
 
   bool sendEvent(int expertHandle, int eventType, string payload, string& err);
   bool sendResponse(int expertHandle, string response, string& err);

   bool getCommandType(int expertHandle, int& res, string& err);
   bool getPayload(int expertHandle, string& res, string& err);
#import

//#define __DEBUG_LOG__

enum LockTickType
{
   NO_LOCK,
   LOCK_EVERY_TICK,
   LOCK_EVERY_CANDLE
};

extern int Port = 8222;
extern LockTickType BacktestingLockTicks = NO_LOCK;

int ExpertHandle;

string _error;
string _response_error;
bool isCrashed = FALSE;

bool IsRemoteReadyForTesting = false;

int _lastBarOpenTime;
bool _is_ticks_locked = false;

class MtOrder
{
public:
   int getTicket() { return _ticket; }
   string getSymbol() { return _symbol; }
   int getOperation() { return _operation; }
   double getOpenPrice() { return _openPrice; }
   double getClosePrice() { return _closePrice; }
   double getLots() { return _lots; }
   double getProfit() { return _profit; }
   string getComment() { return _comment; }
   double getCommission() { return _commission; }
   int getMagicNumber() { return _magicNumber; }
   datetime getOpenTime() { return _openTime; }
   datetime getCloseTime() { return _closeTime; }
   double getSwap() { return _swap; }
   datetime getExpiration() { return _expiration; }
   double getTakeProfit() { return _takeProfit; }
   double getStopLoss() { return _stopLoss; }
   
   static bool isLong(int operation)
   {
      if (operation == OP_BUY || operation == OP_BUYLIMIT || operation == OP_BUYSTOP)
         return true;
      return false;
   }
   
   JSONObject* CreateJson()
   {
      JSONObject *jo = new JSONObject();   
      jo.put("Ticket", new JSONNumber(_ticket));
      jo.put("Symbol", new JSONString(_symbol));
      jo.put("Operation", new JSONNumber(_operation));
      jo.put("OpenPrice", new JSONNumber(_openPrice));
      jo.put("ClosePrice", new JSONNumber(_closePrice));
      jo.put("Lots", new JSONNumber(_lots));
      jo.put("Profit", new JSONNumber(_profit));
      if (_comment != "")
      {
         jo.put("Comment", new JSONString(_comment));
      }
      jo.put("Commission", new JSONNumber(_commission));
      jo.put("MagicNumber", new JSONNumber(_magicNumber));
      jo.put("MtOpenTime", new JSONNumber(_openTime));
      jo.put("MtCloseTime", new JSONNumber(_closeTime));
      jo.put("Swap", new JSONNumber(_swap));
      jo.put("MtExpiration", new JSONNumber(_expiration));
      jo.put("TakeProfit", new JSONNumber(_takeProfit));
      jo.put("StopLoss", new JSONNumber(_stopLoss));
      return jo;
   }
         
   static MtOrder* LoadOrder(int index, int select, int pool)
   {
      MtOrder* order = NULL;
      if (OrderSelect(index, select, pool))
      {
         order = new MtOrder();
         order._ticket = OrderTicket();
         order._symbol = OrderSymbol();
         order._operation = OrderType();
         order._openPrice = OrderOpenPrice();
         order._closePrice = OrderClosePrice();
         order._lots = OrderLots();
         order._profit = OrderProfit();
         order._commission = OrderCommission();
         order._magicNumber = OrderMagicNumber();
         order._openTime = OrderOpenTime();
         order._closeTime = OrderCloseTime();
         order._swap = OrderSwap();
         order._expiration = OrderExpiration();
         order._takeProfit = OrderTakeProfit();
         order._stopLoss = OrderStopLoss();
         
         if (!IsTesting())
         {
            order._comment = OrderComment();
         }
      }
      return order;
   }         
         
private:
   int _ticket;
   string _symbol;
   int _operation;
   double _openPrice;
   double _closePrice;
   double _lots;
   double _profit;
   string _comment;
   double _commission;
   int _magicNumber;
   datetime _openTime;
   datetime _closeTime;
   double _swap;
   datetime _expiration;
   double _takeProfit;
   double _stopLoss;
};

JSONObject* MqlTickToJson(const MqlTick& tick)
{
    JSONObject *jo = new JSONObject();
    jo.put("Time", new JSONNumber((long)tick.time));
    jo.put("Bid", new JSONNumber(tick.bid));
    jo.put("Ask", new JSONNumber(tick.ask));
    jo.put("Last", new JSONNumber(tick.last));
    jo.put("Volume", new JSONNumber(tick.volume));
    return jo;
}

class MtObject
{
public:
   virtual JSONObject* CreateJson() = 0;
};

class MtQuote : public MtObject
{
public:
   MtQuote(string symbol, const MqlTick& tick)
   {
      _symbol = symbol;
      _tick = tick;
   }
   
   virtual JSONObject* CreateJson()
   {
      JSONObject *jo = new JSONObject();
      jo.put("Tick", MqlTickToJson(_tick));
      jo.put("Instrument", new JSONString(_symbol));
      jo.put("ExpertHandle", new JSONNumber(ExpertHandle));
      return jo;
   }
   
private:
   string   _symbol;
   MqlTick  _tick;
};

class MtTimeBar: public MtObject
{
public:
   MtTimeBar(string symbol, datetime openTime, datetime closeTime, double open, double close, double high, double low)
   {
      _symbol = symbol;
      _openTime = openTime;
      _closeTime = closeTime;
      _open = open;
      _close = close;
      _high = high;
      _low = low;
   }
   
   virtual JSONObject* CreateJson()
   {
      JSONObject *jo = new JSONObject();   
      jo.put("Symbol", new JSONString(_symbol));
      jo.put("MtOpenTime", new JSONNumber(_openTime));
      jo.put("MtCloseTime", new JSONNumber(_closeTime));
      jo.put("Open", new JSONNumber(_open));
      jo.put("Close", new JSONNumber(_close));
      jo.put("High", new JSONNumber(_high));
      jo.put("Low", new JSONNumber(_low));
      return jo;
   }

private: 
   string _symbol;
   datetime _openTime;
   datetime _closeTime;
   double _open;
   double _close;
   double _high;
   double _low;
};

class MtChartEvent: public MtObject
{
public:
   MtChartEvent(long chartId, int eventId, long lparam, double dparam, string sparam)
   {
      _chartId = chartId;
      _eventId = eventId;
      _lparam = lparam;
      _dparam = dparam;
      _sparam = sparam;
   }
   
   virtual JSONObject* CreateJson()
   {
      JSONObject *jo = new JSONObject();   
      jo.put("ChartId", new JSONNumber(_chartId));
      jo.put("EventId", new JSONNumber(_eventId));
      jo.put("Lparam", new JSONNumber(_lparam));
      jo.put("Dparam", new JSONNumber(_dparam));
      jo.put("Sparam", new JSONString(_sparam));
      return jo;
   }
   
private:
   long _chartId;
   int _eventId;
   long _lparam;
   double _dparam;
   string _sparam;
};

class MtLockTickEvent: public MtObject
{
public:
   MtLockTickEvent(string symbol)
   {
      _symbol = symbol;
   }
   
   virtual JSONObject* CreateJson()
   {
      JSONObject *jo = new JSONObject();
      jo.put("Instrument", new JSONString(_symbol));
      return jo;
   }
   
private:
   string _symbol;
};

class MtSession
{
   public:  
      static MtSession* LoadSession(string symbol, int day, int index, int type)
      {
         MtSession *session = new MtSession(symbol, day, index, type);
         
         if (type == QUOTE)
         {
            session._hasData = SymbolInfoSessionQuote(symbol, day, index, session._from, session._to);
         }
         else
         {
            session._hasData = SymbolInfoSessionTrade(symbol, day, index, session._from, session._to);
         }
         
         return session;          
      }
   
      JSONObject* CreateJson()
      {
         JSONObject *jo = new JSONObject();
         jo.put("Symbol", new JSONString(_symbol));
         jo.put("DayOfWeek", new JSONNumber(_dayOfWeek));
         jo.put("Index", new JSONNumber(_index));
         jo.put("MtFromTime", new JSONNumber(_from));
         jo.put("MtToTime", new JSONNumber(_to));
         jo.put("HasData", new JSONBool(_hasData));
         jo.put("Type", new JSONNumber(_type));
         return jo;
      }

   private:
      string _symbol;
      int _dayOfWeek;
      int _index;
      datetime _from;
      datetime _to;
      bool _hasData;
      int _type;
      
      MtSession(string symbol, int day, int index, int type)
      {
         _symbol = symbol;
         _dayOfWeek = day;
         _index = index;
         _type = type;
      }
};

enum MtSessionType
{
   QUOTE,
   TRADE
};

enum MtEventTypes
{
   LAST_TIME_BAR_EVENT     = 1,
   MT_CHART_EVENT          = 2,
   ON_LOCK_TICKS_EVENT     = 3,
   ON_TICK_EVENT           = 4
};

int preinit()
{
   StringInit(_error,1000,0);
   StringInit(_response_error,1000,0);

   return (0);
}

int OnInit()
{
   preinit();
     
   if (IsDllsAllowed() == FALSE) 
   {
      MessageBox("Dlls not allowed.", "MtApi", MB_OK);
      isCrashed = TRUE;
      return (1);
   }
   if (IsLibrariesAllowed() == FALSE) 
   {
      MessageBox("Libraries not allowed.", "MtApi", MB_OK);
      isCrashed = TRUE;
      return (1);
   }

   if (IsTradeAllowed() == FALSE) 
   {
      Print("INFO: trading is not allowed.");
   }  

   ExpertHandle = WindowHandle(Symbol(), Period());
   
   if (!initExpert(ExpertHandle, Port, _error))
   {
       MessageBox(_error, "MtApi", MB_OK);
       isCrashed = TRUE;
       return(1);
   }
    
   if (ExecuteCommand() == 1)
   {   
      isCrashed = TRUE;
      return (1);
   }
   
   //--- Backtesting mode
   if (IsTesting())
   {      
      Print("Waiting on remote client...");
      //wait for command (BacktestingReady) from remote side to be ready for work
      while(!IsRemoteReadyForTesting)
      {
         ExecuteCommand();
         
         //This section uses a while loop to simulate Sleep() during Backtest.
         unsigned int viSleepUntilTick = GetTickCount() + 100; //100 milliseconds
         while(GetTickCount() < viSleepUntilTick) 
         {
            //Do absolutely nothing. Just loop until the desired tick is reached.
         }
      }
   }
   //--- 
   
   _lastBarOpenTime = Time[0];
   
   return (INIT_SUCCEEDED);
}

void OnDeinit(const int reason)
{
   if (isCrashed == 0) 
   {
      if (!deinitExpert(ExpertHandle, _error)) 
      {
         MessageBox(_error, "MtApi", MB_OK);
         isCrashed = TRUE;
      }
   }
}

void updateQuote()
{
   MqlTick last_tick;
   if(SymbolInfoTick(Symbol(),last_tick))
   {
      MtQuote quote(Symbol(), last_tick);
      SendMtEvent(ON_TICK_EVENT, quote);
   }
}

int _tick_count = 0;

void OnTick()
{   
   bool lastbar_time_changed = false;
   if (_lastBarOpenTime != Time[0])
   {
      double open = Open[1];
      double close = Close[1];
      double high = High[1];
      double low = Low[1];
      
      MtTimeBar timeBar(Symbol(), _lastBarOpenTime, Time[0], open, close, high, low);
      SendMtEvent(LAST_TIME_BAR_EVENT, timeBar);
      
      _lastBarOpenTime = Time[0];
      lastbar_time_changed = true;
   }
   
   updateQuote();

   if (IsTesting())
   {
      if (BacktestingLockTicks == LOCK_EVERY_TICK ||
         (BacktestingLockTicks == LOCK_EVERY_CANDLE  && lastbar_time_changed))
      {
         _is_ticks_locked = true;
         
         MtLockTickEvent lock_tick_event(Symbol());
         SendMtEvent(ON_LOCK_TICKS_EVENT, lock_tick_event);
      }
      
      while(true)
      {
         if (IsStopped())
            break;
      
         int executedCommand = ExecuteCommand();
                       
         if (_is_ticks_locked)
            continue;
               
         if (executedCommand == 0) 
            break;
      }
   }
}

void OnTimer()
{
   while(true)
   {
      int executedCommand = ExecuteCommand();
      if (executedCommand == 0) return;
   }
}

void OnChartEvent(const int id,         // Event ID
                  const long& lparam,   // Parameter of type long event
                  const double& dparam, // Parameter of type double event
                  const string& sparam  // Parameter of type string events
                  )
{
   MtChartEvent charEvent(ChartID(), id, lparam, dparam, sparam);
   SendMtEvent(MT_CHART_EVENT, charEvent);
}

void SendMtEvent(MtEventTypes eventType, MtObject& mtObj)
{
   JSONObject* json = mtObj.CreateJson();
   if (sendEvent(ExpertHandle, (int)eventType, json.toString(), _error))
   {
#ifdef __DEBUG_LOG__
      PrintFormat("%s: event = %s", __FUNCTION__, EnumToString(eventType));
      PrintFormat("%s: payload = %s", __FUNCTION__, json.toString());
#endif
   }
   else
   {
      PrintFormat("[ERROR] SendMtEvent: %s", _error);
   }
   
   delete json;
}

//------ helper macros to get and send values ------------------
template <typename T_>
class auto_ptr
{
public:
   T_ *p;
   void reset() { if (this.p) delete this.p; this.p = NULL;}
   
   auto_ptr(void *ptr = NULL): p(ptr) {}
   ~auto_ptr()  { this.reset(); }
   
   void swap(auto_ptr<T_> &other)
   {
      T_ *buf = this.p;
      this.p = other.p;
      other.p = buf;
   }
};

JSONObject* GetJsonPayload()
{
   string payload;
   StringInit(payload, 5000, 0);
   
   if (!getPayload(ExpertHandle, payload, _error))
   {
      PrintFormat("%s [ERROR]: %s", __FUNCTION__, _error);
      return NULL;
   }

   JSONParser payload_parser;
   JSONValue *payload_json = payload_parser.parse(payload);
   
   if (payload_json == NULL) 
   {   
      PrintFormat("%s [ERROR]: %d - %s", __FUNCTION__, (string)payload_parser.getErrorCode(), payload_parser.getErrorMessage());
      return NULL;
   }
   
   return payload_json.isObject() ? payload_json : NULL;
}

#define PRINT_MSG_AND_RETURN_VALUE(msg,value) PrintFormat("%s: %s",__FUNCTION__,msg);return value
#define GET_JSON_PAYLOAD(json) auto_ptr<JSONObject> json(GetJsonPayload()); if (json.p == NULL) { return CreateErrorResponse(-1, "Failed to get payload"); }
#define CHECK_JSON_VALUE(json, name_value) if (json.p.getValue(name_value) == NULL) { PRINT_MSG_AND_RETURN_VALUE(StringFormat("failed to get %s from JSON!", name_value), CreateErrorResponse(-1, (StringFormat("Undefinded mandatory parameter %s", name_value)))); }
#define GET_INT_JSON_VALUE(json, name_value, return_value) CHECK_JSON_VALUE(json, name_value); int return_value = json.p.getInt(name_value)
#define GET_UINT_JSON_VALUE(json, name_value, return_value) CHECK_JSON_VALUE(json, name_value); uint return_value = json.p.getInt(name_value)
#define GET_DOUBLE_JSON_VALUE(json, name_value, return_value) CHECK_JSON_VALUE(json, name_value); double return_value = json.p.getDouble(name_value)
#define GET_LONG_JSON_VALUE(json, name_value, return_value) CHECK_JSON_VALUE(json, name_value); long return_value = json.p.getLong(name_value)
#define GET_ULONG_JSON_VALUE(json, name_value, return_value) CHECK_JSON_VALUE(json, name_value); ulong return_value = json.p.getLong(name_value)
#define GET_STRING_JSON_VALUE(json, name_value, return_value) CHECK_JSON_VALUE(json, name_value); string return_value = json.p.getString(name_value)
#define GET_BOOL_JSON_VALUE(json, name_value, return_value) CHECK_JSON_VALUE(json, name_value); bool return_value = json.p.getBool(name_value)
//--------------------------------------------------------------


int ExecuteCommand()
{
   int command_type = 0;

   if (!getCommandType(ExpertHandle, command_type, _error))
   {
      Print("[ERROR] ExecuteCommand: Failed to get command type! ", _error);
      return (0);
   }

   if (command_type == 0)
      return 0;

#ifdef __DEBUG_LOG__
      Print("ExecuteCommand: commnad type = ", command_type);
#endif

   string response;
   
   switch (command_type)
   {
   case 0:
      //NoCommand
   break;
   case 151: //OrderCloseAll
      response = Execute_OrderCloseAll();
   break;
   case 1: // OrderSend
      response = Execute_OrderSend();
   break;
   case 2: // OrderClose
      response = Execute_OrderClose();
   break;
   case 3: // OrderCloseBy
      response = Execute_OrderCloseBy();
   break;
   case 4: // OrderClosePrice
      response = Execute_OrderClosePrice();           
   break;
   case 1004: // OrderClosePriceByTicket
      response = Execute_OrderClosePriceByTicket();
   break;
   case 5: //OrderCloseTime
      response = Execute_OrderCloseTime();
   break;
   case 6: //OrderComment
      response = Execute_OrderComment();
   break;
   case 7: //OrderCommission
      response = Execute_OrderCommission();
   break;      
   case 8: //OrderDelete
      response = Execute_OrderDelete();
   break;
   case 9: //OrderExpiration
      response = Execute_OrderExpiration();
   break;
   case 10: //OrderLots
      response = Execute_OrderLots();
   break;
   case 11: //OrderMagicNumber
      response = Execute_OrderMagicNumber();
   break;
   case 12: //OrderModify
      response = Execute_OrderModify();
   break;
   case 13: //OrderOpenPrice
      response = Execute_OrderOpenPrice();
   break;
   case 1013: // OrderOpenPriceByTicket
      response = Execute_OrderOpenPriceByTicket();
   break;
   case 14: //OrderOpenTime
      response = Execute_OrderOpenTime();
   break;
   case 15: //OrderPrint
      response = Execute_OrderPrint();
   break;
   case 16: //OrderProfit
      response = Execute_OrderProfit();
   break;
   case 17: //OrderSelect
      response = Execute_OrderSelect();
   break;
   case 18: //OrdersHistoryTotal
      response = Execute_OrdersHistoryTotal();
   break;
   case 19: //OrderStopLoss
      response = Execute_OrderStopLoss();
   break;
   case 20: //OrdersTotal
      response = Execute_OrdersTotal();
   break;
   case 21: //OrderSwap
      response = Execute_OrderSwap();
   break;
   case 22: //OrderSymbol
      response = Execute_OrderSymbol();
   break;
   case 23: //OrderTakeProfit
      response = Execute_OrderTakeProfit();
   break;
   case 24: //OrderTicket
      response = Execute_OrderTicket();
   break;
   case 25: //OrderType
      response = Execute_OrderType();
   break;
   case 26: //GetLastError
      response = Execute_GetLastError();
   break;
   case 27: //IsConnected
      response = Execute_IsConnected();
   break;
   case 28: //IsDemo
      response = Execute_IsDemo();
   break;
   case 29: //IsDllsAllowed
      response = Execute_IsDllsAllowed();
   break;
   case 30: //IsExpertEnabled
      response = Execute_IsExpertEnabled();
   break;
   case 31: //IsLibrariesAllowed
      response = Execute_IsLibrariesAllowed();
   break;
   case 32: //IsOptimization
      response = Execute_IsOptimization();
   break;
   case 33: //IsStopped
      response = Execute_IsStopped();
   break;
   case 34: //IsTesting
      response = Execute_IsTesting();
   break;
   case 35: //IsTradeAllowed
      response = Execute_IsTradeAllowed();
   break;
   case 36: //IsTradeContextBusy
      response = Execute_IsTradeContextBusy();
   break;
   case 37: //IsVisualMode
      response = Execute_IsVisualMode();
   break;
   case 38: //UninitializeReason
      response = Execute_UninitializeReason();
   break;
   case 39: //ErrorDescription
      response = Execute_ErrorDescription();
   break;
   case 40: //AccountBalance
      response = Execute_AccountBalance();
   break;
   case 41: //AccountCredit
      response = Execute_AccountCredit();      
   break;
   case 42: //AccountCompany
      response = Execute_AccountCompany();
   break;
   case 43: //AccountCurrency
      response = Execute_AccountCurrency();
   break;
   case 44: //AccountEquity
      response = Execute_AccountEquity();
   break;
   case 45: //AccountFreeMargin
      response = Execute_AccountFreeMargin();
   break;
   case 46: //AccountFreeMarginCheck
      response = Execute_AccountFreeMarginCheck();
   break;
   case 47: //AccountFreeMarginMode
      response = Execute_AccountFreeMarginMode();
   break;
   case 48: //AccountLeverage
      response = Execute_AccountLeverage();
   break;
   case 49: //AccountMargin
      response = Execute_AccountMargin();
   break;
   case 50: //AccountName
      response = Execute_AccountName();
   break;
   case 51: //AccountNumber
      response = Execute_AccountNumber();
   break;
   case 52: //AccountProfit
      response = Execute_AccountProfit();
   break;
   case 53: //AccountServer
      response = Execute_AccountServer();
   break;
   case 54: //AccountStopoutLevel
      response = Execute_AccountStopoutLevel();
   break;
   case 55: //AccountStopoutMode
      response = Execute_AccountStopoutMode();
   break;
   case 56: //Alert
      response = Execute_Alert();
   break;
   case 57: //Comment
      response = Execute_Comment();
   break;
   case 58: //GetTickCount
      response = Execute_GetTickCount();
   break;
   case 59: //MarketInfo   
      response = Execute_MarketInfo();
   break;
   case 60: //MessageBox
      response = Execute_MessageBox(false);
   break;
   case 61: //MessageBoxA
      response = Execute_MessageBox(true);
   break;
   case 62: //PlaySound
      response = Execute_PlaySound();
   break;
   case 63: //Print
      response = Execute_Print();
   break;
   case 64: //SendFTP
      response = Execute_SendFTP(false);
   break;
   case 65: //SendFTPA
      response = Execute_SendFTP(true);
   break;
   case 66: //SendMail
      response = Execute_SendMail();
   break;
   case 67: //Sleep
      response = Execute_Sleep();
   break;
   case 68: //TerminalCompany
      response = Execute_TerminalCompany();
   break;
   case 69: //TerminalName
      response = Execute_TerminalName();
   break;
   case 70: //TerminalPath
      response = Execute_TerminalPath();
   break;
   case 71: //Day
      response = Execute_Day();
   break;
   case 72: //DayOfWeek
      response = Execute_DayOfWeek();
   break;
   case 73: //DayOfYear
      response = Execute_DayOfYear();
   break;
   case 74: //Hour
      response = Execute_Hour();
   break;
   case 75: //Minute
      response = Execute_Minute();
   break;
   case 76: //Month
      response = Execute_Month();
   break;
   case 77: //Seconds
      response = Execute_Seconds();
   break;
   case 78: //TimeCurrent
      response = Execute_TimeCurrent();
   break;
   case 79: //TimeDay
      response = Execute_TimeDay();
   break;
   case 80: //TimeDayOfWeek
      response = Execute_TimeDayOfWeek();
   break;
   case 81: //TimeDayOfYear
      response = Execute_TimeDayOfYear();
   break;
   case 82: //TimeHour
      response = Execute_TimeHour();
   break;
   case 83: //TimeLocal
      response = Execute_TimeLocal();
   break;
   case 84: //TimeMinute
      response = Execute_TimeMinute();
   break;
   case 85: //TimeMonth
      response = Execute_TimeMonth();
   break;
   case 86: //TimeSeconds
      response = Execute_TimeSeconds();
   break;
   case 87: //TimeYear
      response = Execute_TimeYear();
   break;
   case 88: //Year
      response = Execute_Year();
   break;
   case 89: //GlobalVariableCheck
      response = Execute_GlobalVariableCheck();
   break;
   case 90: //GlobalVariableDel
      response = Execute_GlobalVariableDel();
   break;
   case 91: //GlobalVariableGet
      response = Execute_GlobalVariableGet();
   break;
   case 92: //GlobalVariableName
      response = Execute_GlobalVariableName();
   break;
   case 93: //GlobalVariableSet
      response = Execute_GlobalVariableSet();
   break;
   case 94: //GlobalVariableSetOnCondition
      response = Execute_GlobalVariableSetOnCondition();
   break;
   case 95: //GlobalVariablesDeleteAll
      response = Execute_GlobalVariablesDeleteAll();
   break;
   case 96: //GlobalVariablesTotal
      response = Execute_GlobalVariablesTotal();
   break;
   case 97: //iAC
      response = Execute_iAC();
   break;
   case 98: //iAD
      response = Execute_iAD();
   break;
   case 99: //iAlligator
      response = Execute_iAlligator();
   break;
   case 100: //iADX
      response = Execute_iADX();
   break;
   case 101: //iATR
      response = Execute_iATR();
   break;
   case 102: //iAO
      response = Execute_iAO();
   break;
   case 103: //iBearsPower
      response = Execute_iBearsPower();
   break;
   case 104: //iBands
      response = Execute_iBands();
   break;
   case 105: //iBandsOnArray
      response = Execute_iBandsOnArray();
   break;
   case 106: //iBullsPower
      response = Execute_iBullsPower();
   break;
   case 107: //iCCI
      response = Execute_iCCI();
   break;
   case 108: //iCCIOnArray
      response = Execute_iCCIOnArray();
   break;
   case 109: //iCustom
      response = Execute_iCustom();
   break;
   case 110: //iDeMarker
      response = Execute_iDeMarker();
   break;
   case 111: //iEnvelopes
      response = Execute_iEnvelopes();
   break;
   case 112: //iEnvelopesOnArray
      response = Execute_iEnvelopesOnArray();
   break;
   case 113: //iForce
      response = Execute_iForce();
   break;
   case 114: //iFractals
      response = Execute_iFractals();
   break;
   case 115: //iGator
      response = Execute_iGator();
   break;
   case 116: //iIchimoku
      response = Execute_iIchimoku();
   break;
   case 117: //iBWMFI
      response = Execute_iBWMFI();
   break;
   case 118: //iMomentum
      response = Execute_iMomentum();
   break;
   case 119: //iMomentumOnArray
      response = Execute_iMomentumOnArray();
   break;
   case 120: //iMFI
      response = Execute_iMFI();
   break;
   case 121: //iMA
      response = Execute_iMA();
   break;
   case 122: //iMAOnArray
      response = Execute_iMAOnArray();
   break;
   case 123: //iOsMA
      response = Execute_iOsMA();   
   break;
   case 124: //iMACD
      response = Execute_iMACD();
   break;
   case 125: //iOBV
      response = Execute_iOBV();
   break;
   case 126: //iSAR
      response = Execute_iSAR();
   break;
   case 127: //iRSI
      response = Execute_iRSI();
   break;
   case 128: //iRSIOnArray
      response = Execute_iRSIOnArray();
   break;
   case 129: //iRVI
      response = Execute_iRVI();
   break;
   case 130: //iStdDev
      response = Execute_iStdDev();
   break;
   case 131: //iStdDevOnArray
      response = Execute_iStdDevOnArray();
   break;
   case 132: //iStochastic
      response = Execute_iStochastic();
   break;
   case 133: //iWPR
      response = Execute_iWPR();
   break;
   case 134: //iBars
      response = Execute_iBars();
   break;
   case 135: //iBarShift
      response = Execute_iBarShift();
   break;
   case 136: //iClose
      response = Execute_iClose();
   break;
   case 137: //iHigh
      response = Execute_iHigh();
   break;
   case 138: //iHighest
      response = Execute_iHighest();
   break;
   case 139: //iLow
      response = Execute_iLow();
   break;
   case 140: //iLowest
      response = Execute_iLowest();
   break;
   case 141: //iOpen
      response = Execute_iOpen();
   break;
   case 142: //iTime
      response = Execute_iTime();
   break;
   case 143: //iVolume
      response = Execute_iVolume();
   break;
   case 144: //iCloseArray
      response = Execute_iCloseArray();
   break;
   case 145: //iHighArray
      response = Execute_iHighArray();
   break;
   case 146: //iLowArray
      response = Execute_iLowArray();
   break;
   case 147: //iOpenArray
      response = Execute_iOpenArray();
   break;
   case 148: //iVolumeArray
      response = Execute_iVolumeArray();
   break;
   case 149: //iTimeArray
      response = Execute_iTimeArray();
   break;
   case 150: //RefreshRates
      response = Execute_RefreshRates();
   break;
   case 153: //TerminalInfoString
      response = Execute_TerminalInfoString();
   break;
   case 154: //SymbolInfoString
      response = Execute_SymbolInfoString();
   break;
   case 156: //BacktestingReady
      response = Execute_BacktestingReady();
   break;
   case 200: //SymbolsTotal
      response = Execute_SymbolsTotal();
   break;
   case 201: //SymbolName
      response = Execute_SymbolName();
   break;
   case 202: //SymbolSelect
      response = Execute_SymbolSelect();
   break;
   case 203: //SymbolInfoInteger
      response = Execute_SymbolInfoInteger();
   break;
   case 204: //TerminalInfoInteger
      response = Execute_TerminalInfoInteger();
   break;
   case 205: //TerminalInfoDouble
      response = Execute_TerminalInfoDouble();
   break;
   case 206: //ChartId
      response = Execute_CharId();
   break;
   case 207: //ChartRedraw
      response = Execute_ChartRedraw();
   break;
   case 208: //ObjectCreate
      response = Execute_ObjectCreate();
   break;
   case 209: //ObjectName
      response = Execute_ObjectName();
   break;
   case 210: //ObjectDelete
      response = Execute_ObjectDelete();
   break;
   case 211: //ObjectsDeleteAll
      response = Execute_ObjectsDeleteAll();
   break;
   case 212: //ObjectFind
      response = Execute_ObjectFind();
   break;
   case 213: //ObjectGetTimeByValue
      response = Execute_ObjectGetTimeByValue();
   break;
   case 214: //ObjectGetValueByTime
      response = Execute_ObjectGetValueByTime();
   break;
   case 215: //ObjectMove
      response = Execute_ObjectMove();
   break;
   case 216: //ObjectsTotal
      response = Execute_ObjectsTotal();
   break;
   case 217: //ObjectGetDouble
      response = Execute_ObjectGetDouble();
   break;
   case 218: //ObjectGetInteger
      response = Execute_ObjectGetInteger();
   break;
   case 219: //ObjectGetString
      response = Execute_ObjectGetString();
   break;
   case 220: //ObjectSetDouble
      response = Execute_ObjectSetDouble();
   break;
   case 221: //ObjectSetInteger
      response = Execute_ObjectSetInteger();
   break;
   case 222: //ObjectSetString
      response = Execute_ObjectSetString();
   break;
   case 223: //TextSetFont
      response = Execute_TextSetFont();
   break;
   case 224: //TextOut
//      Execute_TextOut();
   break;
   case 225: //TextGetSize
//      Execute_TextGetSize();
   break;
   case 226: //ObjectDescription
      response = Execute_ObjectDescription();
   break;
   case 227: //ObjectGet
//      Execute_ObjectGet();
   break;
   case 228: //ObjectGetFiboDescription
      response = Execute_ObjectGetFiboDescription();
   break;
   case 229: //ObjectGetShiftByValue
      response = Execute_ObjectGetShiftByValue();
   break;
   case 230: //ObjectGetValueByShift
      response = Execute_ObjectGetValueByShift();
   break;
   case 231: //ObjectSet
      response = Execute_ObjectSet();
   break;
   case 232: //ObjectSetFiboDescription
      response = Execute_ObjectSetFiboDescription();
   break;
   case 233: //ObjectSetText
      response = Execute_ObjectSetText();
   break;
   case 234: //ObjectType
      response = Execute_ObjectType();
   break;
   case 235: //UnlockTiks
      response = Execute_UnlockTicks();
   break;
   case 236: //ChartApplyTemplate
      response = Execute_ChartApplyTemplate();
   break;
   case 237: //ChartSaveTemplate
      response = Execute_ChartSaveTemplate();
   break;
   case 238: //ChartWindowFind
      response = Execute_ChartWindowFind();
   break;
   case 239: //ChartTimePriceToXY
      response = Execute_ChartTimePriceToXY();
   break;
   case 240: //ChartXYToTimePrice
      response = Execute_ChartXYToTimePrice();
   break;
   case 241: //ChartOpen
      response = Execute_ChartOpen();
   break;
   case 242: //ChartFirst
      response = Execute_ChartFirst();
   break;
   case 243: //ChartNext
      response = Execute_ChartNext();
   break;
   case 244: //ChartClose
      response = Execute_ChartClose();
   break;
   case 245: //ChartSymbol
      response = Execute_ChartSymbol();
   break;
   case 246: //ChartPeriod
      response = Execute_ChartPeriod();
   break;
   case 247: //ChartSetDouble
      response = Execute_ChartSetDouble();
   break;
   case 248: //ChartSetInteger
      response = Execute_ChartSetInteger();
   break;
   case 249: //ChartSetString
      response = Execute_ChartSetString();
   break;
   case 250: //ChartGetDouble
      response = Execute_ChartGetDouble();
   break;
   case 251: //ChartGetInteger
      response = Execute_ChartGetInteger();
   break;
   case 252: //ChartGetString
      response = Execute_ChartGetString();
   break;
   case 253: //ChartNavigate
      response = Execute_ChartNavigate();
   break;
   case 254: //ChartIndicatorDelete
      response = Execute_ChartIndicatorDelete();
   break;
   case 255: //ChartIndicatorName
      response = Execute_ChartIndicatorName();
   break;
   case 256: //ChartIndicatorsTotal
      response = Execute_ChartIndicatorsTotal();
   break;
   case 257: //ChartWindowOnDropped
      response = Execute_ChartWindowOnDropped();
   break;
   case 258: //ChartPriceOnDropped
      response = Execute_ChartPriceOnDropped();
   break;
   case 259: //ChartTimeOnDropped
      response = Execute_ChartTimeOnDropped();
   break;
   case 260: //ChartXOnDropped
      response = Execute_ChartXOnDropped();
   break;
   case 261: //ChartYOnDropped
      response = Execute_ChartYOnDropped();
   break;
   case 262: //ChartSetSymbolPeriod
      response = Execute_ChartSetSymbolPeriod();
   break;
   case 263: //ChartScreenShot
      response = Execute_ChartScreenShot();
   break;
   case 264: //WindowBarsPerChart
      response = Execute_WindowBarsPerChart();
   break;
   case 265: //WindowExpertName
      response = Execute_WindowExpertName();
   break;
   case 266: //WindowFind
      response = Execute_WindowFind();
   break;
   case 267: //WindowFirstVisibleBar
      response = Execute_WindowFirstVisibleBar();
   break;
   case 268: //WindowHandle
      response = Execute_WindowHandle();
   break;
   case 269: //WindowIsVisible
      response = Execute_WindowIsVisible();
   break;
   case 270: //WindowOnDropped
      response = Execute_WindowOnDropped();
   break;
   case 271: //WindowPriceMax
      response = Execute_WindowPriceMax();
   break;
   case 272: //WindowPriceMin
      response = Execute_WindowPriceMin();
   break;
   case 273: //WindowPriceOnDropped
      response = Execute_WindowPriceOnDropped();
   break;
   case 274: //WindowRedraw
      response = Execute_WindowRedraw();
   break;
   case 275: //WindowScreenShot
      response = Execute_WindowScreenShot();
   break;
   case 276: //WindowTimeOnDropped
      response = Execute_WindowTimeOnDropped();
   break;
   case 277: //WindowsTotal
      response = Execute_WindowsTotal();
   break;
   case 278: //WindowXOnDropped
      response = Execute_WindowXOnDropped();
   break;
   case 279: //WindowYOnDropped
      response = Execute_WindowYOnDropped();
   break;
   case 280: //ChangeAccount
      response = Execute_ChangeAccount();
   break;
   case 281: //TimeGMT
      response = Execute_TimeGMT();
   break;
   case 282: //GetOrder
      response = Execute_GetOrder();
   break;
   case 283: //GetOrders
      response = Execute_GetOrders();
   break;
   case 284: //CopyRates
      response = Execute_CopyRates();
   break;
   case 285: //Session
      response = Execute_Session();
   break;
   case 286: //SeriesInfoInteger
      response = Execute_SeriesInfoInteger();
   break;
   case 287: //SeriesInfoString
      //Execute_SeriesInfoString();
   break;
   case 288: //SymbolInfoTick
      response = Execute_SymbolInfoTick();
   break;
   case 289: //SymbolInfoDouble
      response = Execute_SymbolInfoDouble();
   break;
   case 290: //GetQuote
      response = Execute_GetQuote();
   break;
   case 291: //GetSymbols
      response = Execute_GetSymbols();
   break;
   
   default:
      Print("WARNING: Unknown command type = ", command_type);
      response = CreateErrorResponse(-1, "Unknown command type");
      break;
   }
   
   if (!sendResponse(ExpertHandle, response, _error))
      PrintFormat("[ERROR] response: %s", _error);

   return (command_type);
}

string Execute_OrderSend()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Cmd", cmd);
   GET_DOUBLE_JSON_VALUE(jo, "Volume", volume);

   double price;
   JSONValue *jvPrice = jo.p.getValue("Price");
   if (jvPrice != NULL)
   {
      price = jvPrice.getDouble();
   }
   else
   {
      int mode = MtOrder::isLong(cmd) ? MODE_ASK : MODE_BID;
      price = MarketInfo(symbol, mode);
   }

   JSONValue *jvSlippage = jo.p.getValue("Slippage");
   double slippage = (jvSlippage != NULL) ? jvSlippage.getDouble() : 1;

   JSONValue *jvStoploss = jo.p.getValue("Stoploss");
   double stoploss = (jvStoploss != NULL) ? jvStoploss.getDouble() : 0;

   JSONValue *jvTakeprofit = jo.p.getValue("Takeprofit");
   double takeprofit = (jvTakeprofit != NULL) ? jvTakeprofit.getDouble() : 0;

   JSONValue *jvComment = jo.p.getValue("Comment");
   string comment = (jvComment != NULL) ? jvComment.getString() : NULL;

   JSONValue *jvMagic = jo.p.getValue("Magic");
   int magic = (jvMagic != NULL) ? jvMagic.getInt() : 0;

   JSONValue *jvExpiration = jo.p.getValue("Expiration");
   int expiration = (jvExpiration != NULL) ? jvExpiration.getInt() : 0;

   JSONValue *jvArrowColor = jo.p.getValue("ArrowColor");
   int arrowcolor = (jvArrowColor != NULL) ? jvArrowColor.getInt() : clrNONE;

   int result = OrderSend(symbol, cmd, volume, price, slippage, stoploss, takeprofit, comment, magic, expiration, arrowcolor);
   if(result < 0)
   {
      return CreateErrorResponse(GetLastError(), "OrderSend failed");
   }
   
   return CreateSuccessResponse(result);
}

string Execute_OrderClose()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Ticket", ticket);

   double price;
   JSONValue *jvPrice = jo.p.getValue("Price");
   if (jvPrice != NULL)
   {
      price = jvPrice.getDouble();
   }
   else
   {
      if (OrderSelect(ticket, SELECT_BY_TICKET))
      {
         string symbol = OrderSymbol();
         int mode = MtOrder::isLong(OrderType()) ? MODE_BID : MODE_ASK;
         price = MarketInfo(symbol, mode);
      }
      else
      {
         return CreateErrorResponse(-1, "Failed select order to get current price");
      }
   }
   
   double lots;
   JSONValue *jvLots = jo.p.getValue("Lots");
   if (jvLots != NULL)
   {
      lots = jvLots.getDouble();
   }
   else
   {
      if (OrderSelect(ticket, SELECT_BY_TICKET))
      {
         lots = OrderLots();
      }
      else
      {
         return CreateErrorResponse(-1, "Failed select order to get lots of order");
      }
   }
   
   JSONValue *jvSlippage = jo.p.getValue("Slippage");
   double slippage = (jvSlippage != NULL) ? jvSlippage.getDouble() : 1;
   
   JSONValue *jvArrowColor = jo.p.getValue("ArrowColor");
   int arrowcolor = (jvArrowColor != NULL) ? jvArrowColor.getInt() : clrNONE;

   bool result = OrderClose(ticket, lots, price, slippage, arrowcolor);
   return CreateSuccessResponse(result);
}

string Execute_OrderCloseAll()
{
   bool result = OrderCloseAll();
   return CreateSuccessResponse(result);
}

string Execute_OrderCloseBy()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Ticket", ticket);
   GET_INT_JSON_VALUE(jo, "Opposite", opposite);
   
   JSONValue *jvArrowColor = jo.p.getValue("ColorValue");
   int color_value = (jvArrowColor != NULL) ? jvArrowColor.getInt() : clrNONE;
   
   bool result = OrderCloseBy(ticket, opposite, color_value);
   return CreateSuccessResponse(result);
}

string Execute_OrderClosePrice()
{
   double result = OrderClosePrice();
   return CreateSuccessResponse(result);
}

string Execute_OrderClosePriceByTicket()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Ticket", ticket);
   
   bool selected = OrderSelect(ticket, SELECT_BY_TICKET);
   if (!selected)
   {
      int last_error = GetLastError();
      PrintFormat("[ERROR] Command: %s, Failed to select order! ErrorCode = %d", "OrderClosePriceByTicket", last_error);
      return CreateErrorResponse(last_error, "Failed to select order");
   }
   
   double result = OrderClosePrice();
   return CreateSuccessResponse(result);
}

string Execute_OrderCloseTime()
{
   int result = OrderCloseTime();
   return CreateSuccessResponse(result);
}

string Execute_OrderComment()
{
   string result = OrderComment();
   return CreateSuccessResponse(result);
}

string Execute_OrderCommission()
{
   double result = OrderCommission();
   return CreateSuccessResponse(result);
}

string Execute_OrderDelete()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Ticket", ticket);
   
   JSONValue *jvArrowColor = jo.p.getValue("ArrowColor");
   int arrow_color = (jvArrowColor != NULL) ? jvArrowColor.getInt() : clrNONE;
   
   double result = OrderDelete(ticket, arrow_color);
   return CreateSuccessResponse(result);
}

string Execute_OrderExpiration()
{
   int result = OrderExpiration();
   return CreateSuccessResponse(result);
}

string Execute_OrderLots()
{
   double result = OrderLots();
   return CreateSuccessResponse(result);
}

string Execute_OrderMagicNumber()
{
   int result = OrderMagicNumber();
   return CreateSuccessResponse(result);
}

string Execute_OrderModify()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Ticket", ticket);
   GET_DOUBLE_JSON_VALUE(jo, "Price", price);
   GET_DOUBLE_JSON_VALUE(jo, "Stoploss", stoploss);
   GET_DOUBLE_JSON_VALUE(jo, "Takeprofit", takeprofit);
   GET_INT_JSON_VALUE(jo, "Expiration", expiration);
   
   JSONValue *jvArrowColor = jo.p.getValue("ArrowColor");
   int arrow_color = (jvArrowColor != NULL) ? jvArrowColor.getInt() : clrNONE;   

   bool result = OrderModify(ticket, price, stoploss, takeprofit, expiration, arrow_color);
   return CreateSuccessResponse(result);
}

string Execute_OrderOpenPrice()
{
   double result = OrderOpenPrice();
   return CreateSuccessResponse(result);
}

string Execute_OrderOpenPriceByTicket()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Ticket", ticket);
   
   bool selected = OrderSelect(ticket, SELECT_BY_TICKET);
   if (!selected)
   {
      int last_error = GetLastError();
      PrintFormat("[ERROR] Command: %s, Failed to select order! ErrorCode = %d", "OrderOpenPriceByTicket", last_error);
      return CreateErrorResponse(last_error, "Failed to select order");
   }
   
   double result = OrderOpenPrice();
   return CreateSuccessResponse(result);
}

string Execute_OrderOpenTime()
{
   int result = OrderOpenTime();
   return CreateSuccessResponse(result);
}

string Execute_OrderPrint()
{
   OrderPrint();
   return CreateSuccessResponse();
}

string Execute_OrderProfit()
{
   double result = OrderProfit();
   return CreateSuccessResponse(result);
}

string Execute_OrderSelect()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Index", index);
   GET_INT_JSON_VALUE(jo, "Select", select);
   GET_INT_JSON_VALUE(jo, "Pool", pool);
   
   bool result = OrderSelect(index, select, pool);
   return CreateSuccessResponse(result);
}

string Execute_OrdersHistoryTotal()
{
   int result = OrdersHistoryTotal();
   return CreateSuccessResponse(result);
}

string Execute_OrderStopLoss()
{
   double result = OrderStopLoss();
   return CreateSuccessResponse(result);
}

string Execute_OrdersTotal()
{
   int result = OrdersTotal();
   return CreateSuccessResponse(result);
}

string Execute_OrderSwap()
{
   double result = OrderSwap();
   return CreateSuccessResponse(result);
}

string Execute_OrderSymbol()
{
   string result = OrderSymbol();
   return CreateSuccessResponse(result);
}

string Execute_OrderTakeProfit()
{
   double result = OrderTakeProfit();
   return CreateSuccessResponse(result);
}

string Execute_OrderTicket()
{
   int result = OrderTicket();
   return CreateSuccessResponse(result);
}

string Execute_OrderType()
{
   int result = OrderType();
   return CreateSuccessResponse(result);
}

string Execute_GetLastError()
{
   int result = GetLastError();
   return CreateSuccessResponse(result);
}

string Execute_IsConnected()
{
   bool result = IsConnected();
   return CreateSuccessResponse(result);
}

string Execute_IsDemo()
{
   bool result = IsDemo();
   return CreateSuccessResponse(result);;
}

string Execute_IsDllsAllowed()
{
   bool result = IsDllsAllowed();
   return CreateSuccessResponse(result);
}

string Execute_IsExpertEnabled()
{
   bool result = IsExpertEnabled();
   return CreateSuccessResponse(result);
}

string Execute_IsLibrariesAllowed()
{
   bool result = IsLibrariesAllowed();
   return CreateSuccessResponse(result);
}

string Execute_IsOptimization()
{
   bool result = IsOptimization();
   return CreateSuccessResponse(result);
}

string Execute_IsStopped()
{
   bool result = IsStopped();
   return CreateSuccessResponse(result);
}

string Execute_IsTesting()
{
   bool result = IsTesting();
   return CreateSuccessResponse(result);
}

string Execute_IsTradeAllowed()
{
   bool result = IsTradeAllowed();
   return CreateSuccessResponse(result);
}

string Execute_IsTradeContextBusy()
{
   bool result = IsTradeContextBusy();
   return CreateSuccessResponse(result);
}

string Execute_IsVisualMode()
{
   bool result = IsVisualMode();
   return CreateSuccessResponse(result);
}

string Execute_UninitializeReason()
{
   int result = UninitializeReason();
   return CreateSuccessResponse(result);
}

string Execute_ErrorDescription()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "ErrorCode", error_code);
   
   string result = ErrorDescription(error_code);
   return CreateSuccessResponse(result);
}

string Execute_AccountBalance()
{
   double result = AccountBalance();
   return CreateSuccessResponse(result);
}

string Execute_AccountCredit()
{
   double result = AccountCredit();
   return CreateSuccessResponse(result);
}

string Execute_AccountCompany()
{
   string result = AccountCompany();
   return CreateSuccessResponse(result);
}

string Execute_AccountCurrency()
{
   string result = AccountCurrency();
   return CreateSuccessResponse(result);
}

string Execute_AccountEquity()
{
   double result = AccountEquity();
   return CreateSuccessResponse(result);
}

string Execute_AccountFreeMargin()
{
   double result = AccountFreeMargin();
   return CreateSuccessResponse(result);
}

string Execute_AccountFreeMarginCheck()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Cmd", cmd);
   GET_DOUBLE_JSON_VALUE(jo, "Volume", volume);
   
   double result = AccountFreeMarginCheck(symbol, cmd, volume);
   return CreateSuccessResponse(result);
}

string Execute_AccountFreeMarginMode()
{
   double result = AccountFreeMarginMode();
   return CreateSuccessResponse(result);
}

string Execute_AccountLeverage()
{
   int result = AccountLeverage();
   return CreateSuccessResponse(result);
}

string Execute_AccountMargin()
{
   double result = AccountMargin();
   return CreateSuccessResponse(result);
}

string Execute_AccountName()
{
   string result = AccountName();
   return CreateSuccessResponse(result);
}

string Execute_AccountNumber()
{
   int result = AccountNumber();
   return CreateSuccessResponse(result);
}

string Execute_AccountProfit()
{
   double result = AccountProfit();
   return CreateSuccessResponse(result);
}

string Execute_AccountServer()
{
   string result = AccountServer();
   return CreateSuccessResponse(result);
}

string Execute_AccountStopoutLevel()
{
   int result = AccountStopoutLevel();
   return CreateSuccessResponse(result);
}

string Execute_AccountStopoutMode()
{
   int result = AccountStopoutMode();
   return CreateSuccessResponse(result);
}

string Execute_Alert()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Msg", msg);

   Alert(msg);
   return CreateSuccessResponse();
}

string Execute_Comment()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Msg", msg);

   Comment(msg);
   return CreateSuccessResponse();
}

string Execute_GetTickCount()
{
   int result = GetTickCount();
   return CreateSuccessResponse(result);
}

string Execute_MarketInfo()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Type", type);
   
   double result = MarketInfo(symbol, type);
   return CreateSuccessResponse(result);
}

string Execute_MessageBox(bool use_ext_params)
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Text", text);
   
   int result;
   if (use_ext_params)
   {
      GET_STRING_JSON_VALUE(jo, "Caption", caption);
      GET_INT_JSON_VALUE(jo, "Flag", flag);
      result = MessageBox(text, caption, flag);
   }
   else
   {
      result = MessageBox(text);
   }
   
   return CreateSuccessResponse(result);
}

string Execute_PlaySound()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Filename", filename);
   
   bool result = PlaySound(filename);
   return CreateSuccessResponse(result);
}

string Execute_Print()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Msg", msg);
   
   Print(msg);
   return CreateSuccessResponse();
}

string Execute_SendFTP(bool use_path)
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Filename", filename);
   
   bool result;
   if (use_path)
   {
      GET_STRING_JSON_VALUE(jo, "FtpPath", ftp_path);
      result = SendFTP(filename, ftp_path);
   }
   else
   {
      result = SendFTP(filename);
   }

   return CreateSuccessResponse(result);
}

string Execute_SendMail()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Subject", subject);
   GET_STRING_JSON_VALUE(jo, "SomeText", some_text);
   
   bool result = SendMail(subject, some_text);
   return CreateSuccessResponse(result);
}

string Execute_Sleep()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Milliseconds", milliseconds);
   
   Sleep(milliseconds);
   return CreateSuccessResponse();
}

string Execute_TerminalCompany()
{
   string result = TerminalCompany();
   return CreateSuccessResponse(result);
}

string Execute_TerminalName()
{
   string result = TerminalName();
   return CreateSuccessResponse(result);
}

string Execute_TerminalPath()
{
   string result = TerminalPath();
   return CreateSuccessResponse(result);
}

string Execute_Day()
{
   int result = Day();
   return CreateSuccessResponse(result);
}

string Execute_DayOfWeek()
{
   int result = DayOfWeek();
   return CreateSuccessResponse(result);
}

string Execute_DayOfYear()
{
   int result = DayOfYear();
   return CreateSuccessResponse(result);
}

string Execute_Hour()
{
   int result = Hour();
   return CreateSuccessResponse(result);
}

string Execute_Minute()
{
   int result = Minute();
   return CreateSuccessResponse(result);
}

string Execute_Month()
{
   int result = Month();
   return CreateSuccessResponse(result);
}

string Execute_Seconds()
{
   int result = Seconds();
   return CreateSuccessResponse(result);
}

string Execute_TimeCurrent()
{
   int result = TimeCurrent();
   return CreateSuccessResponse(result);
}

string Execute_TimeDay()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Date", date);
   
   int result = TimeDay(date);
   return CreateSuccessResponse(result);
}

string Execute_TimeDayOfWeek()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Date", date);
   
   int result = TimeDayOfWeek(date);
   return CreateSuccessResponse(result);
}

string Execute_TimeDayOfYear()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Date", date);

   int result = TimeDayOfYear(date);
   return CreateSuccessResponse(result);
}

string Execute_TimeHour()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Time", time);
   
   int result = TimeHour(time);
   return CreateSuccessResponse(result);
}

string Execute_TimeLocal()
{
   int result = TimeLocal();
   return CreateSuccessResponse(result);
}

string Execute_TimeMinute()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Time", time);
   
   int result = TimeMinute(time);
   return CreateSuccessResponse(result);
}

string Execute_TimeMonth()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Time", time);
   
   int result = TimeMonth(time);
   return CreateSuccessResponse(result);
}

string Execute_TimeSeconds()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Time", time);
   
   int result = TimeSeconds(time);
   return CreateSuccessResponse(result);
}

string Execute_TimeYear()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Time", time);
   
   int result = TimeYear(time);
   return CreateSuccessResponse(result);
}

string Execute_Year()
{
   int result = Year();
   return CreateSuccessResponse(result);
}

string Execute_GlobalVariableCheck()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Name", name);
   
   bool result = GlobalVariableCheck(name);
   return CreateSuccessResponse(result);
}

string Execute_GlobalVariableDel()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Name", name);

   bool result = GlobalVariableDel(name);
   return CreateSuccessResponse(result);
}

string Execute_GlobalVariableGet()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Name", name);

   double result = GlobalVariableGet(name);
   return CreateSuccessResponse(result);
}

string Execute_GlobalVariableName()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Index", index);

   string result = GlobalVariableName(index);
   return CreateSuccessResponse(result);
}

string Execute_GlobalVariableSet()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Name", name);
   GET_DOUBLE_JSON_VALUE(jo, "Value", value);
   
   int result = GlobalVariableSet(name, value);
   return CreateSuccessResponse(result);
}

string Execute_GlobalVariableSetOnCondition()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Name", name);
   GET_DOUBLE_JSON_VALUE(jo, "Value", value);
   GET_DOUBLE_JSON_VALUE(jo, "CheckValue", check_value);
   
   bool result = GlobalVariableSetOnCondition(name, value, check_value);
   return CreateSuccessResponse(result);
}

string Execute_GlobalVariablesDeleteAll()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "PrefixName", prefix_name);
   
   int result = GlobalVariablesDeleteAll(prefix_name);
   return CreateSuccessResponse(result);
}

string Execute_GlobalVariablesTotal()
{
   int result = GlobalVariablesTotal();
   return CreateSuccessResponse(result);
}

string Execute_iAC()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   double result = iAC(symbol, timeframe, shift);
   return CreateSuccessResponse(result);
}

string Execute_iAD()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iAD(symbol, timeframe, shift);
   return CreateSuccessResponse(result);
}

string Execute_iAlligator()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "JawPeriod", jaw_period);
   GET_INT_JSON_VALUE(jo, "JawShift", jaw_shift);
   GET_INT_JSON_VALUE(jo, "TeethPeriod", teeth_period);
   GET_INT_JSON_VALUE(jo, "TeethShift", teeth_shift);
   GET_INT_JSON_VALUE(jo, "LipsPeriod", lips_period);
   GET_INT_JSON_VALUE(jo, "LipsShift", lips_shift);
   GET_INT_JSON_VALUE(jo, "MaMethod", ma_method);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Mode", mode);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iAlligator(symbol, timeframe, jaw_period, jaw_shift, teeth_period, teeth_shift, lips_period, lips_shift, ma_method, applied_price, mode, shift);
   return CreateSuccessResponse(result);
}

string Execute_iADX()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Mode", mode);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iADX(symbol, timeframe, period, applied_price, mode, shift);
   return CreateSuccessResponse(result);
}

string Execute_iATR()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   double result = iATR(symbol, timeframe, period, shift);
   return CreateSuccessResponse(result);
}

string Execute_iAO()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iAO(symbol, timeframe, shift);
   return CreateSuccessResponse(result);
}

string Execute_iBearsPower()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iBearsPower(symbol, timeframe, period, applied_price, shift);
   return CreateSuccessResponse(result);
}

string Execute_iBands()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_DOUBLE_JSON_VALUE(jo, "Deviation", deviation);
   GET_INT_JSON_VALUE(jo, "BandsShift", bands_shift);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Mode", mode);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   double result = iBands(symbol, timeframe, period, deviation, bands_shift, applied_price, mode, shift);
   return CreateSuccessResponse(result);
}

string Execute_iBandsOnArray()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Total", total);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_DOUBLE_JSON_VALUE(jo, "Deviation", deviation);
   GET_INT_JSON_VALUE(jo, "BandsShift", bands_shift);
   GET_INT_JSON_VALUE(jo, "Mode", mode);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   CHECK_JSON_VALUE(jo, "Data");
   JSONArray* data_jo = jo.p.getArray("Data");
   
   double data[];
   ArrayResize(data, data_jo.size());
   
   for(int i = 0; i < data_jo.size(); i++)
      data[i] = (datetime) data_jo.getDouble(i);
      
   double result = iBandsOnArray(data, total, period, deviation, bands_shift, mode, shift);
   return CreateSuccessResponse(result);
}

string Execute_iBullsPower()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   double result = iBullsPower(symbol, timeframe, period, applied_price, shift);
   return CreateSuccessResponse(result);
}

string Execute_iCCI()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   double result = iCCI(symbol, timeframe, period, applied_price, shift);
   return CreateSuccessResponse(result);
}

string Execute_iCCIOnArray()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Total", total);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   CHECK_JSON_VALUE(jo, "Data");
   JSONArray* data_jo = jo.p.getArray("Data");
   
   double data[];
   ArrayResize(data, data_jo.size());
   
   for(int i = 0; i < data_jo.size(); i++)
      data[i] = (datetime) data_jo.getDouble(i);
      
   double result = iCCIOnArray(data, total, period, shift);
   return CreateSuccessResponse(result);
}

string Execute_iDeMarker()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iDeMarker(symbol, timeframe, period, shift);
   return CreateSuccessResponse(result);
}

string Execute_iEnvelopes()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "MaPeriod", ma_period);
   GET_INT_JSON_VALUE(jo, "MaMethod", ma_method);
   GET_INT_JSON_VALUE(jo, "MaShift", ma_shift);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_DOUBLE_JSON_VALUE(jo, "Deviation", deviation);
   GET_INT_JSON_VALUE(jo, "Mode", mode);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iEnvelopes(symbol, timeframe, ma_period, ma_method, ma_shift, applied_price, deviation, mode, shift);
   return CreateSuccessResponse(result);
}

string Execute_iEnvelopesOnArray()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Total", total);
   GET_INT_JSON_VALUE(jo, "MaPeriod", ma_period);
   GET_INT_JSON_VALUE(jo, "MaMethod", ma_method);
   GET_INT_JSON_VALUE(jo, "MaShift", ma_shift);
   GET_DOUBLE_JSON_VALUE(jo, "Deviation", deviation);
   GET_INT_JSON_VALUE(jo, "Mode", mode);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   CHECK_JSON_VALUE(jo, "Data");
   JSONArray* data_jo = jo.p.getArray("Data");
   
   double data[];
   ArrayResize(data, data_jo.size());
   
   for(int i = 0; i < data_jo.size(); i++)
      data[i] = (datetime) data_jo.getDouble(i);

   double result = iEnvelopesOnArray(data, total, ma_period, ma_method, ma_shift, deviation, mode, shift);
   return CreateSuccessResponse(result);
}

string Execute_iForce()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "MaMethod", ma_method);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   double result = iForce(symbol, timeframe, period, ma_method, applied_price, shift);
   return CreateSuccessResponse(result);
}

string Execute_iFractals()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Mode", mode);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   double result = iFractals(symbol, timeframe, mode, shift);
   return CreateSuccessResponse(result);
}

string Execute_iGator()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "JawPeriod", jaw_period);
   GET_INT_JSON_VALUE(jo, "JawShift", jaw_shift);
   GET_INT_JSON_VALUE(jo, "TeethPeriod", teeth_period);
   GET_INT_JSON_VALUE(jo, "TeethShift", teeth_shift);
   GET_INT_JSON_VALUE(jo, "LipsPeriod", lips_period);
   GET_INT_JSON_VALUE(jo, "LipsShift", lips_shift);
   GET_INT_JSON_VALUE(jo, "MaMethod", ma_method);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Mode", mode);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   double result = iGator(symbol, timeframe, jaw_period, jaw_shift, teeth_period, teeth_shift, lips_period, lips_shift, ma_method, applied_price, mode, shift);
   return CreateSuccessResponse(result);
}

string Execute_iIchimoku()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "TenkanSen", tenkan_sen);
   GET_INT_JSON_VALUE(jo, "KijunSen", kijun_sen);
   GET_INT_JSON_VALUE(jo, "SenkouSpanB", senkou_span_b);
   GET_INT_JSON_VALUE(jo, "Mode", mode);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   double result = iIchimoku(symbol, timeframe, tenkan_sen, kijun_sen, senkou_span_b, mode, shift);
   return CreateSuccessResponse(result);
}

string Execute_iBWMFI()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   double result = iBWMFI(symbol, timeframe, shift);
   return CreateSuccessResponse(result);
}

string Execute_iMomentum()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iMomentum(symbol, timeframe, period, applied_price, shift);
   return CreateSuccessResponse(result);
}

string Execute_iMomentumOnArray()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Total", total);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   CHECK_JSON_VALUE(jo, "Data");
   JSONArray* data_jo = jo.p.getArray("Data");
   
   double data[];
   ArrayResize(data, data_jo.size());
   
   for(int i = 0; i < data_jo.size(); i++)
      data[i] = (datetime) data_jo.getDouble(i);
      
   double result = iMomentumOnArray(data, total, period, shift);
   return CreateSuccessResponse(result);
}

string Execute_iMFI()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iMFI(symbol, timeframe, period, shift);
   return CreateSuccessResponse(result);
}

string Execute_iMA()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "MaShift", ma_shift);
   GET_INT_JSON_VALUE(jo, "MaMethod", ma_method);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   double result = iMA(symbol, timeframe, period, ma_shift, ma_method, applied_price, shift);
   return CreateSuccessResponse(result);
}

string Execute_iMAOnArray()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Total", total);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "MaShift", ma_shift);
   GET_INT_JSON_VALUE(jo, "MaMethod", ma_method);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   CHECK_JSON_VALUE(jo, "Data");
   JSONArray* data_jo = jo.p.getArray("Data");
   
   double data[];
   ArrayResize(data, data_jo.size());
   
   for(int i = 0; i < data_jo.size(); i++)
      data[i] = (datetime) data_jo.getDouble(i);
      
   double result = iMAOnArray(data, total, period, ma_shift, ma_method, shift);
   return CreateSuccessResponse(result);
}

string Execute_iOsMA()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "FastEmaPeriod", fast_ema_period);
   GET_INT_JSON_VALUE(jo, "SlowEmaPeriod", slow_ema_period);
   GET_INT_JSON_VALUE(jo, "SignalPeriod", signal_period);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iOsMA(symbol, timeframe, fast_ema_period, slow_ema_period, signal_period, applied_price, shift);
   return CreateSuccessResponse(result);
}

string Execute_iMACD()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "FastEmaPeriod", fast_ema_period);
   GET_INT_JSON_VALUE(jo, "SlowEmaPeriod", slow_ema_period);
   GET_INT_JSON_VALUE(jo, "SignalPeriod", signal_period);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Mode", mode);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iMACD(symbol, timeframe, fast_ema_period, slow_ema_period, signal_period, applied_price, mode, shift);
   return CreateSuccessResponse(result);
}

string Execute_iOBV()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iOBV(symbol, timeframe, applied_price, shift);
   return CreateSuccessResponse(result);
}

string Execute_iSAR()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_DOUBLE_JSON_VALUE(jo, "Step", step);
   GET_DOUBLE_JSON_VALUE(jo, "Maximum", maximum);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iSAR(symbol, timeframe, step, maximum, shift);
   return CreateSuccessResponse(result);
}

string Execute_iRSI()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iRSI(symbol, timeframe, period, applied_price, shift);
   return CreateSuccessResponse(result);
}

string Execute_iRSIOnArray()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Total", total);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   CHECK_JSON_VALUE(jo, "Data");
   JSONArray* data_jo = jo.p.getArray("Data");
   
   double data[];
   ArrayResize(data, data_jo.size());
   
   for(int i = 0; i < data_jo.size(); i++)
      data[i] = (datetime) data_jo.getDouble(i);
   
   double result = iRSIOnArray(data, total, period, shift);
   return CreateSuccessResponse(result);
}

string Execute_iRVI()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "Mode", mode);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iRVI(symbol, timeframe, period, mode, shift);
   return CreateSuccessResponse(result);
}

string Execute_iStdDev()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "MaPeriod", ma_period);
   GET_INT_JSON_VALUE(jo, "MaShift", ma_shift);
   GET_INT_JSON_VALUE(jo, "MaMethod", ma_method);
   GET_INT_JSON_VALUE(jo, "AppliedPrice", applied_price);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iStdDev(symbol, timeframe, ma_period, ma_shift, ma_method, applied_price, shift);
   return CreateSuccessResponse(result);
}

string Execute_iStdDevOnArray()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Total", total);
   GET_INT_JSON_VALUE(jo, "MaPeriod", ma_period);
   GET_INT_JSON_VALUE(jo, "MaShift", ma_shift);
   GET_INT_JSON_VALUE(jo, "MaMethod", ma_method);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   CHECK_JSON_VALUE(jo, "Data");
   JSONArray* data_jo = jo.p.getArray("Data");
   
   double data[];
   ArrayResize(data, data_jo.size());
   
   for(int i = 0; i < data_jo.size(); i++)
      data[i] = (datetime) data_jo.getDouble(i);

   double result = iStdDevOnArray(data, total, ma_period, ma_shift, ma_method, shift);
   return CreateSuccessResponse(result);
}

string Execute_iStochastic()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Kperiod", kperiod);
   GET_INT_JSON_VALUE(jo, "Dperiod", dperiod);
   GET_INT_JSON_VALUE(jo, "Slowing", slowing);
   GET_INT_JSON_VALUE(jo, "Method", method);
   GET_INT_JSON_VALUE(jo, "PriceField", price_field);
   GET_INT_JSON_VALUE(jo, "Mode", mode);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iStochastic(symbol, timeframe, kperiod, dperiod, slowing, method, price_field, mode, shift);
   return CreateSuccessResponse(result);
}

string Execute_iWPR()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Period", period);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iWPR(symbol, timeframe, period, shift);
   return CreateSuccessResponse(result);
}

string Execute_iBars()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);

   int result = iBars(symbol, timeframe);
   return CreateSuccessResponse(result);
}

string Execute_iBarShift()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Time", time);
   GET_BOOL_JSON_VALUE(jo, "Exact", exact);

   int result = iBarShift(symbol, timeframe, (datetime)time, exact);
   return CreateSuccessResponse(result);
}

string Execute_iClose()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
      
   double result = iClose(symbol, timeframe, shift);
   return CreateSuccessResponse(result);
}

string Execute_iHigh()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iHigh(symbol, timeframe, shift);
   return CreateSuccessResponse(result);
}

string Execute_iHighest()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Type", type);
   GET_INT_JSON_VALUE(jo, "Count", count);
   GET_INT_JSON_VALUE(jo, "StartValue", start_value);

   int result = iHighest(symbol, timeframe, type, count, start_value);
   return CreateSuccessResponse(result);
}

string Execute_iLow()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iLow(symbol, timeframe, shift);
   return CreateSuccessResponse(result);
}

string Execute_iLowest()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Type", type);
   GET_INT_JSON_VALUE(jo, "Count", count);
   GET_INT_JSON_VALUE(jo, "StartValue", start_value);
   
   int result = iLowest(symbol, timeframe, type, count, start_value);
   return CreateSuccessResponse(result);
}

string Execute_iOpen()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iOpen(symbol, timeframe, shift);
   return CreateSuccessResponse(result);
}

string Execute_iTime()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   int result = iTime(symbol, timeframe, shift);
   return CreateSuccessResponse(result);
}

string Execute_iVolume()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result = iVolume(symbol, timeframe, shift);
   return CreateSuccessResponse(result);
}

string Execute_iCloseArray()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);

   int barsCount = iBars(symbol, timeframe);
   JSONArray* result = new JSONArray();
   for(int i = 0; i < barsCount; i++)
   {
      double value = iClose(symbol, timeframe, i);
      result.put(i, new JSONNumber(value));
   }
   return CreateSuccessResponse(result);
}

string Execute_iHighArray()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);

   int barsCount = iBars(symbol, timeframe);
   JSONArray* result = new JSONArray();
   for(int i = 0; i < barsCount; i++)
   {
      double value = iHigh(symbol, timeframe, i);
      result.put(i, new JSONNumber(value));
   }
   return CreateSuccessResponse(result);
}

string Execute_iLowArray()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);

   int barsCount = iBars(symbol, timeframe);
   JSONArray* result = new JSONArray();
   for(int i = 0; i < barsCount; i++)
   {
      double value = iLow(symbol, timeframe, i);
      result.put(i, new JSONNumber(value));
   }
   return CreateSuccessResponse(result);
}

string Execute_iOpenArray()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);

   int barsCount = iBars(symbol, timeframe);   
   JSONArray* result = new JSONArray();
   for(int i = 0; i < barsCount; i++)
   {
      double value = iOpen(symbol, timeframe, i);
      result.put(i, new JSONNumber(value));
   }
   return CreateSuccessResponse(result);
}

string Execute_iVolumeArray()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);

   int barsCount = iBars(symbol, timeframe);   
   JSONArray* result = new JSONArray();
   for(int i = 0; i < barsCount; i++)
   {
      double value = iVolume(symbol, timeframe, i);
      result.put(i, new JSONNumber(value));
   }
   return CreateSuccessResponse(result);
}

string Execute_iTimeArray()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);

   int barsCount = iBars(symbol, timeframe);   
   JSONArray* result = new JSONArray();
   for(int i = 0; i < barsCount; i++)
   {
      double value = iTime(symbol, timeframe, i);
      result.put(i, new JSONNumber(value));
   }
   return CreateSuccessResponse(result);
}

string Execute_RefreshRates()
{
   bool result = RefreshRates();
   return CreateSuccessResponse(result);
}

string Execute_TerminalInfoString()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "PropertyId", propertyId);

   string result = TerminalInfoString(propertyId);
   StringReplace(result, "\\", "\\\\");
   return CreateSuccessResponse(result);
}

string Execute_SymbolInfoString()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "PropId", propId);

   string result = SymbolInfoString(symbol, propId);
   return CreateSuccessResponse(result);
}

string Execute_BacktestingReady()
{
   if (IsTesting())
   {
      Print("Remote client is ready for backteting");
      IsRemoteReadyForTesting = true;
   }
   
   return CreateSuccessResponse(IsRemoteReadyForTesting);
}

string Execute_SymbolsTotal()
{
   GET_JSON_PAYLOAD(jo);
   GET_BOOL_JSON_VALUE(jo, "Selected", selected);

   int result = SymbolsTotal(selected);
   return CreateSuccessResponse(result);
}

string Execute_SymbolName()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Pos", pos);
   GET_BOOL_JSON_VALUE(jo, "Selected", selected);

   string result = SymbolName(pos, selected);
   return CreateSuccessResponse(result);
}

string Execute_SymbolSelect()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Name", name);
   GET_BOOL_JSON_VALUE(jo, "Select", select);

   bool result = SymbolSelect(name, select);
   return CreateSuccessResponse(result);
}

string Execute_SymbolInfoInteger()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Name", name);
   GET_INT_JSON_VALUE(jo, "PropId", propId);

   long result = SymbolInfoInteger(name, propId);
   return CreateSuccessResponse(result);
}

string Execute_TerminalInfoInteger()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "PropertyId", propertyId);
   
   int result = TerminalInfoInteger(propertyId);
   return CreateSuccessResponse(result);
}

string Execute_TerminalInfoDouble()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "PropertyId", propertyId);

   double result = TerminalInfoDouble(propertyId);
   return CreateSuccessResponse(result);
}

string Execute_CharId()
{
   long result = ChartID();
   return CreateSuccessResponse(result);
}

string Execute_ChartRedraw()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);

   ChartRedraw(chartId);
   return CreateSuccessResponse();
}

string Execute_ObjectCreate()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_INT_JSON_VALUE(jo, "ObjectType", objectType);
   GET_INT_JSON_VALUE(jo, "SubWindow", subWindow);
   GET_INT_JSON_VALUE(jo, "Time1", time1);
   GET_DOUBLE_JSON_VALUE(jo, "Price1", price1);
   GET_INT_JSON_VALUE(jo, "Time2", time2);
   GET_DOUBLE_JSON_VALUE(jo, "Price2", price2);
   GET_INT_JSON_VALUE(jo, "Time3", time3);
   GET_DOUBLE_JSON_VALUE(jo, "Price3", price3);   

   bool result = ObjectCreate(chartId, objectName, objectType, subWindow, time1, price1, time2, price2, time3, price3);
   return CreateSuccessResponse(result);
}

string Execute_ObjectName()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "ObjectIndex", objectIndex);
   GET_INT_JSON_VALUE(jo, "SubWindow", subWindow);
   GET_INT_JSON_VALUE(jo, "ObjectType", objectType);

   string result = ObjectName(chartId, objectIndex, subWindow, objectType);
   return CreateSuccessResponse(result);
}

string Execute_ObjectDelete()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   
   bool result =  ObjectDelete(chartId, objectName);
   return CreateSuccessResponse(result);
}

string Execute_ObjectsDeleteAll()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "SubWindow", subWindow);
   GET_INT_JSON_VALUE(jo, "ObjectType", objectType);
   
   int result = ObjectsDeleteAll(chartId, subWindow, objectType);
   return CreateSuccessResponse(result);
}

string Execute_ObjectFind()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   
   int result = ObjectFind(chartId, objectName);
   return CreateSuccessResponse(result);
}

string Execute_ObjectGetTimeByValue()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_DOUBLE_JSON_VALUE(jo, "Value", value);
   GET_INT_JSON_VALUE(jo, "LineId", lineId);
   
   int result = ObjectGetTimeByValue(chartId, objectName, value, lineId);
   return CreateSuccessResponse(result);
}

string Execute_ObjectGetValueByTime()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_INT_JSON_VALUE(jo, "Time", time);
   GET_INT_JSON_VALUE(jo, "LineId", lineId);
   
   double result = ObjectGetValueByTime(chartId, objectName, time, lineId);
   return CreateSuccessResponse(result);
}

string Execute_ObjectMove()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_INT_JSON_VALUE(jo, "PointIndex", pointIndex);
   GET_INT_JSON_VALUE(jo, "Time", time);
   GET_DOUBLE_JSON_VALUE(jo, "Price", price);

   bool result = ObjectMove(chartId, objectName, pointIndex, time, price);
   return CreateSuccessResponse(result);
}

string Execute_ObjectsTotal()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "SubWindow", subWindow);
   GET_INT_JSON_VALUE(jo, "Type", type);
   
   int result = ObjectsTotal(chartId, subWindow, type);
   return CreateSuccessResponse(result);
}

string Execute_ObjectGetDouble()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_INT_JSON_VALUE(jo, "PropId", propId);
   GET_INT_JSON_VALUE(jo, "PropModifier", propModifier);

   double result = ObjectGetDouble(chartId, objectName, propId, propModifier);
   return CreateSuccessResponse(result);
}

string Execute_ObjectGetInteger()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_INT_JSON_VALUE(jo, "PropId", propId);
   GET_INT_JSON_VALUE(jo, "PropModifier", propModifier);
   
   long result = ObjectGetInteger(chartId, objectName, propId, propModifier);
   return CreateSuccessResponse(result);
}

string Execute_ObjectGetString()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_INT_JSON_VALUE(jo, "PropId", propId);
   GET_INT_JSON_VALUE(jo, "PropModifier", propModifier);
   
   string result = ObjectGetString(chartId, objectName, propId, propModifier);
   return CreateSuccessResponse(result);
}

string Execute_ObjectSetDouble()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_INT_JSON_VALUE(jo, "PropId", propId);
   GET_DOUBLE_JSON_VALUE(jo, "PropValue", propValue);

   bool result; 
   if (jo.p.getValue("PropModifier") != NULL)
   {
      int propModifier = jo.p.getInt("PropModifier");
      result = ObjectSetDouble(chartId, objectName, propId, propModifier, propValue);
   }
   else
   {
      result = ObjectSetDouble(chartId, objectName, propId, propValue);
   }
   
   return CreateSuccessResponse(result);
}

string Execute_ObjectSetInteger()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_INT_JSON_VALUE(jo, "PropId", propId);
   GET_DOUBLE_JSON_VALUE(jo, "PropValue", propValue);
   
   bool result; 
   if (jo.p.getValue("PropModifier") != NULL)
   {
      int propModifier = jo.p.getInt("PropModifier");
      result = ObjectSetInteger(chartId, objectName, propId, propModifier, propValue);
   }
   else
   {
      result = ObjectSetInteger(chartId, objectName, propId, propValue);
   }
   
   return CreateSuccessResponse(result);
}

string Execute_ObjectSetString()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_INT_JSON_VALUE(jo, "PropId", propId);
   GET_STRING_JSON_VALUE(jo, "PropValue", propValue);
   
   bool result; 
   if (jo.p.getValue("PropModifier") != NULL)
   {
      int propModifier = jo.p.getInt("PropModifier");
      result = ObjectSetString(chartId, objectName, propId, propModifier, propValue);
   }
   else
   {
      result = ObjectSetString(chartId, objectName, propId, propValue);
   }
   
   return CreateSuccessResponse(result);
}

string Execute_TextSetFont()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Name", name);
   GET_INT_JSON_VALUE(jo, "Size", size);
   GET_INT_JSON_VALUE(jo, "Flags", flags);
   GET_INT_JSON_VALUE(jo, "Orientation", orientation);

   bool result = TextSetFont(name, size, flags, orientation);
   return CreateSuccessResponse(result);
}

string Execute_ObjectDescription()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   
   string result = ObjectDescription(objectName);
   return CreateSuccessResponse(result);
}

string Execute_ObjectGetFiboDescription()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_INT_JSON_VALUE(jo, "Index", index);
   
   string result = ObjectGetFiboDescription(objectName, index);
   return CreateSuccessResponse(result);
}

string Execute_ObjectGetShiftByValue()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_DOUBLE_JSON_VALUE(jo, "Value", value);

   int result = ObjectGetShiftByValue(objectName, value);
   return CreateSuccessResponse(result);
}

string Execute_ObjectGetValueByShift()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
   double result = ObjectGetValueByShift(objectName, shift);
   return CreateSuccessResponse(result);
}

string Execute_ObjectSet()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_INT_JSON_VALUE(jo, "Index", index);
   GET_DOUBLE_JSON_VALUE(jo, "Value", value);
   
   bool result = ObjectSet(objectName, index, value);
   return CreateSuccessResponse(result);
}

string Execute_ObjectSetFiboDescription()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_INT_JSON_VALUE(jo, "Index", index);
   GET_STRING_JSON_VALUE(jo, "Text", text);
   
   bool result = ObjectSetFiboDescription(objectName, index, text);
   return CreateSuccessResponse(result);
}

string Execute_ObjectSetText()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   GET_STRING_JSON_VALUE(jo, "Text", text);
   GET_INT_JSON_VALUE(jo, "FontSize", fontSize);
   GET_STRING_JSON_VALUE(jo, "FontName", fontName);
   GET_INT_JSON_VALUE(jo, "TextColor", textColor);
   
   bool result = ObjectSetText(objectName, text, fontSize, fontName, textColor);
   return CreateSuccessResponse(result);
}

string Execute_ObjectType()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "ObjectName", objectName);
   
   int result = ObjectType(objectName);
   return CreateSuccessResponse(result);
}

string Execute_UnlockTicks()
{
   if (!IsTesting())
   {
      Print("WARNING: function UnlockTicks can be used only for backtesting");
      return CreateErrorResponse(-1, "Function UnlockTicks can be used only for backtesting");
   }
   
   _is_ticks_locked = false;
   return CreateSuccessResponse();
}

string Execute_ChartApplyTemplate()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "Filename", filename);
   
   bool result = ChartApplyTemplate(chartId, filename);
   return CreateSuccessResponse(result);
}

string Execute_ChartSaveTemplate()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "Filename", filename);
   
   bool result = ChartSaveTemplate(chartId, filename);
   return CreateSuccessResponse(result);
}

string Execute_ChartWindowFind()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "IndicatorShortname", indicatorShortname);
   
   int result = ChartWindowFind(chartId, indicatorShortname);
   return CreateSuccessResponse(result);
}

string Execute_ChartTimePriceToXY()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "SubWindow", subWindow);
   GET_INT_JSON_VALUE(jo, "Time", time);
   GET_DOUBLE_JSON_VALUE(jo, "Price", price);
   
   int x,y;
   bool ok = ChartTimePriceToXY(chartId, subWindow, time, price, x, y);
   
   JSONObject* result_value_jo = new JSONObject();
   result_value_jo.put("RetVal", new JSONBool(ok));
   JSONObject* xy_jo = new JSONObject();
   xy_jo.put("X", new JSONNumber(x));
   xy_jo.put("Y", new JSONNumber(y));
   result_value_jo.put("Result", xy_jo);
   
   return CreateSuccessResponse(result_value_jo);
}

string Execute_ChartXYToTimePrice()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "X", x);
   GET_INT_JSON_VALUE(jo, "Y", y);
   
   int sub_window;
   datetime mt_time;
   double price;
   bool ok = ChartXYToTimePrice(chartId, x, y, sub_window, mt_time, price);
   
   JSONObject* result_value_jo = new JSONObject();
   result_value_jo.put("RetVal", new JSONBool(ok));
   JSONObject* time_price_jo = new JSONObject();
   time_price_jo.put("SubWindow", new JSONNumber(sub_window));
   time_price_jo.put("Time", new JSONNumber((long)mt_time));
   time_price_jo.put("Price", new JSONNumber(price));
   result_value_jo.put("Result", time_price_jo);
   
   return CreateSuccessResponse(result_value_jo);
}

string Execute_ChartOpen()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Period", period);
   
   long result = ChartOpen(symbol, period);
   return CreateSuccessResponse(result);
}

string Execute_ChartFirst()
{
   long result = ChartFirst();
   return CreateSuccessResponse(result);
}

string Execute_ChartNext()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   
   long result = ChartNext(chartId);
   return CreateSuccessResponse(result);
}

string Execute_ChartClose()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);

   bool result = ChartClose(chartId);
   return CreateSuccessResponse(result);
}

string Execute_ChartSymbol()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);

   string result = ChartSymbol(chartId);
   return CreateSuccessResponse(result);
}

string Execute_ChartPeriod()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);

   int result = ChartPeriod(chartId);
   return CreateSuccessResponse(result);
}

string Execute_ChartSetDouble()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "PropId", propId);
   GET_DOUBLE_JSON_VALUE(jo, "Value", value);
   
   bool result = ChartSetDouble(chartId, propId, value);
   return CreateSuccessResponse(result);
}

string Execute_ChartSetInteger()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "PropId", propId);
   GET_LONG_JSON_VALUE(jo, "Value", value);
   
   bool result = ChartSetInteger(chartId, propId, value);
   return CreateSuccessResponse(result);
}

string Execute_ChartSetString()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "PropId", propId);
   GET_STRING_JSON_VALUE(jo, "Value", value);
   
   bool result = ChartSetString(chartId, propId, value);
   return CreateSuccessResponse(result);
}

string Execute_ChartGetDouble()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "PropId", propId);
   GET_INT_JSON_VALUE(jo, "SubWindow", subWindow);
   
   double result = ChartGetDouble(chartId, propId, subWindow);
   return CreateSuccessResponse(result);
}

string Execute_ChartGetInteger()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "PropId", propId);
   GET_INT_JSON_VALUE(jo, "SubWindow", subWindow);
   
   long result = ChartGetInteger(chartId, propId, subWindow);
   return CreateSuccessResponse(result);
}

string Execute_ChartGetString()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "PropId", propId);
   
   string result = ChartGetString(chartId, propId);
   return CreateSuccessResponse(result);
}

string Execute_ChartNavigate()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "Position", position);
   GET_INT_JSON_VALUE(jo, "Shift", shift);
   
  bool result = ChartNavigate(chartId, position, shift);
  return CreateSuccessResponse(result);
}

string Execute_ChartIndicatorDelete()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "SubWindow", subWindow);
   GET_STRING_JSON_VALUE(jo, "IndicatorShortname", indicatorShortname);
   
   bool result = ChartIndicatorDelete(chartId, subWindow, indicatorShortname);
   return CreateSuccessResponse(result);
}

string Execute_ChartIndicatorName()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "SubWindow", subWindow);
   GET_INT_JSON_VALUE(jo, "Index", index);
   
   string result = ChartIndicatorName(chartId, subWindow, index);
   return CreateSuccessResponse(result);
}

string Execute_ChartIndicatorsTotal()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_INT_JSON_VALUE(jo, "SubWindow", subWindow);
   
   int result = ChartIndicatorsTotal(chartId, subWindow);
   return CreateSuccessResponse(result);
}

string Execute_ChartWindowOnDropped()
{
   int result = ChartWindowOnDropped();
   return CreateSuccessResponse(result);
}

string Execute_ChartPriceOnDropped()
{
   double result = ChartPriceOnDropped();
   return CreateSuccessResponse(result);
}

string Execute_ChartTimeOnDropped()
{
   int result = ChartTimeOnDropped();
   return CreateSuccessResponse(result);
}

string Execute_ChartXOnDropped()
{
   int result = ChartXOnDropped();
   return CreateSuccessResponse(result);
}

string Execute_ChartYOnDropped()
{
   int result = ChartYOnDropped();
   return CreateSuccessResponse(result);
}

string Execute_ChartSetSymbolPeriod()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Period", period);
   
   bool result = ChartSetSymbolPeriod(chartId, symbol, period);
   return CreateSuccessResponse(result);
}

string Execute_ChartScreenShot()
{
   GET_JSON_PAYLOAD(jo);
   GET_LONG_JSON_VALUE(jo, "ChartId", chartId);
   GET_STRING_JSON_VALUE(jo, "Filename", filename);
   GET_INT_JSON_VALUE(jo, "Width", width);
   GET_INT_JSON_VALUE(jo, "Height", height);
   GET_INT_JSON_VALUE(jo, "AlignMode", alignMode);
   
   bool result = ChartScreenShot(chartId, filename, width, height, alignMode);
   return CreateSuccessResponse(result);
}

string Execute_WindowBarsPerChart()
{
   int result = WindowBarsPerChart();
   return CreateSuccessResponse(result);
}

string Execute_WindowExpertName()
{
   string result = WindowExpertName();
   return CreateSuccessResponse(result);
}

string Execute_WindowFind()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Name", name);
   
   int result = WindowFind(name);
   return CreateSuccessResponse(result);
}

string Execute_WindowFirstVisibleBar()
{
   int result = WindowFirstVisibleBar();
   return CreateSuccessResponse(result);
}

string Execute_WindowHandle()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   
   int result = WindowHandle(symbol, timeframe);
   return CreateSuccessResponse(result);
}

string Execute_WindowIsVisible()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Index", index);
   
   bool result = WindowIsVisible(index) != 0;
   return CreateSuccessResponse(result); 
}

string Execute_WindowOnDropped()
{
   int result = WindowOnDropped();
   return CreateSuccessResponse(result);
}

string Execute_WindowPriceMax()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Index", index);
   
   int result = WindowPriceMax(index);
   return CreateSuccessResponse(result);
}

string Execute_WindowPriceMin()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Index", index);
   
   int result = WindowPriceMin(index);
   return CreateSuccessResponse(result);
}

string Execute_WindowPriceOnDropped()
{
   double result = WindowPriceOnDropped();
   return CreateSuccessResponse(result);
}

string Execute_WindowRedraw()
{
   WindowRedraw();
   return CreateSuccessResponse();
}

string Execute_WindowScreenShot()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Filename", filename);
   GET_INT_JSON_VALUE(jo, "SizeX", sizeX);
   GET_INT_JSON_VALUE(jo, "SizeY", sizeY);
   GET_INT_JSON_VALUE(jo, "StartBar", startBar);
   GET_INT_JSON_VALUE(jo, "ChartScale", chartScale);
   GET_INT_JSON_VALUE(jo, "ChartMode", chartMode);
   
   bool result =  WindowScreenShot(filename, sizeX, sizeY, startBar, chartScale, chartMode);
   return CreateSuccessResponse(result);
}

string Execute_WindowTimeOnDropped()
{
   int result = WindowTimeOnDropped();
   return CreateSuccessResponse(result);
}

string Execute_WindowsTotal()
{
   int result = WindowsTotal();
   return CreateSuccessResponse(result);
}

string Execute_WindowXOnDropped()
{
   int result = WindowXOnDropped();
   return CreateSuccessResponse(result);
}

string Execute_WindowYOnDropped()
{
   int result = WindowYOnDropped();
   return CreateSuccessResponse(result);
}

string Execute_ChangeAccount()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Login", login);
   GET_STRING_JSON_VALUE(jo, "Password", password);
   GET_STRING_JSON_VALUE(jo, "Host", host);
   
   bool result = auth(login, password, host);
   return CreateSuccessResponse(result);
}

string Execute_TimeGMT()
{
   int result = TimeGMT();
   return CreateSuccessResponse(result);
}

bool OrderCloseAll()
{
   int total = OrdersTotal();
   for(int i = total-1; i >= 0; i--)
   {
      if (OrderSelect(i, SELECT_BY_POS))
      {
         int type = OrderType();   
         switch(type)
         {
            //Close opened long positions
            case OP_BUY: OrderClose( OrderTicket(), OrderLots(), MarketInfo(OrderSymbol(), MODE_BID), 5, Red );
               break;      
            //Close opened short positions
            case OP_SELL: OrderClose( OrderTicket(), OrderLots(), MarketInfo(OrderSymbol(), MODE_ASK), 5, Red );
               break;
         }
      }
   }

   return (true);
}

string CreateErrorResponse(int code, string error)
{
   JSONValue* jsonError;
   if (code == 0)
      jsonError = new JSONString("0");
   else
      jsonError = new JSONNumber((long)code);
      
   JSONObject *joResponse = new JSONObject();   
   joResponse.put("ErrorCode", jsonError);
   joResponse.put("ErrorMessage", new JSONString(error));

   string res = joResponse.toString();
   delete joResponse;
   return res; 
}

string CreateSuccessResponse(JSONValue* responseBody = NULL)
{
   JSONObject joResponse;
   joResponse.put("ErrorCode", new JSONString("0"));
      
   if (responseBody != NULL)
      joResponse.put("Value", responseBody);   
   
   return joResponse.toString();
}

string CreateSuccessResponse(bool result)
{
   return CreateSuccessResponse(new JSONBool(result));
}

string CreateSuccessResponse(int result)
{
   return CreateSuccessResponse(new JSONNumber(result));
}

string CreateSuccessResponse(long result)
{
   return CreateSuccessResponse(new JSONNumber(result));
}

string CreateSuccessResponse(double result)
{
   return CreateSuccessResponse(new JSONNumber(result));
}

string CreateSuccessResponse(string result)
{
   return CreateSuccessResponse(new JSONString(result));
}


string CreateSuccessResponse(string responseName, JSONValue* responseBody)
{
   JSONObject *joResponse = new JSONObject();
   joResponse.put("ErrorCode", new JSONNumber(ERR_NO_ERROR));

   if (responseBody != NULL)
   {
      joResponse.put(responseName, responseBody);
   }
   
   string res = joResponse.toString();
   delete joResponse;

#ifdef __DEBUG_LOG__   
   PrintFormat("CreateSuccessResponse: %s", res);
#endif
   
   return res;
}

string Execute_GetOrder()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Index", index);
   GET_INT_JSON_VALUE(jo, "Select", select);
   GET_INT_JSON_VALUE(jo, "Pool", pool);

   MtOrder* order = MtOrder::LoadOrder(index, select, pool);
   if (order == NULL)
   {
      return CreateErrorResponse(GetLastError(), "GetOrder failed");
   }
   
   string result = CreateSuccessResponse(order.CreateJson());
   delete order;
   return result;
}

string Execute_GetOrders()
{
   GET_JSON_PAYLOAD(jo);
   GET_INT_JSON_VALUE(jo, "Pool", pool);

   int total = (pool == MODE_HISTORY) ? OrdersHistoryTotal() : OrdersTotal();

   JSONArray* joOrders = new JSONArray();
   for(int pos = 0; pos < total; pos++)
   {
      MtOrder* order = MtOrder::LoadOrder(pos, SELECT_BY_POS, pool);
      if (order == NULL)
      {
         delete joOrders;
         return CreateErrorResponse(GetLastError(), "GetOrders failed");
      }

      joOrders.put(pos, order.CreateJson());
      delete order; 
   }
   
   return CreateSuccessResponse(joOrders);
}

string Execute_iCustom()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_STRING_JSON_VALUE(jo, "Name", name);
   GET_INT_JSON_VALUE(jo, "Mode", mode);
   GET_INT_JSON_VALUE(jo, "Shift", shift);

   double result;

   if (jo.p.getValue("Params") == NULL)
   {
      result = iCustom(symbol, timeframe, name, mode, shift);
   }
   else 
   {
      JSONArray *jaParams = jo.p.getArray("Params");
      int size = jaParams.size();

      if (size < 0 || size > 10)
      {
         return CreateErrorResponse(-1, "Parameter's count is out of range.");
      }

      if (jo.p.getValue("ParamsType") == NULL)
      {
         return CreateErrorResponse(-1, "Undefinded mandatory parameter ParamsType");
      }

      int paramsType =  jo.p.getInt("ParamsType");
      switch (paramsType)
      {
      case 0: //Int
      {
         int intParams[];
         ArrayResize(intParams, size);
         for (int it_i = 0; it_i < size; it_i++)
            intParams[it_i] = jaParams.getInt(it_i);
         result = iCustomT(symbol, timeframe, name, intParams, size, mode, shift);
      }
      break;
      case 1: //Double
      {
         double doubleParams[];
         ArrayResize(doubleParams, size);
         for (int it_d = 0; it_d < size; it_d++)
            doubleParams[it_d] = jaParams.getDouble(it_d);
         result = iCustomT(symbol, timeframe, name, doubleParams, size, mode, shift);
      }
      break;
      case 2: //String
      {
         string stringParams[];
         ArrayResize(stringParams, size);
         for (int it_s = 0; it_s < size; it_s++)
            stringParams[it_s] = jaParams.getString(it_s);
         result = iCustomT(symbol, timeframe, name, stringParams, size, mode, shift);
      }
      break;
      case 3: //Boolean
      {
         bool boolParams[];
         ArrayResize(boolParams, size);
         for (int it_b = 0; it_b < size; it_b++)
            boolParams[it_b] = jaParams.getBool(it_b);
         result = iCustomT(symbol, timeframe, name, boolParams, size, mode, shift);
      }
      break;
      default:
         return CreateErrorResponse(-1, "Unsupported type of iCustom parameters.");
      }
   }

   return CreateSuccessResponse(result);  
}

template<typename T>
double iCustomT(string symbol, int timeframe, string name, T &p[], int count, int mode, int shift)
{
   switch(count)
   {
   case 0:
      return iCustom(symbol, timeframe, name, mode, shift);
      break;
   case 1:
      return iCustom(symbol, timeframe, name, p[0], mode, shift);
   case 2:
      return iCustom(symbol, timeframe, name, p[0], p[1], mode, shift);
   case 3:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], mode, shift);
   case 4:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], p[3], mode, shift);
   case 5:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], p[3], p[4], mode, shift);
   case 6:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], p[3], p[4], p[5], mode, shift);
   case 7:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], p[3], p[4], p[5], p[6], mode, shift);
   case 8:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], mode, shift);
   case 9:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], mode, shift);
   case 10:
      return iCustom(symbol, timeframe, name, p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9], mode, shift);
   default:
         return 0;
   }
}

string Execute_CopyRates()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "SymbolName", symbolName);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeFrame);
   GET_INT_JSON_VALUE(jo, "CopyRatesType", copyRatesType);

   MqlRates rates[];
   ArraySetAsSeries(rates,true);
   int copied = 0;

   if (copyRatesType == 1)
   {
      if (jo.p.getValue("StartPos") == NULL)
      {
         return CreateErrorResponse(-1, "Undefinded mandatory parameter StartPos");
      }
      if (jo.p.getValue("Count") == NULL)
      {
         return CreateErrorResponse(-1, "Undefinded mandatory parameter Count");
      }

      copied = CopyRates(symbolName, timeFrame, jo.p.getInt("StartPos"), jo.p.getInt("Count"), rates);
   }
   else if (copyRatesType == 2)
   {
      if (jo.p.getValue("StartTime") == NULL)
      {
         return CreateErrorResponse(-1, "Undefinded mandatory parameter StartTime");
      }
      if (jo.p.getValue("Count") == NULL)
      {
         return CreateErrorResponse(-1, "Undefinded mandatory parameter Count");
      }

      copied = CopyRates(symbolName, timeFrame, (datetime)jo.p.getInt("StartTime"), jo.p.getInt("Count"), rates);
   }
   else if (copyRatesType == 3)
   {
      if (jo.p.getValue("StartTime") == NULL)
      {
         return CreateErrorResponse(-1, "Undefinded mandatory parameter StartTime");
      }
      if (jo.p.getValue("StopTime") == NULL)
      {
         return CreateErrorResponse(-1, "Undefinded mandatory parameter StopTime");
      }

      copied = CopyRates(symbolName, timeFrame, (datetime)jo.p.getInt("StartTime"), (datetime)jo.p.getInt("StopTime"), rates); 
   }
   else
   {
      return CreateErrorResponse(-1, "Unsupported type of CopyRates.");
   }

   if(copied < 0)
   {
      return CreateErrorResponse(GetLastError(), "CopyRates failed.");
   }

   JSONArray* jaRates = new JSONArray();
   for(int i = 0; i < copied; i++)
   {
      JSONObject *joRate = new JSONObject();
      joRate.put("MtTime", new JSONNumber(rates[i].time));
      joRate.put("Open", new JSONNumber(rates[i].open)); 
      joRate.put("High", new JSONNumber(rates[i].high));
      joRate.put("Low", new JSONNumber(rates[i].low));
      joRate.put("Close", new JSONNumber(rates[i].close));
      joRate.put("TickVolume", new JSONNumber(rates[i].tick_volume));
      joRate.put("Spread", new JSONNumber(rates[i].spread));
      joRate.put("RealVolume", new JSONNumber(rates[i].real_volume));

      jaRates.put(i, joRate);
   }

   return CreateSuccessResponse(jaRates);
}

string Execute_Session()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "SymbolName", symbolName);
   GET_INT_JSON_VALUE(jo, "DayOfWeek", dayOfWeek);
   GET_INT_JSON_VALUE(jo, "SessionIndex", sessionIndex);
   GET_INT_JSON_VALUE(jo, "SessionType", sessionType);  

   MtSession* session = MtSession::LoadSession(symbolName, dayOfWeek, sessionIndex, sessionType);
   JSONObject* result = session.CreateJson();
   delete session;
   return CreateSuccessResponse(result);
}

string Execute_SeriesInfoInteger()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "SymbolName", symbolName);
   GET_INT_JSON_VALUE(jo, "Timeframe", timeframe);
   GET_INT_JSON_VALUE(jo, "PropId", propId);

   long value = 0;
   if (!SeriesInfoInteger(symbolName, timeframe, propId, value))
   {
      return CreateErrorResponse(GetLastError(), "SeriesInfoInteger failed");
   }

   return CreateSuccessResponse(value);
}

string Execute_SymbolInfoDouble()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "SymbolName", symbolName);
   GET_INT_JSON_VALUE(jo, "PropId", propId);

   double value = 0;
   if (!SymbolInfoDouble(symbolName, propId, value))
   {
      return CreateErrorResponse(GetLastError(), "SymbolInfoDouble failed");
   }

   return CreateSuccessResponse(value);
}

string Execute_SymbolInfoTick()
{
   GET_JSON_PAYLOAD(jo);
   GET_STRING_JSON_VALUE(jo, "Symbol", symbol);
   
   MqlTick tick;
   if (!SymbolInfoTick(symbol, tick))
   {
      return CreateErrorResponse(GetLastError(), "SymbolInfoDouble failed");
   }

   JSONObject *joTick = new JSONObject();   
   joTick.put("MtTime", new JSONNumber(tick.time));
   joTick.put("Bid", new JSONNumber(tick.bid)); 
   joTick.put("Ask", new JSONNumber(tick.ask));
   joTick.put("Last", new JSONNumber(tick.last));
   joTick.put("Volume", new JSONNumber(tick.volume));

   return CreateSuccessResponse(joTick);
}

string Execute_GetQuote()
{
   MqlTick tick;
   SymbolInfoTick(Symbol(), tick);
   
   MtQuote quote(Symbol(), tick);
   return CreateSuccessResponse(quote.CreateJson());
}

string Execute_GetSymbols()
{
   GET_JSON_PAYLOAD(jo);
   GET_BOOL_JSON_VALUE(jo, "Selected", selected);
   
   const int symbolsCount = SymbolsTotal(selected);
   JSONArray* jaSymbols = new JSONArray();
   int idx = 0;
   for(int idxSymbol = 0; idxSymbol < symbolsCount; idxSymbol++)
   {      
      string symbol = SymbolName(idxSymbol, selected);
      string firstChar = StringSubstr(symbol, 0, 1);
      if(firstChar != "#" && StringLen(symbol) == 6)
      {        
         jaSymbols.put(idx, new JSONString(symbol));
         idx++;
      } 
   }
   
   return CreateSuccessResponse(jaSymbols);
}