classdef ConnectToMT5_Api < handle
  %CONNECTMT5 Connects to the MtApi Client via .NET
  
  properties
    h;
    hOutputWindow
  end
  
  methods
%xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx    
    function obj = ConnectToMT5_Api
      
      obj.h = MtApi5.MtApi5Client();
    end
%xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx    
    function startMT5 (self,addr,port)
      %% Handle to the .NET client
      
      fprintf('Connecting...\n');

      self.h.BeginConnect(addr,port);
      
      
    end
    
    function stopMT5(self)
        self.h.BeginDisconnect();
    end
    function setOutputWindowHandle(self,h)
        self.hOutputWindow = h;
    end
 %% 
    function [copied_cnt, MqlRates] = getHistoryBars(self)
        
        symbol          = 'EURUSD';
        period          = 'M1';
        start_cnt       = 0;
        end_cnt         = 5;

                
       [ copied_cnt,MqlRates ] = self.h.CopyRates(symbol,period,start_cnt,end_cnt);
         
    end
% *************************************************************************
% **************** Api Function **********************************************
%**************************************************************************



%%  double =  AccountInfoDouble(enum MtApi5.ENUM_ACCOUNT_INFO_DOUBLE)    

   function res = AccountInfoDouble(self,enum)
        res = self.h.AccountInfoDouble(enum);    
    end
    
%%   uint32 =  SymbolsTotal(bool selected)   
    
function  res = SymbolsTotal(self,selected)
    
    res = self.h.SymbolsTotal(selected);

end
    

%% Market Info

%% public bool SymbolInfoTick(string symbol, out MqlTick  tick)
function [mqltick] = SymbolInfoTick(self,symbol)
      
    [mqltick] = self.h.SymbolInfoTick(symbol);
    
end

%% Trading functions

%% bool OrderSend(MqlTradeRequest request, out MqlTradeResult result)
%% 
function [res,result]  = OrderSend(self, request)
 
    [res,result] = self.h.OrderSend(request);
 
     
end




%%     
    function r = setQuoteListener(self)
        r = addlistener(self.h, 'QuoteUpdate', @self.quoteListener);
    end
    
   function r = setQuoteAddedListener(self)
        r = addlistener(self.h, 'QuoteAdded', @self.quoteAddedListener);
    end
 
    function r = setConnectionListener(self)
        r = addlistener(self.h, 'ConnectionStateChanged', @self.connectionListener);
    end
    
    
   function quoteAddedListener(event)
       askString = num2str(event.Quote.Ask,'%01.5f');
%          askString = sprintf(event.Quote.Ask,'%01.5f');
       bidString = num2str(event.Quote.Bid,'%01.5f');
%          bidString = num2str(event.Quote.Bid,'%01.5f');
          transmitString = strcat(askString,',',bidString);
         fprintf('%s: %s\n',char(event.Quote.Instrument),transmitString);
    end
  
    
    
    function quoteListener(self,~,event)
        askString = num2str(event.Quote.Ask,'%01.5f');
        bidString = num2str(event.Quote.Bid,'%01.5f');
        transmitString = strcat(askString,',',bidString);
        text_to = sprintf('%s: %s\n',char(event.Quote.Instrument),transmitString);
       
        NT_TextToOutputWindow(self.hOutputWindow,text_to);

    end
    
    function connectionListener(self, ~, event)
        connectState = event.Status.ToString;
        text_to = sprintf('Connection changed: %s\n', char(connectState));   
        NT_TextToOutputWindow(self.hOutputWindow,text_to);
        
        if (connectState.eq('Failed') == true)
            text_to = sprintf('Reason: %s\n', char(event.ConnectionMessage));
            NT_TextToOutputWindow(self.hOutputWindow,text_to);
        end
        
    end
  end
end