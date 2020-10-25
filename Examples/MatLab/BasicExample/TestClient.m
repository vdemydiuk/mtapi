function TestClient
% Main program 

% interfaces to the MT5 client
% receives price data from MT5 via .NET interface
% recieives history bars from MT5


ordersend_ok = false;

   [output_figH,output_listH] =  NT_CreateOutputWindow('Test OutputWindow ',[400,400,800,200]);
   NT_TextToOutputWindow(output_listH,'Startup ...');

   [Trades_figH,Trades_listH] =  NT_CreateOutputWindow('Trades OutputWindow ',[400,800,800,200]);
   NT_TextToOutputWindow(Trades_listH,'Startup Trades Window ...');

   [Events_figH,Events_listH] =  NT_CreateOutputWindow('Events OutputWindow ',[400,1200,800,200]);
   NT_TextToOutputWindow(Events_listH,'Startup Events Window ...');


format short
MTApi5_dll_Location = 'C:\Program Files\MtApi5\MtApi5.dll'; 


%% Connect to .NET

asm1 = NET.addAssembly (MTApi5_dll_Location);

NT_TextToOutputWindow(output_listH,'MtApi5.dll loaded ...');
%% Open the Metatrader Link via .NET
MT5 = ConnectToMT5_Api;                                                     % creates the MT5 object
MT5.setOutputWindowHandle(Events_listH)                                     % copy output window pointer to listener 
MT5.setConnectionListener;                                                  % create connection listener
MT5.setQuoteListener;                                                       % create quote listener
MT5.setQuoteAddedListener;                                                  % create quote added listener

port = 8301;
addr = '127.0.0.1';
MT5.startMT5(addr,port);                                                    % establishes .NET connection

NT_TextToOutputWindow(output_listH,'Setup Mt5Api ...');

while MT5.h.ConnectionState ~= MtApi5.Mt5ConnectionState.Connected
    pause(0.01);
   NT_TextToOutputWindow(output_listH,'Waiting for Connection....');
end


NT_TextToOutputWindow(output_listH,'Connected ...');


%% Test SymbolsTotal
text_to = sprintf('Symbols= %d ', MT5.SymbolsTotal(true));
NT_TextToOutputWindow(output_listH,text_to);
%% Test AccountInfo

text_to = sprintf('Balance = %d \n',MT5.AccountInfoDouble(MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_BALANCE));
NT_TextToOutputWindow(output_listH,text_to);
%% Test OrderSend
request = MtApi5.MqlTradeRequest;   
tick = MtApi5.MqlTick;                        

% [tick] = MT5.SymbolInfoTick('EURUSD');     


request.Action     = MtApi5.ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_DEAL;
request.Symbol     = 'EURUSD';
request.Deviation  = 30;
request.Volume     = 0.01;   %% Account MIN 10000 !
request.Type       = MtApi5.ENUM_ORDER_TYPE.ORDER_TYPE_BUY;

request.Price = tick.ask;

try
 [ordersend_ok,result] = MT5.OrderSend(request);
 catch exception
    text_to = sprintf('NET Error in OrderSend = %s\n ',exception.message);
    NT_TextToOutputWindow(Trades_listH,text_to);
 end      
if (ordersend_ok)
       text_to = sprintf('OrderSend OK \n'); 
       NT_TextToOutputWindow(Trades_listH,text_to);
       
       text_to = sprintf('Result: %s \n',char(result.ToString));            
       NT_TextToOutputWindow(Trades_listH,text_to);
    
else
     text_to = sprintf('OrderSend ERROR \n'); 
     NT_TextToOutputWindow(Trades_listH,text_to);
    
end
    
    

%% while not pressed 0
while true
    pause(0.1);
    prompt = 'Press 0 key to stop\n';
    x = input(prompt);
    if (x == 0)
        break;
    end
end
MT5.stopMT5;
fprintf('Connection Stopped \n');

end