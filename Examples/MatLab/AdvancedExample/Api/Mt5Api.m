classdef Mt5Api < handle
  %CONNECTMT5 Connects to the MtApi Client via .NET
  
  properties
  
    MTApi5_dll_Location = 'dll\MtApi5.dll'
    
%   MTApi5_dll_Location = 'E:\GitHub-RePos\MtApi\build\products\Release\MtApi5.dll';% 1.021 
%   MTApi5_dll_Location = '  E:\GitHub-RePos\MtApi\MtApi5\bin\x64\Release\MtApi5.dll' ;%  1.021
%  MTApi5_dll_Location = 'd:\Git-Extern\mtapi\build\products\Debug\MtApi5.dll';% 1.019 
%   MTApi5_dll_Location =  'D:\GitHub-RePos\MtApi\build\products\Release\MtApi5.dll'; % 1.0191
    
    vVersion = 1.021;
    h;
    asm;
    cAccount;
    cCommon;
    cCheckup;
    cDateTime;
    cEvents;
    cMarketInfo;
    cObject;
    cProp;
    cTeIndicator;
    cTimeSeries;
    cTrade;
    cLogger;
    fDeleteLogger;
    hQuoteAddedListener;
    hQuoteListener;
    hConnectionListener;
    hOnTradeTransactionListener;
    
    hOnTesterDeInitListener % Listener for Tester DeInit Event
    charConnectionState = 'StartUp';
    
    bStatusDeInit = false;  % Status Flag of DeInit Tester   
  end
  
  methods
  %% constructor
    function obj = Mt5Api(MTApi5_dll_Location)
        if nargin > 1 
            if isempty(MTApi5_dll_Location)
               disp('No Path for API dll using standard');
            end           
        end
       obj.MTApi5_dll_Location = strcat(pwd(),'\',MTApi5_dll_Location);
       
      [obj.cLogger, obj.fDeleteLogger] = logging.getLogger('ApiLogMain');  
      
      
      
      MtApi_asm = NET.addAssembly(obj.MTApi5_dll_Location);
      
      
      obj.cLogger.debug('Net Assembly loaded',0);
      obj.cLogger.debug(char(MtApi_asm.AssemblyHandle.CodeBase),0);
      obj.cLogger.debug(char(MtApi_asm.AssemblyHandle.ToString),0);
      
      obj.h = MtApi5.MtApi5Client();
      
      
      obj.asm = MtApi_asm;
      obj.cAccount        = cntMt5Api_account(obj);
      obj.cCommon         = cntMt5Api_common(obj);
      obj.cCheckup        = cntMt5Api_checkup(obj);
      obj.cDateTime       = cntMt5Api_datetime(obj);
      obj.cEvents         = cntMt5Api_events(obj);
      obj.cMarketInfo     = cntMt5Api_marketinfo(obj);
      obj.cTeIndicator    = cntMt5Api_teindicator(obj);
      obj.cTimeSeries     = cntMt5Api_timeseries(obj);
      obj.cTimeSeries.mSymbol = 'EURUSD';
      obj.cTrade          = cntMt5Api_trade(obj);
%       obj.cEnum         = cntMtApi_enums(obj);
    end
  %% destructor
  function delete(self)
                
     self.asm.delete;
     
      
%          delete(self.hOnTesterDeInitListener);
        
                
  end
  %% class functions
    function ok = startMT5 (self,addr,port)
      %% Handle to the .NET client    
       self.cLogger.info(sprintf('Connecting  to %s:%d ...',addr,port));
       
       
     
        
        ok = false;
        
        t = timer('TimerFcn', @errorWhileNoConnection, 'StartDelay',60);
        start(t);
        status=true;
        
        while  (status==true) 
            
           try
       
            self.h.BeginConnect(addr,port);      
      
           catch ME
               
           end    
            
           pause(1);
           
            if  self.h.ConnectionState == MtApi5.Mt5ConnectionState.Connected
                
                stop(t);
                delete(t);
                ok = true;
                self.cLogger.info(sprintf('Conneced  to %s:%d ',addr,port));
                
                return;
            end
            
         
            
        end
        
        ok = false;
        
        self.cLogger.error('Connection Timer Out!'); 
        
        function errorWhileNoConnection(self,event)
            
          status=false;                   
          stop(t); 
          delete(t);       
          
        end
    
    end   
    function stopMT5(self)
        
        self.cLogger.info('Close Connection...');
        self.h.BeginDisconnect();
    end    
    function init_Loggers(self,cmdLineLevel,logLevel,WebHook,Instance)
   % cmdLineLevel,logLevel -> ALL,TRACE,DEBUG,INFO,WARNING,ERROR,CRITICAL,OFF
     self.cLogger.setSlackWebHook(WebHook);
     self.cLogger.setSlackInstance(Instance)       
                    
%     self.cLogger = logging.getLogger('ApiLog');
    self.cLogger.setFilename('C:\Work\NeuronalTrader_matlab\Log\Api.log');
          
          switch logLevel
                      case 'ALL'
                                    self.cLogger.setLogLevel(logging.logging.ALL);
                      case 'TRACE'
                                    self.cLogger.setLogLevel(logging.logging.TRACE);
                      case 'DEBUG'
                                    self.cLogger.setLogLevel(logging.logging.DEBUG);
                      case 'INFO'
                                    self.cLogger.setLogLevel(logging.logging.INFO);
                      case 'WARNING'
                                    self.cLogger.setLogLevel(logging.logging.WARNING);
                      case 'ERROR'
                                    self.cLogger.setLogLevel(logging.logging.ERROR);
                      case 'CRITICAL'
                                    self.cLogger.setLogLevel(logging.logging.CRITICAL);
                      case 'OFF'
                                    self.cLogger.setLogLevel(logging.logging.OFF);
                      otherwise
                                  error('cLogger Init');
          end
          
          switch cmdLineLevel
                      
                      case 'ALL'
                                    self.cLogger.setCommandWindowLevel(logging.logging.ALL);
                      case 'TRACE'
                                    self.cLogger.setCommandWindowLevel(logging.logging.TRACE);
                      case 'DEBUG'
                                    self.cLogger.setCommandWindowLevel(logging.logging.DEBUG);
                      case 'INFO'
                                    self.cLogger.setCommandWindowLevel(logging.logging.INFO);
                      case 'WARNING'
                                    self.cLogger.setCommandWindowLevel(logging.logging.WARNING);
                      case 'ERROR'
                                    self.cLogger.setCommandWindowLevel(logging.logging.ERROR);
                      case 'CRITICAL'
                                    self.cLogger.setCommandWindowLevel(logging.logging.CRITICAL);
                      case 'OFF'
                                    self.cLogger.setCommandWindowLevel(logging.logging.OFF);
                      otherwise
                                  error('cLogger Init');
                     
          end          
                      
                      
                      
          
          

          self.cLogger.info(' *********     Parameter: LogLevel = ALL      ****************  ');

          self.cLogger.trace('Trace Line Test');
          self.cLogger.debug('Debug Line Test');  
          self.cLogger.info('Info Line Test');
          self.cLogger.warn('Warning Line Test');
          self.cLogger.error('Error Line Test');
          self.cLogger.critical('Critical Line Test');

          self.cLogger.info('Api Logger is started');
            end
    function result = isApiConnected(self)
    % Check if Api is Connected
    if self.h.ConnectionState == MtApi5.Mt5ConnectionState.Connected
        result = true;
    else
        result = false;
    end
    end
    function result = isMarketOpen(self)
    % Check if Market is Open
    
    end
    function res = isTesterMode(self) 
     % Checks if the Expert Advisor runs in the testing mode..
     res = self.h.IsTesting();
    end
    function res = send_TesterStop(self) 
     % Checks if the Expert Advisor runs in the testing mode..
     res = self.cCommon.TimeGMT();
    end
    function res = GetQuotes(self)
     % Load quotes connected into MetaTrader API.
     res = self.h.GetQuotes();
    end  
    
    %% ASM Functions
    function version = getVersion(self)
        version = char(self.asm.AssemblyHandle.ToString());
        self.cLogger.info(version),0;     
    end
    function path = getBasePath(self)
        path = char(self.asm.AssemblyHandle.CodeBase);
        self.cLogger.info(path),0;     
    end
    
  %% develop function
    function ticks = readTicks(self)
        
        
        
    end
    function eurusd = runOnX7Live(self)
      self.InitLogger;
      self.cLogger.info('Develop on X7 live started');
      self.setConnectionListener;
      self.setOnTradeTransactionListener;

      if self.startMT5('192.168.178.15',8228)    % Connnect to Live
                   
        eurusd = DataStore.Ticks();
        eurusd.createMqlTicks(int32(100));
        
        self.setQuoteListener(eurusd);      
      
      end   
      
    end
    function stopX7Live(self)
      self.cLogger.trace('Cleanup Listener ,Objects');
      self.hConnectionListener.delete;
      self.hOnTradeTransactionListener.delete;

      self.stopMT5;
      while self.isApiConnected()
         pause(0.1);
      end
      logging.clearLogger('ApiLogMain')
    end
    function [connected ,eurusd] = runOn(self,mode)
      
      self.cLogger.info('Setup Connection');
      self.setConnectionListener();
      self.setOnTradeTransactionListener();
      
      switch mode
          
           case 'AA'
      
             self.startMT5('192.168.178.15',8301);    % Connnect to Pool Tester AA

             eurusd = DataStore.Ticks();
             eurusd.createMqlTicks(int32(100));
        
%              self.setQuoteListener(eurusd);
          
           case 'AB'
      
             self.startMT5('192.168.178.15',8302);    % Connnect to Pool Tester AB

             eurusd = DataStore.Ticks();
             eurusd.createMqlTicks(int32(100));
        
%              self.setQuoteListener(eurusd);
                   
           case 'AC'
      
             self.startMT5('192.168.178.15',8303);    % Connnect to Pool Tester AC

             eurusd = DataStore.Ticks();
             eurusd.createMqlTicks(int32(100));

          case 'AD'
      
             self.startMT5('192.168.178.15',8304);    % Connnect to Pool Tester AD

             eurusd = DataStore.Ticks();
             eurusd.createMqlTicks(int32(100));
        
         case 'X7T'
      
             self.startMT5('192.168.178.15',8230);    % Connnect to Tester

             eurusd = DataStore.Ticks();
             eurusd.createMqlTicks(int32(100));
        
%              self.setQuoteListener(eurusd);
             
           case 'X7L'
      
             self.startMT5('192.168.178.15',8228);    % Connnect to Live

             eurusd = DataStore.Ticks();
             eurusd.createMqlTicks(int32(100));
        
%              self.setQuoteListener(eurusd);
             
           case 'X3CommandPoolAA'
      
             self.startMT5('127.0.0.1',8231);    % Connnect to Pool Tester AA

             eurusd = DataStore.Ticks();
             eurusd.createMqlTicks(int32(100));
        
%              self.setQuoteListener(eurusd);
          
  
            
            case 'X3T'
      
             self.startMT5('192.168.178.25',8231);    % Connnect to Tester

             eurusd = DataStore.Ticks();
             eurusd.createMqlTicks(int32(100));
        
%              self.setQuoteListener(eurusd);
             
            case 'X3L'
      
             self.startMT5('192.168.178.25',8228);    % Connnect to Live

             eurusd = DataStore.Ticks();
             eurusd.createMqlTicks(int32(100));
        
%              self.setQuoteListener(eurusd);

          case 'X2T'
      
             self.startMT5('192.168.178.11',8211);    % Connnect to Tester

             eurusd = DataStore.Ticks();
             eurusd.createMqlTicks(int32(100));
        
%              self.setQuoteListener(eurusd);
             
            case 'X2L'
      
             self.startMT5('192.168.178.11',8228);    % Connnect to Live

             eurusd = DataStore.Ticks();
             eurusd.createMqlTicks(int32(100));
        
%              self.setQuoteListener(eurusd);
             
          case 'default'
                          
      
             self.startMT5('127.0.0.1',8300);    % Connnect to default PC

             eurusd = DataStore.Ticks();
             eurusd.createMqlTicks(int32(100));
             
             self.setQuoteListener(eurusd);
          
      end
      
     if ~self.isApiConnected()
            connected = false;   
            return;
     end
          
     connected = true;
    end
    function stopX7Tester(self)
      self.cLogger.trace('Cleanup Listener ,Objects');
      
      self.hConnectionListener.delete;
      
     
      
      self.stopMT5;
      while self.isApiConnected()
         pause(0.1);
      end
      logging.clearLogger('ApiLogMain')
    end
  %% Event Listener    
    function r = setQuoteListener(self,tickHandle)
        r = addlistener(self.h, 'QuoteUpdate', @(src,event)self.quoteListener(src,event,tickHandle));
        self.hQuoteListener = r;
    end   
    function r = setQuoteAddedListener(self)
        r = addlistener(self.h, 'QuoteAdded', @self.quoteAddedListener);
        self.hQuoteAddListener = r;
    end
    function r = setConnectionListener(self)
        r = addlistener(self.h, 'ConnectionStateChanged', @(src,event)self.connectionListener(src,event,self.cLogger));
        self.hConnectionListener = r;
    end  
    function r = setDeInitListener(self)
             r = addlistener(self.h, 'OnTesterDeInit', @(src,event)self.OnDeInitListener(src,event,self.cLogger));
             self.hTesterDeInitListener = r;
    end  
    function r = setOnTradeTransactionListener(self)
        r = addlistener(self.h, 'OnTradeTransaction', @self.OnTradeTransactionListener);
        self.hOnTradeTransactionListener= r;
    end
    function setupDeInitListener(self)
                self.hOnTesterDeInitListener = addlistener(self.h, 'OnTesterDeInit', @(src,event)self.processOnDeInitEvent(src,event));  
                self.cLogger.trace('DeInit Listener set');
            end  
    function quoteAddedListener(event)
       askString = num2str(event.Quote.Ask,'%01.5f');
%          askString = sprintf(event.Quote.Ask,'%01.5f');
       bidString = num2str(event.Quote.Bid,'%01.5f');
%          bidString = num2str(event.Quote.Bid,'%01.5f');
          transmitString = strcat(askString,',',bidString);
         fprintf('%s: %s\n',char(event.Quote.Instrument),transmitString);
    end     
    function quoteListener(~,~,event,tickHandle)
%         askString = num2str(event.Quote.Ask,'%01.5f');
%         bidString = num2str(event.Quote.Bid,'%01.5f');
%         transmitString = strcat(askString,',',bidString);
%         fprintf('%s: %s\n',char(event.Quote.Instrument),transmitString);
      if ~feature('IsDebugMode')  
        tickHandle.storeNewQuoteEvent(event);
      end  
%         tickHandle.bid.push(event.Quote.Bid);

    end   
    function OnTradeTransactionListener(~,~,event)
           
%        askString = num2str(event.Quote.Ask,'%01.5f');
%          askString = sprintf(event.Quote.Ask,'%01.5f');
%        bidString = num2str(event.Quote.Bid,'%01.5f');
%          bidString = num2str(event.Quote.Bid,'%01.5f');
%           transmitString = strcat(askString,',',bidString);
%          fprintf('%s: %s\n',char(event.Deal),transmitString);
         
    end 
    function connectionListener(obj,~, event,logger)
      
        connectState = event.Status.ToString();
        
        obj.charConnectionState = connectState.char;
        
        logger.trace('New Connection State:');
        logger.info(char(connectState));        
        
         switch connectState.char
            
             case 'Connecting'
                 
             case 'Connected'
                 
             case 'Disconnected'
                 
                 logger.error('case Disconnected !!!');
                 
             case 'Failed'
                 
                logger.warn('Connection failed');
                logger.warn(sprintf('Reason: %s',char(event.ConnectionMessage)));
         end       
                      
    end
    function [ok] = processOnDeInitEvent(self,~,event)
             self.cLogger.trace('DeInit Event: Code 0');
%              disp(event)
             self.bStatusDeInit = true;
             
             ok=true;
            end   
  end
end

