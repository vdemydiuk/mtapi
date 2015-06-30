#property copyright "Vyacheslav Demidyuk"
#property link      "DW"

#include <WinUser32.mqh>
#include <stdlib.mqh>

#import "MTConnector.dll"
   bool initExpert(int expertHandle, int port, string symbol, double bid, double ask, string& err);
   bool deinitExpert(int expertHandle, string& err);   
   bool updateQuote(int expertHandle, string symbol, double bid, double ask, string& err);   
   
   bool sendIntResponse(int expertHandle, int response);
   bool sendBooleanResponse(int expertHandle, int response);
   bool sendDoubleResponse(int expertHandle, double response);
   bool sendStringResponse(int expertHandle, string response);
   bool sendVoidResponse(int expertHandle);
   bool sendDoubleArrayResponse(int expertHandle, double& values[], int size);
   bool sendIntArrayResponse(int expertHandle, int& values[], int size);   
   
   bool getCommandType(int expertHandle, int& res);
   bool getIntValue(int expertHandle, int paramIndex, int& res);
   bool getDoubleValue(int expertHandle, int paramIndex, double& res);
   bool getStringValue(int expertHandle, int paramIndex, string& res);
#import

extern int Port = 8222;

int ExpertHandle;

string message;
bool isCrashed = FALSE;

string symbolValue;
string commentValue;
string msgValue;
string captionValue;
string filenameValue;
string ftp_pathValue;
string subjectValue;
string some_textValue;
string nameValue;
string prefix_nameValue;

int barsCount;
int priceCount;
int volumeCount;
int timeCount;
int index;
int paramIndex;
int arraySize;

int pCommandType;
int cmdValue;
int slippageValue;
int ticketValue;
int oppositeValue;
int magicValue;
int expirationValue;
int arrow_colorValue;
int colorValue;
int indexValue;
int selectValue;
int poolValue;
int errorCodeValue;
int typeValue;
int flagValue;
int millisecondsValue;
int dateValue;
int timeValue;
int timeframeValue;
int shiftValue;
int periodValue;
int applied_priceValue;
int modeValue;
int deviationValue;
int bands_shiftValue;
int ma_periodValue;
int ma_methodValue;
int ma_shiftValue;
int jaw_periodValue;
int jaw_shiftValue;
int teeth_periodValue;
int teeth_shiftValue;
int lips_periodValue;
int lips_shiftValue;
int tenkan_senValue;
int kijun_senValue;
int senkou_span_bValue;
int fast_ema_periodValue;
int slow_ema_periodValue;
int signal_periodValue;
int KperiodValue;
int DperiodValue;
int slowingValue;
int methodValue;
int price_fieldValue;
int exactValue;
int startValue;
int countValue;
int totalValue;
int tempIntValue;

int timeArray[];
int intValuesArray[];

double result;

double lotsValue;
double volumeValue;
double priceValue;
double stoplossValue;
double takeprofitValue;
double valueValue;
double check_value;
double deviationDoubleValue;
double stepValue;
double maximumValue;
double tempDoubleValue;

double priceArray[];
double volumeArray[];
double doubleValuesArray[];

double myBid;
double myAsk;

int preinit()
{
   message        = "111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111" + "";
   symbolValue    = "222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222" + "";
   commentValue   = "333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333" + "";
   msgValue       = "444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444" + "";
   captionValue   = "555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555" + "";
   filenameValue  = "666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666" + "";
   ftp_pathValue  = "777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777" + "";
   subjectValue   = "888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888" + "";
   some_textValue = "999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999" + "";
   nameValue      = "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" + "";
   prefix_nameValue = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" + "";

   return (0);
}

int init() {
   preinit();
     
   myBid = Bid;
   myAsk = Ask;
      
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
      MessageBox("Trade not allowed.", "MtApi", MB_OK);
      isCrashed = TRUE;
      return (1);
   }  

   ExpertHandle = WindowHandle(Symbol(), Period());
   
   if (!initExpert(ExpertHandle, Port, Symbol(), Bid, Ask, message))
   {
       MessageBox(message, "MtApi", MB_OK);
       isCrashed = TRUE;
       return(1);
   }
   
   if (executeCommand() == 1)
   {   
      isCrashed = TRUE;
      return (1);
   }
   
   return (0);
}

int deinit() {
   if (isCrashed == 0) 
   {
      if (!deinitExpert(ExpertHandle, message)) 
      {
         MessageBox(message, "MtApi", MB_OK);
         isCrashed = TRUE;
         return (1);
      }
   }
   
   return (0);
}

int start() 
{
   while(true)
   {
      if (myBid != Bid || myAsk != Ask)
      {         
         updateQuote(ExpertHandle, Symbol(), Bid, Ask, message);
         
         myBid = Bid;
         myAsk = Ask;
      }
   
      if (executeCommand() == 0)      
      {   
         return(0);
      }   
      
      RefreshRates();
   }

   return (0);
}

int executeCommand()
{
   pCommandType = 0;
      
   if (!getCommandType(ExpertHandle, pCommandType))
   {
      Print("[ERROR] getCommandType");
      return (0);
   }         
   
   int commandType = pCommandType;     
   
   switch (commandType) 
   {
   case 0:
      //NoCommand               
      break;
      
   case 1: // OrderSend
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, cmdValue))
      {
         PrintParamError("cmd");
      }
               
      if (!getDoubleValue(ExpertHandle, 2, volumeValue))
      {
         PrintParamError("volume");      
      }
      
      if (!getDoubleValue(ExpertHandle, 3, priceValue))
      {
         PrintParamError("price");   
      }
      
      if (!getIntValue(ExpertHandle, 4, slippageValue))
      {
         PrintParamError("slippage");        
      }
      
      if (!getDoubleValue(ExpertHandle, 5, stoplossValue))
      {
         PrintParamError("stoploss"); 
      }

      if (!getDoubleValue(ExpertHandle, 6, takeprofitValue))
      {
         PrintParamError("takeprofit");
      }
      
      if (!getStringValue(ExpertHandle, 7, commentValue)) 
      {
         PrintParamError("comment");
      }
      
      if (!getIntValue(ExpertHandle, 8, magicValue))
      {
         PrintParamError("magic");
      }
      
      if (!getIntValue(ExpertHandle, 9, expirationValue))
      {
         PrintParamError("expiration");
      }
      
      if (!getIntValue(ExpertHandle, 10, arrow_colorValue))
      {
         PrintParamError("arrow_color");  
      }
      
      if (!sendIntResponse(ExpertHandle, OrderSend(symbolValue, cmdValue, volumeValue, priceValue
                                       , slippageValue, stoplossValue, takeprofitValue
                                       , commentValue, magicValue, expirationValue, arrow_colorValue))) 
      {
         PrintResponseError("OrderSend");
      }         
      
      break;   
            
   case 2: // OrderClose
      if (!getIntValue(ExpertHandle, 0, ticketValue)) 
      {
         PrintParamError("ticket");
      }
      
      if (!getDoubleValue(ExpertHandle, 1, lotsValue))
      {
         PrintParamError("lots");
      }
               
      if (!getDoubleValue(ExpertHandle, 2, priceValue))
      {
         PrintParamError("price");
      }
              
      if (!getIntValue(ExpertHandle, 3, slippageValue))
      {
         PrintParamError("slippage");       
      }
      
      if (!getIntValue(ExpertHandle, 4, colorValue))
      {
         PrintParamError("color");    
      }

      if (!sendBooleanResponse(ExpertHandle, OrderClose(ticketValue, lotsValue, priceValue, slippageValue, colorValue))) 
      {
         PrintResponseError("OrderClose");
      }               

      break;
      
   case 3: // OrderCloseBy
      if (!getIntValue(ExpertHandle, 0, ticketValue)) 
      {
         PrintParamError("ticket");
      }
                       
      if (!getIntValue(ExpertHandle, 1, oppositeValue))
      {
         PrintParamError("opposite");
      }
      
      if (!getIntValue(ExpertHandle, 2, colorValue))
      {
         PrintParamError("color");
      }

      if (!sendBooleanResponse(ExpertHandle, OrderCloseBy(ticketValue, oppositeValue, colorValue))) 
      {
         PrintResponseError("OrderCloseBy");
      }  
                     
      break;
      
   case 4: // OrderClosePrice
      if (!sendDoubleResponse(ExpertHandle, OrderClosePrice())) 
      {
         PrintResponseError("OrderClosePrice");
      }  
                     
      break; 
      
   case 1004: // OrderClosePriceByTicket
      
      result = 0;         
                              
      if (OrderSelectByTicketFromCommand()) 
      {
         result = OrderClosePrice();
      } 
      else
      {
         PrintResponseError("OrderClosePriceByTicket");         
      }
      
      if (!sendDoubleResponse(ExpertHandle, result)) 
      {
         PrintResponseError("OrderClosePriceByTicket");
      }  
                     
      break;       
                   
   case 5: //OrderCloseTime
      if (!sendIntResponse(ExpertHandle, OrderCloseTime())) 
      {
         PrintResponseError("OrderCloseTime");
      }  
                     
      break; 

   case 6: //OrderComment
      if (!sendStringResponse(ExpertHandle, OrderComment())) 
      {
         PrintResponseError("OrderComment");
      }
      
      break;

   case 7: //OrderCommission
      if (!sendDoubleResponse(ExpertHandle, OrderCommission())) 
      {
         PrintResponseError("OrderCommission");
      }
      
      break;
      
   case 8: //OrderDelete      
      if (!getIntValue(ExpertHandle, 0, ticketValue)) 
      {
         PrintParamError("ticket");
      }
      
      if (!getIntValue(ExpertHandle, 1, arrow_colorValue))
      {
         PrintParamError("arrow_color");      
      }
      
      if (!sendDoubleResponse(ExpertHandle, OrderDelete(ticketValue, arrow_colorValue))) 
      {
         PrintResponseError("OrderDelete");
      }
      
      break;         
      
   case 9: //OrderExpiration
      if (!sendIntResponse(ExpertHandle, OrderExpiration())) 
      {
         PrintResponseError("OrderExpiration");
      }  
                     
      break; 
      
   case 10: //OrderLots
      if (!sendDoubleResponse(ExpertHandle, OrderLots())) 
      {
         PrintResponseError("OrderLots");
      }  
                     
      break;         

   case 11: //OrderMagicNumber
      if (!sendIntResponse(ExpertHandle, OrderMagicNumber())) 
      {
         PrintResponseError("OrderMagicNumber");
      }  
                     
      break;     
      
   case 12: //OrderModify
   
      if (!getIntValue(ExpertHandle, 0, ticketValue))
      {
         PrintParamError("ticket");
      }
                        
      if (!getDoubleValue(ExpertHandle, 1, priceValue))
      {
         PrintParamError("price");
      }
      
      if (!getDoubleValue(ExpertHandle, 2, stoplossValue))
      {
         PrintParamError("stoploss");  
      }

      if (!getDoubleValue(ExpertHandle, 3, takeprofitValue))
      {
         PrintParamError("takeprofit");   
      }
              
      if (!getIntValue(ExpertHandle, 4, expirationValue))
      {
         PrintParamError("expiration");   
      }
      
      if (!getIntValue(ExpertHandle, 5, arrow_colorValue))
      {
         PrintParamError("arrow_color");
      }
      
      if (!sendBooleanResponse(ExpertHandle, OrderModify(ticketValue, priceValue, stoplossValue, takeprofitValue, expirationValue, arrow_colorValue))) 
      {
         PrintResponseError("OrderModify");
      }  
                     
      break;       
      
   case 13: //OrderOpenPrice
      if (!sendDoubleResponse(ExpertHandle, OrderOpenPrice())) 
      {
         PrintResponseError("OrderOpenPrice");
      }  
                     
      break;   
      
   case 1013: // OrderOpenPriceByTicket
      
      result = 0;         
                              
      if (OrderSelectByTicketFromCommand()) 
      {
         result = OrderOpenPrice();
      } 
      else
      {
         PrintResponseError("OrderOpenPriceByTicket");         
      }
      
      if (!sendDoubleResponse(ExpertHandle, result)) 
      {
         PrintResponseError("OrderOpenPriceByTicket");
      }  
                     
      break;      
      
      case 14: //OrderOpenTime
      if (!sendIntResponse(ExpertHandle, OrderOpenTime())) 
      {
         PrintResponseError("OrderOpenTime");
      }  
                     
      break;     
      
      case 15: //OrderPrint
      OrderPrint();

      sendVoidResponse(ExpertHandle);
                     
      break;      
      
      case 16: //OrderProfit
      if (!sendDoubleResponse(ExpertHandle, OrderProfit())) 
      {
         PrintResponseError("OrderProfit");
         break;
      }  
                     
      break;     
      
   case 17: //OrderSelect
   
      indexValue = 0;
      selectValue = 0;
      poolValue = 0;
      
      if (!getIntValue(ExpertHandle, 0, indexValue))
      {
         PrintParamError("index");
      }            
                        
      if (!getIntValue(ExpertHandle, 1, selectValue))
      {
         PrintParamError("select");
      }
      
      if (!getIntValue(ExpertHandle, 2, poolValue))
      {
         PrintParamError("pool");
      }
      
      if (!sendBooleanResponse(ExpertHandle, OrderSelect(indexValue, selectValue, poolValue))) 
      {
         PrintResponseError("OrderSelect");
      }  
                     
      break;
      
   case 18: //OrdersHistoryTotal
      if (!sendIntResponse(ExpertHandle, OrdersHistoryTotal())) 
      {
         PrintResponseError("OrdersHistoryTotal");
      }  
   
      break;       
      
   case 19: //OrderStopLoss
      if (!sendDoubleResponse(ExpertHandle, OrderStopLoss())) 
      {
         PrintResponseError("OrderStopLoss");
      }  

      break;    
      
   case 20: //OrdersTotal
      if (!sendIntResponse(ExpertHandle, OrdersTotal())) 
      {
         PrintResponseError("OrdersTotal");
      }  

      break;     

   case 21: //OrderSwap
      if (!sendDoubleResponse(ExpertHandle, OrderSwap())) 
      {
         PrintResponseError("OrderSwap");
      }  

      break;      

   case 22: //OrderSymbol
      if (!sendStringResponse(ExpertHandle, OrderSymbol())) 
      {
         PrintResponseError("OrderSymbol");
      }  

      break;   
      
   case 23: //OrderTakeProfit
      if (!sendDoubleResponse(ExpertHandle, OrderTakeProfit())) 
      {
         PrintResponseError("OrderTakeProfit");
      }  

      break;      
      
   case 24: //OrderTicket
      if (!sendIntResponse(ExpertHandle, OrderTicket())) 
      {
         PrintResponseError("OrderTicket");
      }  

      break;  
      
   case 25: //OrderType
      if (!sendIntResponse(ExpertHandle, OrderType())) 
      {
         PrintResponseError("OrderType");
      } 
      
      break;                                        
      
   case 26: //GetLastError
      if (!sendIntResponse(ExpertHandle, GetLastError())) 
      {
         PrintResponseError("GetLastError");
      } 
      
      break;    

   case 27: //IsConnected
      if (!sendBooleanResponse(ExpertHandle, IsConnected())) 
      {
         PrintResponseError("IsConnected");
      } 
      
      break;   

   case 28: //IsDemo
      if (!sendBooleanResponse(ExpertHandle, IsDemo())) 
      {
         PrintResponseError("IsDemo");
      } 
      
      break;   
      
   case 29: //IsDllsAllowed
      if (!sendBooleanResponse(ExpertHandle, IsDllsAllowed())) 
      {
         PrintResponseError("IsDllsAllowed");
      } 
      
      break;   
      
   case 30: //IsExpertEnabled
      if (!sendBooleanResponse(ExpertHandle, IsExpertEnabled())) 
      {
         PrintResponseError("IsExpertEnabled");
      } 
      
      break;                                               
      
   case 31: //IsLibrariesAllowed
      if (!sendBooleanResponse(ExpertHandle, IsLibrariesAllowed())) 
      {
         PrintResponseError("IsLibrariesAllowed");
      } 
      
      break;   
      
   case 32: //IsOptimization
      if (!sendBooleanResponse(ExpertHandle, IsOptimization())) 
      {
         PrintResponseError("IsOptimization");
      } 
      
      break;   
      
   case 33: //IsStopped      
      if (!sendBooleanResponse(ExpertHandle, IsStopped())) 
      {
         PrintResponseError("IsStopped");
      } 
      
      break;                              

   case 34: //IsTesting
      if (!sendBooleanResponse(ExpertHandle, IsTesting())) 
      {
         PrintResponseError("IsTesting");
      } 
      
      break;            
      
   case 35: //IsTradeAllowed
      if (!sendBooleanResponse(ExpertHandle, IsTradeAllowed())) 
      {
         PrintResponseError("IsTradeAllowed");
      }          
      
      break;            
      
   case 36: //IsTradeContextBusy
      if (!sendBooleanResponse(ExpertHandle, IsTradeContextBusy())) 
      {
         PrintResponseError("IsTradeContextBusy");
      }   
      
      break;  

   case 37: //IsVisualMode
      if (!sendBooleanResponse(ExpertHandle, IsVisualMode())) 
      {
         PrintResponseError("IsVisualMode");
      } 
      
      break;  
      
   case 38: //UninitializeReason
      if (!sendIntResponse(ExpertHandle, UninitializeReason())) 
      {
         PrintResponseError("UninitializeReason");
      } 
      
      break;  
      
   case 39: //ErrorDescription
      if (!getIntValue(ExpertHandle, 0, errorCodeValue)) 
      {
         PrintParamError("errorCode");
      }
   
      if (!sendStringResponse(ExpertHandle, ErrorDescription(errorCodeValue))) 
      {
         PrintResponseError("ErrorDescription");
      } 
      
      break;  
      
   case 40: //AccountBalance
      if (!sendDoubleResponse(ExpertHandle, AccountBalance())) 
      {
         PrintResponseError("AccountBalance");
      }  
      
      break;  
      
   case 41: //AccountCredit
      if (!sendDoubleResponse(ExpertHandle, AccountCredit())) 
      {
         PrintResponseError("AccountCredit");
      }  
      
      break;  
      
   case 42: //AccountCompany
      if (!sendStringResponse(ExpertHandle, AccountCompany())) 
      {
         PrintResponseError("AccountCompany");
      }  
      
      break;  
      
   case 43: //AccountCurrency
      if (!sendStringResponse(ExpertHandle, AccountCurrency())) 
      {
         PrintResponseError("AccountCurrency");
      }  
      
      break;  
      
   case 44: //AccountEquity
      if (!sendDoubleResponse(ExpertHandle, AccountEquity())) 
      {
         PrintResponseError("AccountEquity");
      }  
      
      break;  
      
   case 45: //AccountFreeMargin
      if (!sendDoubleResponse(ExpertHandle, AccountFreeMargin())) 
      {
         PrintResponseError("AccountFreeMargin");
      }  
      
      break;  
      
   case 46: //AccountFreeMarginCheck      
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, cmdValue)) 
      {
         PrintParamError("cmd");
      }
      
      if (!getDoubleValue(ExpertHandle, 2, volumeValue)) 
      {
         PrintParamError("volume");
      }
      
      if (!sendDoubleResponse(ExpertHandle, AccountFreeMarginCheck(symbolValue, cmdValue, volumeValue))) 
      {
         PrintResponseError("AccountFreeMarginCheck");
      }  
      
      break;  
      
   case 47: //AccountFreeMarginMode
      if (!sendDoubleResponse(ExpertHandle, AccountFreeMarginMode())) 
      {
         PrintResponseError("AccountFreeMarginMode");
      }  
      
      break;  
      
   case 48: //AccountLeverage
      if (!sendIntResponse(ExpertHandle, AccountLeverage())) 
      {
         PrintResponseError("AccountLeverage");
      } 
      
      break;  
      
   case 49: //AccountMargin
      if (!sendDoubleResponse(ExpertHandle, AccountMargin())) 
      {
         PrintResponseError("AccountMargin");
      } 
      
      break;                                                                                                                                
      
   case 50: //AccountName
      if (!sendStringResponse(ExpertHandle, AccountName())) 
      {
         PrintResponseError("AccountName");
      } 
      
      break;  
      
   case 51: //AccountNumber
      if (!sendIntResponse(ExpertHandle, AccountNumber())) 
      {
         PrintResponseError("AccountNumber");
      } 
      
      break;                    
      
   case 52: //AccountProfit
      if (!sendDoubleResponse(ExpertHandle, AccountProfit())) 
      {
         PrintResponseError("AccountProfit");
      } 
      
      break;         
      
   case 53: //AccountServer
      if (!sendStringResponse(ExpertHandle, AccountServer())) 
      {
         PrintResponseError("AccountServer");
      } 
      
      break;         

   case 54: //AccountStopoutLevel
      if (!sendIntResponse(ExpertHandle, AccountStopoutLevel())) 
      {
         PrintResponseError("AccountStopoutLevel");
      } 
      
      break;         

   case 55: //AccountStopoutMode
      if (!sendIntResponse(ExpertHandle, AccountStopoutMode())) 
      {
         PrintResponseError("AccountStopoutMode");
      } 
      
      break;         

   case 56: //Alert
      if (!getStringValue(ExpertHandle, 0, msgValue)) 
      {
         PrintParamError("msg");
      }
      
      Alert(msgValue);
      
      break;         

   case 57: //Comment
      if (!getStringValue(ExpertHandle, 0, msgValue)) 
      {
         PrintParamError("msg");
      }
      
      Comment(msgValue);
      
      break;         

   case 58: //GetTickCount
      if (!sendIntResponse(ExpertHandle, GetTickCount())) 
      {
         PrintResponseError("GetTickCount");
      } 
      
      break;         

   case 59: //MarketInfo   
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, typeValue)) 
      {
         PrintParamError("type");
      }
      
      if (!sendDoubleResponse(ExpertHandle, MarketInfo(symbolValue, typeValue))) 
      {
         PrintResponseError("MarketInfo");
      } 
      
      break;         

   case 60: //MessageBox      
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!sendIntResponse(ExpertHandle, MessageBox(symbolValue))) 
      {
         PrintResponseError("MessageBox");
      } 
      
      break;         

   case 61: //MessageBoxA
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getStringValue(ExpertHandle, 1, captionValue)) 
      {
         PrintParamError("caption");
      }
      
      if (!getIntValue(ExpertHandle, 2, flagValue)) 
      {
         PrintParamError("flag");
      }
      
      if (!sendIntResponse(ExpertHandle, MessageBox(symbolValue, captionValue, flagValue))) 
      {
         PrintResponseError("MessageBox");
      } 
      
      break;         

   case 62: //PlaySound
      if (!getStringValue(ExpertHandle, 0, filenameValue)) 
      {
         PrintParamError("filename");
      }
      
      PlaySound(filenameValue);
               
      break;         

   case 63: //Print
      if (!getStringValue(ExpertHandle, 0, msgValue)) 
      {
         PrintParamError("msg");
      }
   
      Print(msgValue);
      
      break;         

   case 64: //SendFTP
      if (!getStringValue(ExpertHandle, 0, filenameValue)) 
      {
         PrintParamError("filename");
      }
      
      if (!sendBooleanResponse(ExpertHandle, SendFTP(filenameValue))) 
      {
         PrintResponseError("SendFTP");
      } 
      
      break;         

   case 65: //SendFTPA
      if (!getStringValue(ExpertHandle, 0, filenameValue)) 
      {
         PrintParamError("filename");
      }
      
      if (!getStringValue(ExpertHandle, 1, ftp_pathValue)) 
      {
         PrintParamError("ftp_path");
      }
      
      if (!sendBooleanResponse(ExpertHandle, SendFTP(filenameValue, ftp_pathValue))) 
      {
         PrintResponseError("SendFTP");
      } 
      
      break;         

   case 66: //SendMail
      if (!getStringValue(ExpertHandle, 0, subjectValue)) 
      {
         PrintParamError("subject");
      }
      
      if (!getStringValue(ExpertHandle, 1, some_textValue)) 
      {
         PrintParamError("some_text");
      }
      
      SendMail(subjectValue, some_textValue);
      
      break;         

   case 67: //Sleep
      if (!getIntValue(ExpertHandle, 0, millisecondsValue)) 
      {
         PrintParamError("milliseconds");
      }
      
      Sleep(millisecondsValue);
      
      break;         

   case 68: //TerminalCompany      
      if (!sendStringResponse(ExpertHandle, TerminalCompany())) 
      {
         PrintResponseError("TerminalCompany");
      } 
   
      break;

   case 69: //TerminalName
      if (!sendStringResponse(ExpertHandle, TerminalName())) 
      {
         PrintResponseError("TerminalName");
      } 

      break;

   case 70: //TerminalPath
      if (!sendStringResponse(ExpertHandle, TerminalPath())) 
      {
         PrintResponseError("TerminalPath");
      } 

      break;
      
   case 71: //Day
      if (!sendIntResponse(ExpertHandle, Day())) 
      {
         PrintResponseError("Day");
      } 
      
      break;
      
   case 72: //DayOfWeek
      if (!sendIntResponse(ExpertHandle, DayOfWeek())) 
      {
         PrintResponseError("DayOfWeek");
      } 
      
      break;

   case 73: //DayOfYear
      if (!sendIntResponse(ExpertHandle, DayOfYear())) 
      {
         PrintResponseError("DayOfYear");
      } 
      
      break;

   case 74: //Hour
      if (!sendIntResponse(ExpertHandle, Hour())) 
      {
         PrintResponseError("Hour");
      } 
      
      break;

   case 75: //Minute
      if (!sendIntResponse(ExpertHandle, Minute())) 
      {
         PrintResponseError("Minute");
      } 
      
      break;

   case 76: //Month
      if (!sendIntResponse(ExpertHandle, Month())) 
      {
         PrintResponseError("Month");
      } 
      
      break;

   case 77: //Seconds
      if (!sendIntResponse(ExpertHandle, Seconds())) 
      {
         PrintResponseError("Seconds");
      } 
      
      break;

   case 78: //TimeCurrent
      if (!sendIntResponse(ExpertHandle, TimeCurrent())) 
      {
         PrintResponseError("TimeCurrent");
      } 
      
      break;

   case 79: //TimeDay
      if (!getIntValue(ExpertHandle, 0, dateValue)) 
      {
         PrintParamError("date");
      }

      if (!sendIntResponse(ExpertHandle, TimeDay(dateValue))) 
      {
         PrintResponseError("TimeDay");
      } 
      
      break;

   case 80: //TimeDayOfWeek
      if (!getIntValue(ExpertHandle, 0, dateValue)) 
      {
         PrintParamError("date");
      }

      if (!sendIntResponse(ExpertHandle, TimeDayOfWeek(dateValue))) 
      {
         PrintResponseError("TimeDayOfWeek");
      } 
      
      break;

   case 81: //TimeDayOfYear
      if (!getIntValue(ExpertHandle, 0, dateValue)) 
      {
         PrintParamError("date");
      }

      if (!sendIntResponse(ExpertHandle, TimeDayOfYear(dateValue))) 
      {
         PrintResponseError("TimeDayOfYear");
      } 
      
      break;

   case 82: //TimeHour
      if (!getIntValue(ExpertHandle, 0, timeValue)) 
      {
         PrintParamError("time");
      }

      if (!sendIntResponse(ExpertHandle, TimeHour(timeValue))) 
      {
         PrintResponseError("TimeHour");
      } 
      
      break;

   case 83: //TimeLocal
      if (!sendIntResponse(ExpertHandle, TimeLocal())) 
      {
         PrintResponseError("TimeLocal");
      } 
      
      break;

   case 84: //TimeMinute
      if (!getIntValue(ExpertHandle, 0, timeValue)) 
      {
         PrintParamError("time");
      }

      if (!sendIntResponse(ExpertHandle, TimeMinute(timeValue))) 
      {
         PrintResponseError("TimeMinute");
      } 
      
      break;

   case 85: //TimeMonth
      if (!getIntValue(ExpertHandle, 0, timeValue)) 
      {
         PrintParamError("time");
      }

      if (!sendIntResponse(ExpertHandle, TimeMonth(timeValue))) 
      {
         PrintResponseError("TimeMonth");
      } 
      
      break;

   case 86: //TimeSeconds
      if (!getIntValue(ExpertHandle, 0, timeValue)) 
      {
         PrintParamError("time");
      }

      if (!sendIntResponse(ExpertHandle, TimeSeconds(timeValue))) 
      {
         PrintResponseError("TimeSeconds");
      } 
      
      break;

   case 87: //TimeYear
      if (!getIntValue(ExpertHandle, 0, timeValue)) 
      {
         PrintParamError("time");
      }

      if (!sendIntResponse(ExpertHandle, TimeYear(timeValue))) 
      {
         PrintResponseError("TimeYear");
      } 
      
      break;
          
   case 88: //Year
      if (!sendIntResponse(ExpertHandle, Year())) 
      {
         PrintResponseError("Year");
      } 
      
      break;
      
   case 89: //GlobalVariableCheck
      if (!getStringValue(ExpertHandle, 0, nameValue)) 
      {
         PrintParamError("name");
      }
      
      if (!sendBooleanResponse(ExpertHandle, GlobalVariableCheck(nameValue))) 
      {
         PrintResponseError("GlobalVariableCheck");
      } 
      
   break;

   case 90: //GlobalVariableDel
      if (!getStringValue(ExpertHandle, 0, nameValue)) 
      {
         PrintParamError("name");
      }
      
      if (!sendBooleanResponse(ExpertHandle, GlobalVariableDel(nameValue))) 
      {
         PrintResponseError("GlobalVariableDel");
      } 
      
   break;

   case 91: //GlobalVariableGet
      if (!getStringValue(ExpertHandle, 0, nameValue)) 
      {
         PrintParamError("name");
      }
      
      if (!sendDoubleResponse(ExpertHandle, GlobalVariableGet(nameValue))) 
      {
         PrintResponseError("GlobalVariableGet");
      } 
      
   break;

   case 92: //GlobalVariableName
      if (!getIntValue(ExpertHandle, 0, indexValue)) 
      {
         PrintParamError("index");
      }
      
      if (!sendStringResponse(ExpertHandle, GlobalVariableName(indexValue))) 
      {
         PrintResponseError("GlobalVariableName");
      } 
      
   break;
   
   case 93: //GlobalVariableSet
      if (!getStringValue(ExpertHandle, 0, nameValue)) 
      {
         PrintParamError("name");
      }
      
      if (!getDoubleValue(ExpertHandle, 1, valueValue)) 
      {
         PrintParamError("value");
      }
      
      if (!sendIntResponse(ExpertHandle, GlobalVariableSet(nameValue, valueValue))) 
      {
         PrintResponseError("GlobalVariableSet");
      } 
      
   break;
   
   case 94: //GlobalVariableSetOnCondition
      if (!getStringValue(ExpertHandle, 0, nameValue)) 
      {
         PrintParamError("name");
      }
      
      if (!getDoubleValue(ExpertHandle, 1, valueValue)) 
      {
         PrintParamError("value");
      }
      
      if (!getDoubleValue(ExpertHandle, 2, check_value)) 
      {
         PrintParamError("check_value");
      }
      
      if (!sendBooleanResponse(ExpertHandle, GlobalVariableSetOnCondition(nameValue, valueValue, check_value))) 
      {
         PrintResponseError("GlobalVariableSetOnCondition");
      } 
      
   break;
   
   case 95: //GlobalVariablesDeleteAll
      if (!getStringValue(ExpertHandle, 0, prefix_nameValue)) 
      {
         PrintParamError("prefix_name");
      }
      
      if (!sendIntResponse(ExpertHandle, GlobalVariablesDeleteAll(prefix_nameValue))) 
      {
         PrintResponseError("GlobalVariablesDeleteAll");
      } 
      
   break;
   
   case 96: //GlobalVariablesTotal
      if (!sendIntResponse(ExpertHandle, GlobalVariablesTotal())) 
      {
         PrintResponseError("GlobalVariablesTotal");
      } 
      
   break;                              
   
   case 97: //iAC
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iAC(symbolValue, timeframeValue, shiftValue))) 
      {
         PrintResponseError("iAC");
      } 
   
   break;

   case 98: //iAD
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iAD(symbolValue, timeframeValue, shiftValue))) 
      {
         PrintResponseError("iAD");
      } 
   
   break;

   case 99: //iAlligator
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iAD(symbolValue, timeframeValue, shiftValue))) 
      {
         PrintResponseError("iAD");
      } 
   
   break;
   
   case 100: //iADX
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, periodValue)) 
      {
         PrintParamError("period");
      }
      
      if (!getIntValue(ExpertHandle, 3, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         

      if (!getIntValue(ExpertHandle, 4, modeValue)) 
      {
         PrintParamError("mode");
      }
      
      if (!getIntValue(ExpertHandle, 5, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iADX(symbolValue, timeframeValue, periodValue, applied_priceValue, modeValue, shiftValue))) 
      {
         PrintResponseError("iADX");
      } 
   
   break;

   case 101: //iATR
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, periodValue)) 
      {
         PrintParamError("period");
      }
      
      if (!getIntValue(ExpertHandle, 3, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iATR(symbolValue, timeframeValue, periodValue, shiftValue))) 
      {
         PrintResponseError("iATR");
      } 
   
   break;

   case 102: //iAO
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iAO(symbolValue, timeframeValue, shiftValue))) 
      {
         PrintResponseError("iAO");
      }      
   break;

   case 103: //iBearsPower
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, periodValue)) 
      {
         PrintParamError("period");
      }
      
      if (!getIntValue(ExpertHandle, 3, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         
      
      if (!getIntValue(ExpertHandle, 4, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iBearsPower(symbolValue, timeframeValue, periodValue, applied_priceValue, shiftValue))) 
      {
         PrintResponseError("iBearsPower");
      }      
   
   break;

   case 104: //iBands
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, periodValue)) 
      {
         PrintParamError("period");
      }

      if (!getIntValue(ExpertHandle, 3, deviationValue)) 
      {
         PrintParamError("deviation");
      }
      
      if (!getIntValue(ExpertHandle, 4, bands_shiftValue)) 
      {
         PrintParamError("bands_shift");
      }         
      
      if (!getIntValue(ExpertHandle, 5, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         

      if (!getIntValue(ExpertHandle, 6, modeValue)) 
      {
         PrintParamError("mode");
      }         
      
      if (!getIntValue(ExpertHandle, 7, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iBands(symbolValue, timeframeValue, periodValue, deviationValue, bands_shiftValue, applied_priceValue, modeValue, shiftValue))) 
      {
         PrintResponseError("iBands");
      }      
   break;
   
   case 105: //iBandsOnArray
   {         
      paramIndex = 0;
      arraySize = 0;      
      
      getIntValue(ExpertHandle, paramIndex, countValue);
      paramIndex++;
      
      arraySize = countValue;      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue;         
      }                  
      
      getIntValue(ExpertHandle, paramIndex, totalValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, periodValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, deviationValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, bands_shiftValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, modeValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, shiftValue);
      paramIndex++;    
      
      if (!sendDoubleResponse(ExpertHandle, iBandsOnArray(doubleValuesArray, totalValue, periodValue, deviationValue, bands_shiftValue, modeValue, shiftValue))) 
      {
         PrintResponseError("iBandsOnArray");
      }      
   }
   break;

   case 106: //iBullsPower
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, periodValue)) 
      {
         PrintParamError("period");
      }
      
      if (!getIntValue(ExpertHandle, 3, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         
      
      if (!getIntValue(ExpertHandle, 4, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iBullsPower(symbolValue, timeframeValue, periodValue, applied_priceValue, shiftValue))) 
      {
         PrintResponseError("iBullsPower");
      }      
   
   break;

   case 107: //iCCI
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, periodValue)) 
      {
         PrintParamError("period");
      }
      
      if (!getIntValue(ExpertHandle, 3, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         
      
      if (!getIntValue(ExpertHandle, 4, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iCCI(symbolValue, timeframeValue, periodValue, applied_priceValue, shiftValue))) 
      {
         PrintResponseError("iCCI");
      }      
            
   break;

   case 108: //iCCIOnArray
   {         
      paramIndex = 0;
      arraySize = 0;
      
      getIntValue(ExpertHandle, paramIndex, countValue);
      paramIndex++;
      
      arraySize = countValue;      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue;         
      }                  
      
      getIntValue(ExpertHandle, paramIndex, totalValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, periodValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, shiftValue);
      paramIndex++;    
      
      if (!sendDoubleResponse(ExpertHandle, iCCIOnArray(doubleValuesArray, totalValue, periodValue, shiftValue))) 
      {
         PrintResponseError("iCCIOnArray");
      }      
   }
   break;
   
   case 109: //iCustom (int list parameters)
   {         
      paramIndex = 0;
      arraySize = 0;
      
      getStringValue(ExpertHandle, paramIndex, symbolValue);
      paramIndex++;
      
      getIntValue(ExpertHandle, paramIndex, timeframeValue);
      paramIndex++;

      getStringValue(ExpertHandle, paramIndex, nameValue);
      paramIndex++;
      
      getIntValue(ExpertHandle, paramIndex, countValue);
      paramIndex++;
      
      arraySize = countValue;      
      ArrayResize(intValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getIntValue(ExpertHandle, paramIndex, tempIntValue);
         paramIndex++;
         intValuesArray[index] = tempIntValue;         
      }                  
      
      getIntValue(ExpertHandle, paramIndex, modeValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, shiftValue);
      paramIndex++;    
      
      switch(arraySize)
      {
         case 0:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue, totalValue, periodValue, shiftValue));
            break;
         case 1:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , intValuesArray[0]
               , totalValue, periodValue, shiftValue));
               break;
         case 2:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , intValuesArray[0]
               , intValuesArray[1]
               , totalValue, periodValue, shiftValue));
               break;            
         case 3:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , intValuesArray[0]
               , intValuesArray[1]
               , intValuesArray[2]
               , totalValue, periodValue, shiftValue));
               break;                    
         case 4:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , intValuesArray[0]
               , intValuesArray[1]
               , intValuesArray[2]
               , intValuesArray[3]
               , totalValue, periodValue, shiftValue));
               break;   
         case 5:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , intValuesArray[0]
               , intValuesArray[1]
               , intValuesArray[2]
               , intValuesArray[3]
               , intValuesArray[4]
               , totalValue, periodValue, shiftValue));
               break;        
         case 6:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , intValuesArray[0]
               , intValuesArray[1]
               , intValuesArray[2]
               , intValuesArray[3]
               , intValuesArray[4]
               , intValuesArray[5]
               , totalValue, periodValue, shiftValue));
               break;                                        
         case 7:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , intValuesArray[0]
               , intValuesArray[1]
               , intValuesArray[2]
               , intValuesArray[3]
               , intValuesArray[4]
               , intValuesArray[5]
               , intValuesArray[6]
               , totalValue, periodValue, shiftValue));
               break;      
         case 8:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , intValuesArray[0]
               , intValuesArray[1]
               , intValuesArray[2]
               , intValuesArray[3]
               , intValuesArray[4]
               , intValuesArray[5]
               , intValuesArray[6]
               , intValuesArray[7]
               , totalValue, periodValue, shiftValue));
               break;                             
         case 9:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , intValuesArray[0]
               , intValuesArray[1]
               , intValuesArray[2]
               , intValuesArray[3]
               , intValuesArray[4]
               , intValuesArray[5]
               , intValuesArray[6]
               , intValuesArray[7]
               , intValuesArray[8]
               , totalValue, periodValue, shiftValue));
               break;                     
         case 10:
         default:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
            , intValuesArray[0]
            , intValuesArray[1]
            , intValuesArray[2]
            , intValuesArray[3]
            , intValuesArray[4]
            , intValuesArray[5]
            , intValuesArray[6]
            , intValuesArray[7]
            , intValuesArray[8]
            , intValuesArray[9]
            , totalValue, periodValue, shiftValue));
      }
   }
   break;

   case 10109: //iCustom (double list parameters)
   {         
      paramIndex = 0;
      arraySize = 0;
      
      getStringValue(ExpertHandle, paramIndex, symbolValue);
      paramIndex++;
      
      getIntValue(ExpertHandle, paramIndex, timeframeValue);
      paramIndex++;

      getStringValue(ExpertHandle, paramIndex, nameValue);
      paramIndex++;
      
      getIntValue(ExpertHandle, paramIndex, countValue);
      paramIndex++;
      
      arraySize = countValue;      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue;         
      }                  
      
      getIntValue(ExpertHandle, paramIndex, modeValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, shiftValue);
      paramIndex++;    
      
      switch(arraySize)
      {
         case 0:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue, totalValue, periodValue, shiftValue));
            break;
         case 1:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , doubleValuesArray[0]
               , totalValue, periodValue, shiftValue));
               break;
         case 2:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , totalValue, periodValue, shiftValue));
               break;            
         case 3:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , doubleValuesArray[2]
               , totalValue, periodValue, shiftValue));
               break;                    
         case 4:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , doubleValuesArray[2]
               , doubleValuesArray[3]
               , totalValue, periodValue, shiftValue));
               break;   
         case 5:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , doubleValuesArray[2]
               , doubleValuesArray[3]
               , doubleValuesArray[4]
               , totalValue, periodValue, shiftValue));
               break;        
         case 6:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , doubleValuesArray[2]
               , doubleValuesArray[3]
               , doubleValuesArray[4]
               , doubleValuesArray[5]
               , totalValue, periodValue, shiftValue));
               break;                                        
         case 7:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , doubleValuesArray[2]
               , doubleValuesArray[3]
               , doubleValuesArray[4]
               , doubleValuesArray[5]
               , doubleValuesArray[6]
               , totalValue, periodValue, shiftValue));
               break;      
         case 8:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , doubleValuesArray[2]
               , doubleValuesArray[3]
               , doubleValuesArray[4]
               , doubleValuesArray[5]
               , doubleValuesArray[6]
               , doubleValuesArray[7]
               , totalValue, periodValue, shiftValue));
               break;                             
         case 9:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , doubleValuesArray[2]
               , doubleValuesArray[3]
               , doubleValuesArray[4]
               , doubleValuesArray[5]
               , doubleValuesArray[6]
               , doubleValuesArray[7]
               , doubleValuesArray[8]
               , totalValue, periodValue, shiftValue));
               break;                     
         case 10:
         default:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue, timeframeValue, nameValue
            , doubleValuesArray[0]
            , doubleValuesArray[1]
            , doubleValuesArray[2]
            , doubleValuesArray[3]
            , doubleValuesArray[4]
            , doubleValuesArray[5]
            , doubleValuesArray[6]
            , doubleValuesArray[7]
            , doubleValuesArray[8]
            , doubleValuesArray[9]
            , totalValue, periodValue, shiftValue));
      }
   }
   break;

   case 110: //iDeMarker
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, periodValue)) 
      {
         PrintParamError("period");
      }
              
      if (!getIntValue(ExpertHandle, 3, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iDeMarker(symbolValue, timeframeValue, periodValue, shiftValue))) 
      {
         PrintResponseError("iDeMarker");
      }      
                  
   break;

   case 111: //iEnvelopes
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, ma_periodValue)) 
      {
         PrintParamError("ma_period");
      }
      
      if (!getIntValue(ExpertHandle, 3, ma_methodValue)) 
      {
         PrintParamError("ma_method");
      }         

      if (!getIntValue(ExpertHandle, 4, ma_shiftValue)) 
      {
         PrintParamError("ma_shift");
      }         

      if (!getIntValue(ExpertHandle, 5, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         

      if (!getDoubleValue(ExpertHandle, 6, deviationDoubleValue)) 
      {
         PrintParamError("deviation");
      }         

      if (!getIntValue(ExpertHandle, 7, modeValue)) 
      {
         PrintParamError("mode");
      }         
              
      if (!getIntValue(ExpertHandle, 8, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iEnvelopes(symbolValue, timeframeValue, ma_periodValue, ma_methodValue,
                                                         ma_shiftValue, applied_priceValue, deviationDoubleValue, modeValue,
                                                         shiftValue))) 
      {
         PrintResponseError("iEnvelopes");
      }      
   
   break;
   
   case 112: //iEnvelopesOnArray
   {         
      paramIndex = 0;
      arraySize = 0;
      
      getIntValue(ExpertHandle, paramIndex, countValue);
      paramIndex++;
      
      arraySize = countValue;      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue;         
      }                  
      
      getIntValue(ExpertHandle, paramIndex, totalValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, ma_periodValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, ma_methodValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, ma_shiftValue);
      paramIndex++;    
      getDoubleValue(ExpertHandle, paramIndex, deviationDoubleValue);
      paramIndex++;    
      getIntValue(ExpertHandle, paramIndex, modeValue);
      paramIndex++;    
      getIntValue(ExpertHandle, paramIndex, shiftValue);
      paramIndex++;    
      
      if (!sendDoubleResponse(ExpertHandle, iEnvelopesOnArray(doubleValuesArray, totalValue, ma_periodValue, ma_methodValue, ma_shiftValue,  deviationDoubleValue, modeValue, shiftValue))) 
      {
         PrintResponseError("iEnvelopesOnArray");
      }      
   }
   break;

   case 113: //iForce
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, periodValue)) 
      {
         PrintParamError("period");
      }
      
      if (!getIntValue(ExpertHandle, 3, ma_methodValue)) 
      {
         PrintParamError("ma_method");
      }         

      if (!getIntValue(ExpertHandle, 4, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         

      if (!getIntValue(ExpertHandle, 5, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iForce(symbolValue, timeframeValue, periodValue, ma_methodValue,
                                                         applied_priceValue, shiftValue))) 
      {
         PrintResponseError("iForce");
      }      
         
   break;

   case 114: //iFractals
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, modeValue)) 
      {
         PrintParamError("mode");
      }         

      if (!getIntValue(ExpertHandle, 3, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iFractals(symbolValue, timeframeValue, modeValue, shiftValue))) 
      {
         PrintResponseError("iFractals");
      }      
   
   break;

   case 115: //iGator
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2,  jaw_periodValue)) 
      {
         PrintParamError("jaw_period");
      }         

      if (!getIntValue(ExpertHandle, 3, jaw_shiftValue)) 
      {
         PrintParamError("jaw_shift");
      }         

      if (!getIntValue(ExpertHandle, 4, teeth_periodValue)) 
      {
         PrintParamError("teeth_period");
      }         

      if (!getIntValue(ExpertHandle, 5, teeth_shiftValue)) 
      {
         PrintParamError("teeth_shift");
      }         

      if (!getIntValue(ExpertHandle, 6, lips_periodValue)) 
      {
         PrintParamError("lips_period");
      }         

      if (!getIntValue(ExpertHandle, 7, lips_shiftValue)) 
      {
         PrintParamError("lips_shift");
      }         

      if (!getIntValue(ExpertHandle, 8, ma_methodValue)) 
      {
         PrintParamError("ma_method");
      }         

      if (!getIntValue(ExpertHandle, 9, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         

      if (!getIntValue(ExpertHandle, 10, modeValue)) 
      {
         PrintParamError("mode");
      }         

      if (!getIntValue(ExpertHandle, 11, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iGator(symbolValue, timeframeValue, jaw_periodValue, jaw_shiftValue
                                                      , teeth_periodValue, teeth_shiftValue, lips_periodValue, lips_shiftValue
                                                      , ma_methodValue, applied_priceValue, modeValue, shiftValue))) 
      {
         PrintResponseError("iGator");
      }      
   
   break;

   case 116: //iIchimoku
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2,  tenkan_senValue)) 
      {
         PrintParamError("tenkan_sen");
      }         

      if (!getIntValue(ExpertHandle, 3, kijun_senValue)) 
      {
         PrintParamError("kijun_sen");
      }         

      if (!getIntValue(ExpertHandle, 4, senkou_span_bValue)) 
      {
         PrintParamError("senkou_span_b");
      }         

      if (!getIntValue(ExpertHandle, 5, modeValue)) 
      {
         PrintParamError("mode");
      }         

      if (!getIntValue(ExpertHandle, 6, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iIchimoku(symbolValue, timeframeValue, tenkan_senValue, kijun_senValue
                                                      , senkou_span_bValue, modeValue, shiftValue))) 
      {
         PrintResponseError("iIchimoku");
      }      
   
   break;

   case 117: //iBWMFI
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iBWMFI(symbolValue, timeframeValue, shiftValue))) 
      {
         PrintResponseError("iBWMFI");
      }      
   
   break;

   case 118: //iMomentum
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, periodValue)) 
      {
         PrintParamError("period");
      }         
      
      if (!getIntValue(ExpertHandle, 3, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         

      if (!getIntValue(ExpertHandle, 4, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iMomentum(symbolValue, timeframeValue, periodValue, applied_priceValue
                                                         , shiftValue))) 
      {
         PrintResponseError("iMomentum");
      }            
   break;
   
   case 119: //iMomentumOnArray
   {         
      paramIndex = 0;
      arraySize = 0;
      
      getIntValue(ExpertHandle, paramIndex, countValue);
      paramIndex++;
      
      arraySize = countValue;      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue;         
      }                  
      
      getIntValue(ExpertHandle, paramIndex, totalValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, periodValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, shiftValue);
      paramIndex++;    
      
      if (!sendDoubleResponse(ExpertHandle, iMomentumOnArray(doubleValuesArray, totalValue, periodValue, shiftValue))) 
      {
         PrintResponseError("iMomentumOnArray");
      }      
   }
   break;

   case 120: //iMFI
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, periodValue)) 
      {
         PrintParamError("period");
      }         
      
      if (!getIntValue(ExpertHandle, 3, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iMFI(symbolValue, timeframeValue, periodValue, shiftValue))) 
      {
         PrintResponseError("iMFI");
      }            
   
   break;

   case 121: //iMA
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, periodValue)) 
      {
         PrintParamError("period");
      }         
      
      if (!getIntValue(ExpertHandle, 3, ma_shiftValue)) 
      {
         PrintParamError("ma_shift");
      }         

      if (!getIntValue(ExpertHandle, 4, ma_methodValue)) 
      {
         PrintParamError("ma_method");
      }         

      if (!getIntValue(ExpertHandle, 5, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         
      
      if (!getIntValue(ExpertHandle, 6, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iMA(symbolValue, timeframeValue, periodValue, ma_shiftValue
                                                   , ma_methodValue, applied_priceValue, shiftValue))) 
      {
         PrintResponseError("iMA");
      }            
   
   break;
   
   case 122: //iMAOnArray
   {         
      paramIndex = 0;
      arraySize = 0;
      
      getIntValue(ExpertHandle, paramIndex, countValue);
      paramIndex++;
      
      arraySize = countValue;      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue;         
      }                  
      
      getIntValue(ExpertHandle, paramIndex, totalValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, periodValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, ma_shiftValue);
      paramIndex++;    
      getIntValue(ExpertHandle, paramIndex, ma_methodValue);
      paramIndex++;    
      getIntValue(ExpertHandle, paramIndex, shiftValue);
      paramIndex++;    
      
      if (!sendDoubleResponse(ExpertHandle, iMAOnArray(doubleValuesArray, totalValue, periodValue, ma_shiftValue, ma_methodValue, shiftValue))) 
      {
         PrintResponseError("iMAOnArray");
      }      
   }
   break;


   case 123: //iOsMA
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, fast_ema_periodValue)) 
      {
         PrintParamError("fast_ema_period");
      }         
      
      if (!getIntValue(ExpertHandle, 3, slow_ema_periodValue)) 
      {
         PrintParamError("slow_ema_period");
      }         

      if (!getIntValue(ExpertHandle, 4, signal_periodValue)) 
      {
         PrintParamError("signal_period");
      }         

      if (!getIntValue(ExpertHandle, 5, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         
      
      if (!getIntValue(ExpertHandle, 6, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iOsMA(symbolValue, timeframeValue, fast_ema_periodValue, slow_ema_periodValue
                                                   , signal_periodValue, applied_priceValue, shiftValue))) 
      {
         PrintResponseError("iOsMA");
      }            
   
   break;

   case 124: //iMACD
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, fast_ema_periodValue)) 
      {
         PrintParamError("fast_ema_period");
      }         
      
      if (!getIntValue(ExpertHandle, 3, slow_ema_periodValue)) 
      {
         PrintParamError("slow_ema_period");
      }         

      if (!getIntValue(ExpertHandle, 4, signal_periodValue)) 
      {
         PrintParamError("signal_period");
      }         

      if (!getIntValue(ExpertHandle, 5, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         

      if (!getIntValue(ExpertHandle, 6, modeValue)) 
      {
         PrintParamError("mode");
      }         
      
      if (!getIntValue(ExpertHandle, 7, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iMACD(symbolValue, timeframeValue, fast_ema_periodValue, slow_ema_periodValue
                                                   , signal_periodValue, applied_priceValue, modeValue, shiftValue))) 
      {
         PrintResponseError("iMACD");
      }        
            
   break;

   case 125: //iOBV
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         
      
      if (!getIntValue(ExpertHandle, 3, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iOBV(symbolValue, timeframeValue, applied_priceValue, shiftValue))) 
      {
         PrintResponseError("iOBV");
      }        
   
   break;

   case 126: //iSAR
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getDoubleValue(ExpertHandle, 2, stepValue)) 
      {
         PrintParamError("step");
      }         

      if (!getDoubleValue(ExpertHandle, 3, maximumValue)) 
      {
         PrintParamError("maximum");
      }         
      
      if (!getIntValue(ExpertHandle, 4, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iSAR(symbolValue, timeframeValue, stepValue, maximumValue, shiftValue))) 
      {
         PrintResponseError("iSAR");
      }        
   
   break;

   case 127: //iRSI
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, periodValue)) 
      {
         PrintParamError("period");
      }         

      if (!getIntValue(ExpertHandle, 3, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         
      
      if (!getIntValue(ExpertHandle, 4, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iRSI(symbolValue, timeframeValue, periodValue, applied_priceValue, shiftValue))) 
      {
         PrintResponseError("iRSI");
      }        
   
   break;
   
   case 128: //iRSIOnArray
   {         
      paramIndex = 0;
      arraySize = 0;
      
      getIntValue(ExpertHandle, paramIndex, countValue);
      paramIndex++;
      
      arraySize = countValue;      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue;         
      }                  
      
      getIntValue(ExpertHandle, paramIndex, totalValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, periodValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, shiftValue);
      paramIndex++;    
      
      if (!sendDoubleResponse(ExpertHandle, iRSIOnArray(doubleValuesArray, totalValue, periodValue, shiftValue))) 
      {
         PrintResponseError("iRSIOnArray");
      }      
   }
   break;
   
   case 129: //iRVI
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, periodValue)) 
      {
         PrintParamError("period");
      }         

      if (!getIntValue(ExpertHandle, 3, modeValue)) 
      {
         PrintParamError("mode");
      }         
      
      if (!getIntValue(ExpertHandle, 4, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iRVI(symbolValue, timeframeValue, periodValue, modeValue, shiftValue))) 
      {
         PrintResponseError("iRVI");
      }        
   
   break;

   case 130: //iStdDev
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, ma_periodValue)) 
      {
         PrintParamError("ma_period");
      }
      
      if (!getIntValue(ExpertHandle, 3, ma_shiftValue)) 
      {
         PrintParamError("ma_shift");
      }         

      if (!getIntValue(ExpertHandle, 4, ma_methodValue)) 
      {
         PrintParamError("ma_method");
      }         

      if (!getIntValue(ExpertHandle, 5, applied_priceValue)) 
      {
         PrintParamError("applied_price");
      }         

      if (!getIntValue(ExpertHandle, 6, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iStdDev(symbolValue, timeframeValue, ma_periodValue, ma_shiftValue
                                                      , ma_methodValue, applied_priceValue, shiftValue))) 
      {
         PrintResponseError("iStdDev");
      }      
   
   break;
   
   case 131: //iStdDevOnArray
   {         
      paramIndex = 0;
      arraySize = 0;
      
      getIntValue(ExpertHandle, paramIndex, countValue);
      paramIndex++;
      
      arraySize = countValue;      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue;         
      }                  
      
      getIntValue(ExpertHandle, paramIndex, totalValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, ma_periodValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, ma_shiftValue);
      paramIndex++;    
      getIntValue(ExpertHandle, paramIndex, ma_methodValue);
      paramIndex++;    
      getIntValue(ExpertHandle, paramIndex, shiftValue);
      paramIndex++;    
      
      if (!sendDoubleResponse(ExpertHandle, iStdDevOnArray(doubleValuesArray, totalValue, ma_periodValue, ma_shiftValue, ma_methodValue, shiftValue))) 
      {
         PrintResponseError("iStdDevOnArray");
      }      
   }
   break;

   case 132: //iStochastic
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, KperiodValue)) 
      {
         PrintParamError("%Kperiod");
      }
      
      if (!getIntValue(ExpertHandle, 3, DperiodValue)) 
      {
         PrintParamError("%Dperiod");
      }         

      if (!getIntValue(ExpertHandle, 4, slowingValue)) 
      {
         PrintParamError("slowing");
      }         

      if (!getIntValue(ExpertHandle, 5, methodValue)) 
      {
         PrintParamError("method");
      }         

      if (!getIntValue(ExpertHandle, 6, price_fieldValue)) 
      {
         PrintParamError("price_field");
      }         

      if (!getIntValue(ExpertHandle, 7, modeValue)) 
      {
         PrintParamError("mode");
      }         

      if (!getIntValue(ExpertHandle, 8, shiftValue)) 
      {
         PrintParamError("shift");
      }         
      
      if (!sendDoubleResponse(ExpertHandle, iStochastic(symbolValue, timeframeValue, KperiodValue, DperiodValue
                                                      , slowingValue, methodValue, price_fieldValue, modeValue, shiftValue))) 
      {
         PrintResponseError("iStochastic");
      }      
   
   break;

   case 133: //iWPR
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }

      if (!getIntValue(ExpertHandle, 2, periodValue)) 
      {
         PrintParamError("period");
      }
      
      if (!getIntValue(ExpertHandle, 3, shiftValue)) 
      {
         PrintParamError("shift");
      }         

      if (!sendDoubleResponse(ExpertHandle, iWPR(symbolValue, timeframeValue, periodValue, shiftValue))) 
      {
         PrintResponseError("iWPR");
      }      
   
   break;
   
      case 134: //iBars
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!sendIntResponse(ExpertHandle, iBars(symbolValue, timeframeValue))) 
      {
         PrintResponseError("iBars");
      }           
   break;

   case 135: //iBarShift
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, timeValue)) 
      {
         PrintParamError("time");
      }

      if (!getIntValue(ExpertHandle, 3, exactValue)) 
      {
         PrintParamError("exact");
      }
      
      if (!sendIntResponse(ExpertHandle, iBarShift(symbolValue, timeframeValue, timeValue, exactValue))) 
      {
         PrintResponseError("iBarShift");
      }        
   break;

   case 136: //iClose
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, shiftValue)) 
      {
         PrintParamError("shift");
      }
      
      if (!sendDoubleResponse(ExpertHandle, iClose(symbolValue, timeframeValue, shiftValue))) 
      {
         PrintResponseError("iClose");
      }
   break;

   case 137: //iHigh
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, shiftValue)) 
      {
         PrintParamError("shift");
      }
      
      if (!sendDoubleResponse(ExpertHandle, iHigh(symbolValue, timeframeValue, shiftValue))) 
      {
         PrintResponseError("iHigh");
      }
   break;

   case 138: //iHighest
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, typeValue)) 
      {
         PrintParamError("type");
      }
      
      if (!getIntValue(ExpertHandle, 3, countValue)) 
      {
         PrintParamError("count");
      }

      if (!getIntValue(ExpertHandle, 4, startValue)) 
      {
         PrintParamError("count");
      }
      
      if (!sendIntResponse(ExpertHandle, iHighest(symbolValue, timeframeValue, typeValue, countValue, startValue))) 
      {
         PrintResponseError("iHighest");
      }
   break;

   case 139: //iLow
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, shiftValue)) 
      {
         PrintParamError("shift");
      }
      
      if (!sendDoubleResponse(ExpertHandle, iLow(symbolValue, timeframeValue, shiftValue))) 
      {
         PrintResponseError("iLow");
      }
   break;
   
   case 140: //iLowest
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, typeValue)) 
      {
         PrintParamError("type");
      }
      
      if (!getIntValue(ExpertHandle, 3, countValue)) 
      {
         PrintParamError("count");
      }

      if (!getIntValue(ExpertHandle, 4, startValue)) 
      {
         PrintParamError("count");
      }
      
      if (!sendIntResponse(ExpertHandle, iLowest(symbolValue, timeframeValue, typeValue, countValue, startValue))) 
      {
         PrintResponseError("iLowest");
      }
   break;

   case 141: //iOpen
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, shiftValue)) 
      {
         PrintParamError("shift");
      }
      
      if (!sendDoubleResponse(ExpertHandle, iOpen(symbolValue, timeframeValue, shiftValue))) 
      {
         PrintResponseError("iOpen");
      }

   break;

   case 142: //iTime
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, shiftValue)) 
      {
         PrintParamError("shift");
      }

      if (!sendIntResponse(ExpertHandle, iTime(symbolValue, timeframeValue, shiftValue))) 
      {
         PrintResponseError("iTime");
      }
   break;

   case 143: //iVolume
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
      
      if (!getIntValue(ExpertHandle, 2, shiftValue)) 
      {
         PrintParamError("shift");
      }
      
      if (!sendDoubleResponse(ExpertHandle, iVolume(symbolValue, timeframeValue, shiftValue))) 
      {
         PrintResponseError("iVolume");
      }
   break;

   case 144: //iCloseArray
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
            
      barsCount = iBars(symbolValue, timeframeValue);
      priceCount = ArrayResize(priceArray, barsCount);
      
      for(index = 0; index < priceCount; index++)
      {
         priceArray[index] = iClose(symbolValue, timeframeValue, index);
      }
                          
      if (!sendDoubleArrayResponse(ExpertHandle, priceArray, priceCount)) 
      {
         PrintResponseError("iCloseArray");
      }
   break;

   case 145: //iHighArray
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
            
      barsCount = iBars(symbolValue, timeframeValue);
      priceCount = ArrayResize(priceArray, barsCount);
      
      for(index = 0; index < priceCount; index++)
      {
         priceArray[index] = iHigh(symbolValue, timeframeValue, index);
      }
                          
      if (!sendDoubleArrayResponse(ExpertHandle, priceArray, priceCount)) 
      {
         PrintResponseError("iHighArray");
      }
   break;

   case 146: //iLowArray
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
            
      barsCount = iBars(symbolValue, timeframeValue);
      priceCount = ArrayResize(priceArray, barsCount);
      
      for(index = 0; index < priceCount; index++)
      {
         priceArray[index] = iLow(symbolValue, timeframeValue, index);
      }
                          
      if (!sendDoubleArrayResponse(ExpertHandle, priceArray, priceCount)) 
      {
         PrintResponseError("iLowArray");
      }
   break;

   case 147: //iOpenArray
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
            
      barsCount = iBars(symbolValue, timeframeValue);
      priceCount = ArrayResize(priceArray, barsCount);
      
      for(index = 0; index < priceCount; index++)
      {
         priceArray[index] = iOpen(symbolValue, timeframeValue, index);
      }
                          
      if (!sendDoubleArrayResponse(ExpertHandle, priceArray, priceCount)) 
      {
         PrintResponseError("iOpenArray");
      }
   break;

   case 148: //iVolumeArray
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
            
      barsCount = iBars(symbolValue, timeframeValue);
      volumeCount = ArrayResize(volumeArray, barsCount);
      
      for(index = 0; index < volumeCount; index++)
      {
         volumeArray[index] = iVolume(symbolValue, timeframeValue, index);
      }
                          
      if (!sendDoubleArrayResponse(ExpertHandle, volumeArray, volumeCount)) 
      {
         PrintResponseError("iVolumeArray");
      }
   break;

   case 149: //iTimeArray
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!getIntValue(ExpertHandle, 1, timeframeValue)) 
      {
         PrintParamError("timeframe");
      }
            
      barsCount = iBars(symbolValue, timeframeValue);
      timeCount = ArrayResize(timeArray, barsCount);
      
      for(index = 0; index < timeCount; index++)
      {
         timeArray[index] = iTime(symbolValue, timeframeValue, index);
      }
                          
      if (!sendIntArrayResponse(ExpertHandle, timeArray, timeCount)) 
      {
         PrintResponseError("iTimeArray");
      }
   break;
   
   case 150: //RefreshRates
      if (!sendBooleanResponse(ExpertHandle, RefreshRates())) 
      {
         PrintResponseError("RefreshRates");
      }    
   break;
   
   case 151: //OrderCloseAll
      if (!sendBooleanResponse(ExpertHandle, OrderCloseAll())) 
      {
         PrintResponseError("OrderCloseAll");
      }    
   break;   

   default:
      Print("Unknown command type = ", commandType);
      sendVoidResponse(ExpertHandle);      
      break;
   }   
   
   return (commandType);
}

bool OrderSelectByTicketFromCommand()
{
   bool selected = false;
   
   ticketValue = 0;
         
   if (!getIntValue(ExpertHandle, 0, ticketValue))
   {
      PrintParamError("ticket");
      return (false);
   }   
   
   selected = OrderSelect(ticketValue, SELECT_BY_TICKET);
   
   return (selected);
}

void PrintParamError(string paramName)
{
   Print("[ERROR] parameter: ", paramName);
}

void PrintResponseError(string commandName)
{
   Print("[ERROR] response: ", commandName);
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