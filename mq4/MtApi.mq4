#property copyright "Vyacheslav Demidyuk"
#property link      "DW"

#include <WinUser32.mqh>
#include <stdlib.mqh>

#import "MTConnector.dll"
   bool initExpert(int expertHandle, string connectionProfile, string symbol, double bid, double ask, string& err[]);
   bool deinitExpert(int expertHandle, string& err[]);   
   bool updateQuote(int expertHandle, string symbol, double bid, double ask, string& err[]);   
   
   bool sendIntResponse(int expertHandle, int response);
   bool sendBooleanResponse(int expertHandle, int response);
   bool sendDoubleResponse(int expertHandle, double response);
   bool sendStringResponse(int expertHandle, string response);
   bool sendVoidResponse(int expertHandle);
   bool sendDoubleArrayResponse(int expertHandle, double& values[], int size);
   bool sendIntArrayResponse(int expertHandle, int& values[], int size);   
   
   bool getCommandType(int expertHandle, int& res[]);
   bool getIntValue(int expertHandle, int paramIndex, int& res[]);
   bool getDoubleValue(int expertHandle, int paramIndex, double& res[]);
   bool getStringValue(int expertHandle, int paramIndex, string& res[]);
#import

extern string ConnectionProfile = "Local";

int ExpertHandle;

string message[1];
bool isCrashed = FALSE;

string symbolValue[1];
string commentValue[1];
string msgValue[1];
string captionValue[1];
string filenameValue[1];
string ftp_pathValue[1];
string subjectValue[1];
string some_textValue[1];
string nameValue[1];
string prefix_nameValue[1];

int barsCount;
int priceCount;
int volumeCount;
int timeCount;
int index;
int paramIndex;
int arraySize;

int pCommandType[1];
int cmdValue[1];
int slippageValue[1];
int ticketValue[1];
int oppositeValue[1];
int magicValue[1];
int expirationValue[1];
int arrow_colorValue[1];
int colorValue[1];
int indexValue[1];
int selectValue[1];
int poolValue[1];
int errorCodeValue[1];
int typeValue[1];
int flagValue[1];
int millisecondsValue[1];
int dateValue[1];
int timeValue[1];
int timeframeValue[1];
int shiftValue[1];
int periodValue[1];
int applied_priceValue[1];
int modeValue[1];
int deviationValue[1];
int bands_shiftValue[1];
int ma_periodValue[1];
int ma_methodValue[1];
int ma_shiftValue[1];
int jaw_periodValue[1];
int jaw_shiftValue[1];
int teeth_periodValue[1];
int teeth_shiftValue[1];
int lips_periodValue[1];
int lips_shiftValue[1];
int tenkan_senValue[1];
int kijun_senValue[1];
int senkou_span_bValue[1];
int fast_ema_periodValue[1];
int slow_ema_periodValue[1];
int signal_periodValue[1];
int KperiodValue[1];
int DperiodValue[1];
int slowingValue[1];
int methodValue[1];
int price_fieldValue[1];
int exactValue[1];
int startValue[1];
int countValue[1];
int timeArray[1];
int totalValue[1];
int tempIntValue[1];

int intValuesArray[];

double result;

double lotsValue[1];
double volumeValue[1];
double priceValue[1];
double stoplossValue[1];
double takeprofitValue[1];
double valueValue[1];
double check_value[1];
double deviationDoubleValue[1];
double stepValue[1];
double maximumValue[1];
double tempDoubleValue[1];

double priceArray[];
double volumeArray[];
double doubleValuesArray[];

double myBid;
double myAsk;

int preinit()
{
   message[0]        = "111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111" + "";
   symbolValue[0]    = "222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222" + "";
   commentValue[0]   = "333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333" + "";
   msgValue[0]       = "444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444" + "";
   captionValue[0]   = "555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555" + "";
   filenameValue[0]  = "666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666" + "";
   ftp_pathValue[0]  = "777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777" + "";
   subjectValue[0]   = "888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888" + "";
   some_textValue[0] = "999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999" + "";
   nameValue[0]      = "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" + "";
   prefix_nameValue[0] = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" + "";

   return (0);
}

int init() {
   preinit();
     
   myBid = Bid;
   myAsk = Ask;
      
   if (IsDllsAllowed() == FALSE) 
   {
      MessageBoxA(0, "Dlls not allowed.", "MtApi", 0);
      isCrashed = TRUE;
      return (1);
   }
   if (IsLibrariesAllowed() == FALSE) 
   {
      MessageBoxA(0, "Libraries not allowed.", "MtApi", 0);
      isCrashed = TRUE;
      return (1);
   }

   if (IsTradeAllowed() == FALSE) 
   {
      MessageBoxA(0, "Trade not allowed.", "MtApi", 0);
      isCrashed = TRUE;
      return (1);
   }  

   ExpertHandle = WindowHandle(Symbol(), Period());
   
   if (!initExpert(ExpertHandle, ConnectionProfile, Symbol(), Bid, Ask, message))
   {
       MessageBoxA(0, message[0], "MtApi", 0);
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
         MessageBoxA(0, message[0], "MtApi", 0);
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
   pCommandType[0] = 0;
      
   if (!getCommandType(ExpertHandle, pCommandType))
   {
      Print("[ERROR] getCommandType");
      return (0);
   }         
   
   int commandType = pCommandType[0];     
   
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
      
      if (!sendIntResponse(ExpertHandle, OrderSend(symbolValue[0], cmdValue[0], volumeValue[0], priceValue[0]
                                       , slippageValue[0], stoplossValue[0], takeprofitValue[0]
                                       , commentValue[0], magicValue[0], expirationValue[0], arrow_colorValue[0]))) 
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

      if (!sendBooleanResponse(ExpertHandle, OrderClose(ticketValue[0], lotsValue[0], priceValue[0], slippageValue[0], colorValue[0]))) 
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

      if (!sendBooleanResponse(ExpertHandle, OrderCloseBy(ticketValue[0], oppositeValue[0], colorValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, OrderDelete(ticketValue[0], arrow_colorValue[0]))) 
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
      
      if (!sendBooleanResponse(ExpertHandle, OrderModify(ticketValue[0], priceValue[0], stoplossValue[0], takeprofitValue[0], expirationValue[0], arrow_colorValue[0]))) 
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
   
      indexValue[0] = 0;
      selectValue[0] = 0;
      poolValue[0] = 0;
      
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
      
      if (!sendBooleanResponse(ExpertHandle, OrderSelect(indexValue[0], selectValue[0], poolValue[0]))) 
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
   
      if (!sendStringResponse(ExpertHandle, ErrorDescription(errorCodeValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, AccountFreeMarginCheck(symbolValue[0], cmdValue[0], volumeValue[0]))) 
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
      
      Alert(msgValue[0]);
      
      break;         

   case 57: //Comment
      if (!getStringValue(ExpertHandle, 0, msgValue)) 
      {
         PrintParamError("msg");
      }
      
      Comment(msgValue[0]);
      
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
      
      if (!sendDoubleResponse(ExpertHandle, MarketInfo(symbolValue[0], typeValue[0]))) 
      {
         PrintResponseError("MarketInfo");
      } 
      
      break;         

   case 60: //MessageBox      
      if (!getStringValue(ExpertHandle, 0, symbolValue)) 
      {
         PrintParamError("symbol");
      }
      
      if (!sendIntResponse(ExpertHandle, MessageBox(symbolValue[0]))) 
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
      
      if (!sendIntResponse(ExpertHandle, MessageBox(symbolValue[0], captionValue[0], flagValue[0]))) 
      {
         PrintResponseError("MessageBox");
      } 
      
      break;         

   case 62: //PlaySound
      if (!getStringValue(ExpertHandle, 0, filenameValue)) 
      {
         PrintParamError("filename");
      }
      
      PlaySound(filenameValue[0]);
               
      break;         

   case 63: //Print
      if (!getStringValue(ExpertHandle, 0, msgValue)) 
      {
         PrintParamError("msg");
      }
   
      Print(msgValue[0]);
      
      break;         

   case 64: //SendFTP
      if (!getStringValue(ExpertHandle, 0, filenameValue)) 
      {
         PrintParamError("filename");
      }
      
      if (!sendBooleanResponse(ExpertHandle, SendFTP(filenameValue[0]))) 
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
      
      if (!sendBooleanResponse(ExpertHandle, SendFTP(filenameValue[0], ftp_pathValue[0]))) 
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
      
      SendMail(subjectValue[0], some_textValue[0]);
      
      break;         

   case 67: //Sleep
      if (!getIntValue(ExpertHandle, 0, millisecondsValue)) 
      {
         PrintParamError("milliseconds");
      }
      
      Sleep(millisecondsValue[0]);
      
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

      if (!sendIntResponse(ExpertHandle, TimeDay(dateValue[0]))) 
      {
         PrintResponseError("TimeDay");
      } 
      
      break;

   case 80: //TimeDayOfWeek
      if (!getIntValue(ExpertHandle, 0, dateValue)) 
      {
         PrintParamError("date");
      }

      if (!sendIntResponse(ExpertHandle, TimeDayOfWeek(dateValue[0]))) 
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

      if (!sendIntResponse(ExpertHandle, TimeHour(timeValue[0]))) 
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

      if (!sendIntResponse(ExpertHandle, TimeMinute(timeValue[0]))) 
      {
         PrintResponseError("TimeMinute");
      } 
      
      break;

   case 85: //TimeMonth
      if (!getIntValue(ExpertHandle, 0, timeValue)) 
      {
         PrintParamError("time");
      }

      if (!sendIntResponse(ExpertHandle, TimeMonth(timeValue[0]))) 
      {
         PrintResponseError("TimeMonth");
      } 
      
      break;

   case 86: //TimeSeconds
      if (!getIntValue(ExpertHandle, 0, timeValue)) 
      {
         PrintParamError("time");
      }

      if (!sendIntResponse(ExpertHandle, TimeSeconds(timeValue[0]))) 
      {
         PrintResponseError("TimeSeconds");
      } 
      
      break;

   case 87: //TimeYear
      if (!getIntValue(ExpertHandle, 0, timeValue)) 
      {
         PrintParamError("time");
      }

      if (!sendIntResponse(ExpertHandle, TimeYear(timeValue[0]))) 
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
      
      if (!sendBooleanResponse(ExpertHandle, GlobalVariableCheck(nameValue[0]))) 
      {
         PrintResponseError("GlobalVariableCheck");
      } 
      
   break;

   case 90: //GlobalVariableDel
      if (!getStringValue(ExpertHandle, 0, nameValue)) 
      {
         PrintParamError("name");
      }
      
      if (!sendBooleanResponse(ExpertHandle, GlobalVariableDel(nameValue[0]))) 
      {
         PrintResponseError("GlobalVariableDel");
      } 
      
   break;

   case 91: //GlobalVariableGet
      if (!getStringValue(ExpertHandle, 0, nameValue)) 
      {
         PrintParamError("name");
      }
      
      if (!sendDoubleResponse(ExpertHandle, GlobalVariableGet(nameValue[0]))) 
      {
         PrintResponseError("GlobalVariableGet");
      } 
      
   break;

   case 92: //GlobalVariableName
      if (!getIntValue(ExpertHandle, 0, indexValue)) 
      {
         PrintParamError("index");
      }
      
      if (!sendStringResponse(ExpertHandle, GlobalVariableName(indexValue[0]))) 
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
      
      if (!sendIntResponse(ExpertHandle, GlobalVariableSet(nameValue[0], valueValue[0]))) 
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
      
      if (!sendBooleanResponse(ExpertHandle, GlobalVariableSetOnCondition(nameValue[0], valueValue[0], check_value[0]))) 
      {
         PrintResponseError("GlobalVariableSetOnCondition");
      } 
      
   break;
   
   case 95: //GlobalVariablesDeleteAll
      if (!getStringValue(ExpertHandle, 0, prefix_nameValue)) 
      {
         PrintParamError("prefix_name");
      }
      
      if (!sendIntResponse(ExpertHandle, GlobalVariablesDeleteAll(prefix_nameValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iAC(symbolValue[0], timeframeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iAD(symbolValue[0], timeframeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iAD(symbolValue[0], timeframeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iADX(symbolValue[0], timeframeValue[0], periodValue[0], applied_priceValue[0], modeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iATR(symbolValue[0], timeframeValue[0], periodValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iAO(symbolValue[0], timeframeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iBearsPower(symbolValue[0], timeframeValue[0], periodValue[0], applied_priceValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iBands(symbolValue[0], timeframeValue[0], periodValue[0], deviationValue[0], bands_shiftValue[0], applied_priceValue[0], modeValue[0], shiftValue[0]))) 
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
      
      arraySize = countValue[0];      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue[0];         
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
      
      if (!sendDoubleResponse(ExpertHandle, iBandsOnArray(doubleValuesArray, totalValue[0], periodValue[0], deviationValue[0], bands_shiftValue[0], modeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iBullsPower(symbolValue[0], timeframeValue[0], periodValue[0], applied_priceValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iCCI(symbolValue[0], timeframeValue[0], periodValue[0], applied_priceValue[0], shiftValue[0]))) 
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
      
      arraySize = countValue[0];      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue[0];         
      }                  
      
      getIntValue(ExpertHandle, paramIndex, totalValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, periodValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, shiftValue);
      paramIndex++;    
      
      if (!sendDoubleResponse(ExpertHandle, iCCIOnArray(doubleValuesArray, totalValue[0], periodValue[0], shiftValue[0]))) 
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
      
      arraySize = countValue[0];      
      ArrayResize(intValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getIntValue(ExpertHandle, paramIndex, tempIntValue);
         paramIndex++;
         intValuesArray[index] = tempIntValue[0];         
      }                  
      
      getIntValue(ExpertHandle, paramIndex, modeValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, shiftValue);
      paramIndex++;    
      
      switch(arraySize)
      {
         case 0:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0], totalValue[0], periodValue[0], shiftValue[0]));
            break;
         case 1:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , intValuesArray[0]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;
         case 2:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , intValuesArray[0]
               , intValuesArray[1]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;            
         case 3:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , intValuesArray[0]
               , intValuesArray[1]
               , intValuesArray[2]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;                    
         case 4:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , intValuesArray[0]
               , intValuesArray[1]
               , intValuesArray[2]
               , intValuesArray[3]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;   
         case 5:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , intValuesArray[0]
               , intValuesArray[1]
               , intValuesArray[2]
               , intValuesArray[3]
               , intValuesArray[4]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;        
         case 6:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , intValuesArray[0]
               , intValuesArray[1]
               , intValuesArray[2]
               , intValuesArray[3]
               , intValuesArray[4]
               , intValuesArray[5]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;                                        
         case 7:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , intValuesArray[0]
               , intValuesArray[1]
               , intValuesArray[2]
               , intValuesArray[3]
               , intValuesArray[4]
               , intValuesArray[5]
               , intValuesArray[6]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;      
         case 8:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , intValuesArray[0]
               , intValuesArray[1]
               , intValuesArray[2]
               , intValuesArray[3]
               , intValuesArray[4]
               , intValuesArray[5]
               , intValuesArray[6]
               , intValuesArray[7]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;                             
         case 9:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , intValuesArray[0]
               , intValuesArray[1]
               , intValuesArray[2]
               , intValuesArray[3]
               , intValuesArray[4]
               , intValuesArray[5]
               , intValuesArray[6]
               , intValuesArray[7]
               , intValuesArray[8]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;                     
         case 10:
         default:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
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
            , totalValue[0], periodValue[0], shiftValue[0]));
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
      
      arraySize = countValue[0];      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue[0];         
      }                  
      
      getIntValue(ExpertHandle, paramIndex, modeValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, shiftValue);
      paramIndex++;    
      
      switch(arraySize)
      {
         case 0:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0], totalValue[0], periodValue[0], shiftValue[0]));
            break;
         case 1:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , doubleValuesArray[0]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;
         case 2:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;            
         case 3:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , doubleValuesArray[2]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;                    
         case 4:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , doubleValuesArray[2]
               , doubleValuesArray[3]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;   
         case 5:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , doubleValuesArray[2]
               , doubleValuesArray[3]
               , doubleValuesArray[4]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;        
         case 6:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , doubleValuesArray[2]
               , doubleValuesArray[3]
               , doubleValuesArray[4]
               , doubleValuesArray[5]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;                                        
         case 7:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , doubleValuesArray[2]
               , doubleValuesArray[3]
               , doubleValuesArray[4]
               , doubleValuesArray[5]
               , doubleValuesArray[6]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;      
         case 8:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , doubleValuesArray[2]
               , doubleValuesArray[3]
               , doubleValuesArray[4]
               , doubleValuesArray[5]
               , doubleValuesArray[6]
               , doubleValuesArray[7]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;                             
         case 9:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
               , doubleValuesArray[0]
               , doubleValuesArray[1]
               , doubleValuesArray[2]
               , doubleValuesArray[3]
               , doubleValuesArray[4]
               , doubleValuesArray[5]
               , doubleValuesArray[6]
               , doubleValuesArray[7]
               , doubleValuesArray[8]
               , totalValue[0], periodValue[0], shiftValue[0]));
               break;                     
         case 10:
         default:
            sendDoubleResponse(ExpertHandle, iCustom(symbolValue[0], timeframeValue[0], nameValue[0]
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
            , totalValue[0], periodValue[0], shiftValue[0]));
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
      
      if (!sendDoubleResponse(ExpertHandle, iDeMarker(symbolValue[0], timeframeValue[0], periodValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iEnvelopes(symbolValue[0], timeframeValue[0], ma_periodValue[0], ma_methodValue[0],
                                                         ma_shiftValue[0], applied_priceValue[0], deviationDoubleValue[0], modeValue[0],
                                                         shiftValue[0]))) 
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
      
      arraySize = countValue[0];      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue[0];         
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
      
      if (!sendDoubleResponse(ExpertHandle, iEnvelopesOnArray(doubleValuesArray, totalValue[0], ma_periodValue[0], ma_methodValue[0], ma_shiftValue[0],  deviationDoubleValue[0], modeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iForce(symbolValue[0], timeframeValue[0], periodValue[0], ma_methodValue[0],
                                                         applied_priceValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iFractals(symbolValue[0], timeframeValue[0], modeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iGator(symbolValue[0], timeframeValue[0], jaw_periodValue[0], jaw_shiftValue[0]
                                                      , teeth_periodValue[0], teeth_shiftValue[0], lips_periodValue[0], lips_shiftValue[0]
                                                      , ma_methodValue[0], applied_priceValue[0], modeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iIchimoku(symbolValue[0], timeframeValue[0], tenkan_senValue[0], kijun_senValue[0]
                                                      , senkou_span_bValue[0], modeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iBWMFI(symbolValue[0], timeframeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iMomentum(symbolValue[0], timeframeValue[0], periodValue[0], applied_priceValue[0]
                                                         , shiftValue[0]))) 
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
      
      arraySize = countValue[0];      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue[0];         
      }                  
      
      getIntValue(ExpertHandle, paramIndex, totalValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, periodValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, shiftValue);
      paramIndex++;    
      
      if (!sendDoubleResponse(ExpertHandle, iMomentumOnArray(doubleValuesArray, totalValue[0], periodValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iMFI(symbolValue[0], timeframeValue[0], periodValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iMA(symbolValue[0], timeframeValue[0], periodValue[0], ma_shiftValue[0]
                                                   , ma_methodValue[0], applied_priceValue[0], shiftValue[0]))) 
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
      
      arraySize = countValue[0];      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue[0];         
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
      
      if (!sendDoubleResponse(ExpertHandle, iMAOnArray(doubleValuesArray, totalValue[0], periodValue[0], ma_shiftValue[0], ma_methodValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iOsMA(symbolValue[0], timeframeValue[0], fast_ema_periodValue[0], slow_ema_periodValue[0]
                                                   , signal_periodValue[0], applied_priceValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iMACD(symbolValue[0], timeframeValue[0], fast_ema_periodValue[0], slow_ema_periodValue[0]
                                                   , signal_periodValue[0], applied_priceValue[0], modeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iOBV(symbolValue[0], timeframeValue[0], applied_priceValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iSAR(symbolValue[0], timeframeValue[0], stepValue[0], maximumValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iRSI(symbolValue[0], timeframeValue[0], periodValue[0], applied_priceValue[0], shiftValue[0]))) 
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
      
      arraySize = countValue[0];      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue[0];         
      }                  
      
      getIntValue(ExpertHandle, paramIndex, totalValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, periodValue);
      paramIndex++;
      getIntValue(ExpertHandle, paramIndex, shiftValue);
      paramIndex++;    
      
      if (!sendDoubleResponse(ExpertHandle, iRSIOnArray(doubleValuesArray, totalValue[0], periodValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iRVI(symbolValue[0], timeframeValue[0], periodValue[0], modeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iStdDev(symbolValue[0], timeframeValue[0], ma_periodValue[0], ma_shiftValue[0]
                                                      , ma_methodValue[0], applied_priceValue[0], shiftValue[0]))) 
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
      
      arraySize = countValue[0];      
      ArrayResize(doubleValuesArray, arraySize);
      
      for(index = 0; index < arraySize; index++)
      {
         getDoubleValue(ExpertHandle, paramIndex, tempDoubleValue);
         paramIndex++;
         doubleValuesArray[index] = tempDoubleValue[0];         
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
      
      if (!sendDoubleResponse(ExpertHandle, iStdDevOnArray(doubleValuesArray, totalValue[0], ma_periodValue[0], ma_shiftValue[0], ma_methodValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iStochastic(symbolValue[0], timeframeValue[0], KperiodValue[0], DperiodValue[0]
                                                      , slowingValue[0], methodValue[0], price_fieldValue[0], modeValue[0], shiftValue[0]))) 
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

      if (!sendDoubleResponse(ExpertHandle, iWPR(symbolValue[0], timeframeValue[0], periodValue[0], shiftValue[0]))) 
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
      
      if (!sendIntResponse(ExpertHandle, iBars(symbolValue[0], timeframeValue[0]))) 
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
      
      if (!sendIntResponse(ExpertHandle, iBarShift(symbolValue[0], timeframeValue[0], timeValue[0], exactValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iClose(symbolValue[0], timeframeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iHigh(symbolValue[0], timeframeValue[0], shiftValue[0]))) 
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
      
      if (!sendIntResponse(ExpertHandle, iHighest(symbolValue[0], timeframeValue[0], typeValue[0], countValue[0], startValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iLow(symbolValue[0], timeframeValue[0], shiftValue[0]))) 
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
      
      if (!sendIntResponse(ExpertHandle, iLowest(symbolValue[0], timeframeValue[0], typeValue[0], countValue[0], startValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iOpen(symbolValue[0], timeframeValue[0], shiftValue[0]))) 
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

      if (!sendIntResponse(ExpertHandle, iTime(symbolValue[0], timeframeValue[0], shiftValue[0]))) 
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
      
      if (!sendDoubleResponse(ExpertHandle, iVolume(symbolValue[0], timeframeValue[0], shiftValue[0]))) 
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
            
      barsCount = iBars(symbolValue[0], timeframeValue[0]);
      priceCount = ArrayResize(priceArray, barsCount);
      
      for(index = 0; index < priceCount; index++)
      {
         priceArray[index] = iClose(symbolValue[0], timeframeValue[0], index);
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
            
      barsCount = iBars(symbolValue[0], timeframeValue[0]);
      priceCount = ArrayResize(priceArray, barsCount);
      
      for(index = 0; index < priceCount; index++)
      {
         priceArray[index] = iHigh(symbolValue[0], timeframeValue[0], index);
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
            
      barsCount = iBars(symbolValue[0], timeframeValue[0]);
      priceCount = ArrayResize(priceArray, barsCount);
      
      for(index = 0; index < priceCount; index++)
      {
         priceArray[index] = iLow(symbolValue[0], timeframeValue[0], index);
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
            
      barsCount = iBars(symbolValue[0], timeframeValue[0]);
      priceCount = ArrayResize(priceArray, barsCount);
      
      for(index = 0; index < priceCount; index++)
      {
         priceArray[index] = iOpen(symbolValue[0], timeframeValue[0], index);
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
            
      barsCount = iBars(symbolValue[0], timeframeValue[0]);
      volumeCount = ArrayResize(volumeArray, barsCount);
      
      for(index = 0; index < volumeCount; index++)
      {
         volumeArray[index] = iVolume(symbolValue[0], timeframeValue[0], index);
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
            
      barsCount = iBars(symbolValue[0], timeframeValue[0]);
      timeCount = ArrayResize(timeArray, barsCount);
      
      for(index = 0; index < timeCount; index++)
      {
         timeArray[index] = iTime(symbolValue[0], timeframeValue[0], index);
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
   
   ticketValue[0] = 0;
         
   if (!getIntValue(ExpertHandle, 0, ticketValue))
   {
      PrintParamError("ticket");
      return (false);
   }   
   
   selected = OrderSelect(ticketValue[0], SELECT_BY_TICKET);
   
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