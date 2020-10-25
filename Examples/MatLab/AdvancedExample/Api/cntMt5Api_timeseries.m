classdef cntMt5Api_timeseries 
%% Properties
   properties
        cMt5Api;
        cApiResult;
        cLogger;
       
        hCharConnectionState;
        mSymbol char;
        mBars DataStore.Bars;
       
        debugCounter = uint32(0);
        hbDeInitStatus;
        
   end
 
   methods
%% Contructor
 
    function obj = cntMt5Api_timeseries(api)
        
        obj.cMt5Api = api;     
        obj.cLogger = api.cLogger; 
        obj.cLogger.trace('TimeSeries class created',0);
        obj.cApiResult = cntMt5Api_apiResult;
%         obj.hCharConnectionState = api.hConState;
%         obj.hbDeInitStatus = api.hbDEINITSTATUS;  
        
    end
%% Helper Functions

function connection_ok = isConnected(obj) 
      
    if  obj.cMt5Api.h.ConnectionState == MtApi5.Mt5ConnectionState.Connected
         connection_ok = true;
    else
         connection_ok = false;
    end        
end
function deinit = isDeInitSet(obj)
    
    deinit =  obj.cMt5Api.bStatusDeInit;
    if (deinit)
        disp(deinit)
    end
end

%% Api Functions

    function cApiResult = SeriesInfoInteger(self,symbolName, enum_TF, enum_propID)
        % int SeriesInfoInteger(string symbolName, ENUM_TIMEFRAMES timeframe, ENUM_SERIES_INFO_INTEGER propId)  
        %
        % Returns information about the state of historical data.
        % "symbolName" > Symbol name       
        % "timeframe"  > Period
        % "propId"     > Identifier of the requested property, value of the ENUM_SERIES_INFO_INTEGER enumeration
        cApiResult = cntMt5Api_apiResult;
         
        cApiResult.Int64 = self.cMt5Api.h.SeriesInfoInteger(symbolName, enum_TF, enum_propID);
         
        if isinteger(cApiResult.Int64)
            cApiResult.setSuccess(); 
        else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError('Error from Mt5');
        end
         
    end                  % Test ok
    function cApiResult = Bars(self,symbolName, enum_TF)
        % int Bars(string symbolName, ENUM_TIMEFRAMES timeframe)
        %
        % Returns information about the state of historical data.
        % "symbolName" > Symbol name       
        % "timeframe"  > Period

        cApiResult = cntMt5Api_apiResult;
         
        cApiResult.Int32 = self.cMt5Api.h.Bars(symbolName, enum_TF);
         
        if isinteger(cApiResult.Int32)
            cApiResult.setSuccess(); 
        else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError('Error from Mt5');
        end
         
    end                                            % Test ok
    function cApiResult = Bars2(self,symbolName, enum_TF, sDTstartTime, sDTstopTime)
        % int Bars(string symbolName, ENUM_TIMEFRAMES timeframe)
        %
        % Returns information about the state of historical data.
        % "symbolName" > Symbol name       
        % "timeframe"  > Period

        cApiResult = cntMt5Api_apiResult;
         
        cApiResult.Int32 = self.cMt5Api.h.Bars(symbolName, enum_TF, sDTstartTime, sDTstopTime );
         
        if isinteger(cApiResult.Int32)
            cApiResult.setSuccess(); 
        else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError('Error from Mt5');
        end
         
    end                % Test ok
    function cApiResult = CopyRates(self,symbolName, timeframe, startPos, count)
        %% int CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out MqlRates[] ratesArray)
        % Gets history data of MqlRates structure of a specified symbol-period in specified quantity into the ratesArray array.
        % The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        %
        % Returns information about the state of historical data.
        % "symbolName" > Symbol name       
        % "timeframe"  > Period
        % "startPos"   > The start position for the first element to copy.
        % "count"      > Data count to copy.
        % "ratesArray" > Array of MqlRates type
        cApiResult = cntMt5Api_apiResult;
        self.debugCounter = self.debugCounter+1;
        
        
        if ~self.isDeInitSet()
         
         
         try
%           disp('start api access') 
%           disp('STATUS:');
%            disp(self.cMt5Api.bStatusDeInit)
           
          [cApiResult.Int32, cApiResult.MqlRates] = self.cMt5Api.h.CopyRates(symbolName, timeframe, startPos, count );     
%           disp('access end');
          if isinteger(cApiResult.Int32)   && cApiResult.Int32 == count  && cApiResult.MqlRates.Length == count
              cApiResult.setSuccess(); 
          else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError(2,'Error from Mt5');
          end
          
          
         catch ME
            
            cApiResult.setError(1,"Try Catch failed ,No Connection");
            self.cLogger.error('Try Catch failed ,No Connection'); 
            
            
             disp(ME.stack(1))
            self.cLogger.error('.NET Execption'); 
                  
         end
        
        else
            cApiResult.setError(1,"No Connection");
            self.cLogger.error('No Connection'); 
 
        end
        
    end                    % Test ok
    function cApiResult = CopyRatesTimeCount(self,symbolName, timeframe, sDTstartTime, count)
        %% int CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out MqlRates[] ratesArray)
        % Gets history data of MqlRates structure of a specified symbol-period in specified quantity into the ratesArray array.
        % The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        %
        % Returns information about the state of historical data.
        % "symbolName" > Symbol name       
        % "timeframe"  > Period
        % "startTime"  > The start time for the first element to copy.
        % "count"      > Data count to copy.
        % "ratesArray" > Array of MqlRates type
        cApiResult = cntMt5Api_apiResult;
         
        [cApiResult.Int32, cApiResult.MqlRates] = self.cMt5Api.h.CopyRates(symbolName, timeframe, sDTstartTime, count );
         
        if isinteger(cApiResult.Int32)   && cApiResult.Int32 == count  && cApiResult.MqlRates.Length == count
            cApiResult.setSuccess(); 
        else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError('Error from Mt5');
        end
        
        
    end       % Test ok
    function cApiResult = CopyRatesTwoTimes(self,symbolName, timeframe, sDTstartTime, sDTstopTime)
        %% Gets history data of MqlRates structure of a specified symbol-period in specified quantity into the ratesArray array.
        %
        % The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        % Returns information about the state of historical data.
        % "symbolName" > Symbol name       
        % "timeframe"  > Period
        % "startTime"  > The start time for the first element to copy.
        % "stopTime"   > Bar time, corresponding to the last element to copy.
        % "ratesArray" > Array of MqlRates type
        
        cApiResult = cntMt5Api_apiResult;
         
        [cApiResult.Int32, cApiResult.MqlRates] = self.cMt5Api.h.CopyRates(symbolName, timeframe, sDTstartTime, sDTstopTime );
         
        if isinteger(cApiResult.Int32)  
            cApiResult.setSuccess(); 
        else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError('Error from Mt5');
        end
        
        
    end  % Test ok
    function cApiResult = CopyTime(self,symbolName, timeframe, value1, value2)
        %% int CopyTime(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out DateTime[] timeArray)
        %% int CopyTime(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out DateTime[] timeArray)
        %% int CopyTime(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out DateTime[] timeArray)       
        % The function gets to time_array history data of bar opening time for the specified symbol-period pair in the specified quantity.
        %
        % The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        % Returns information about the state of historical data.
        % "symbolName" > Symbol name       
        % "timeframe"  > Period
        % "count"      > Data count to copy
        % "startPos"   > The start position for the first element to copy
        % "startTime"  > The start time for the first element to copy
        % "stopTime"   > Bar time, corresponding to the last element to copy
        % "timeArray"  > Array of DatetTme type

            
        
        cApiResult = cntMt5Api_apiResult;
         
        [cApiResult.Int32, cApiResult.sDateTimes] = self.cMt5Api.h.CopyTime(symbolName, timeframe, value1, value2 );
         
        if isinteger(cApiResult.Int32)  
            cApiResult.setSuccess(); 
        else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError('Error from Mt5');
        end
        
        
    end                      % Test ok
    function cApiResult = CopyOpen(self,symbolName, timeframe, value1, value2)
        %% int CopyOpen(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out out double[] openArray)
        %% int CopyOpen(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out double[] openArray)
        %% int CopyOpen(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out double[] openArray)    
        % The function gets into open_array the history data of bar open prices for the selected symbol-period pair in the specified quantity
        %
        % The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        % Returns information about the state of historical data.
        % "symbolName" > Symbol name       
        % "timeframe"  > Period
        % "count"      > Data count to copy
        % "startPos"   > The start position for the first element to copy
        % "startTime"  > The start time for the first element to copy
        % "stopTime"   > Bar time, corresponding to the last element to copy
        % "openArray"  > Array of double type

            
        
        cApiResult = cntMt5Api_apiResult;
         
        [cApiResult.Int32, cApiResult.sDoubles] = self.cMt5Api.h.CopyOpen(symbolName, timeframe, value1, value2 );
         
        if isinteger(cApiResult.Int32) && cApiResult.Int32 > 0  
            cApiResult.setSuccess(); 
        else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError('Error from Mt5');
        end
        
        
    end                      % Test ok
    function cApiResult = CopyHigh(self,symbolName, timeframe, value1, value2)
        %% int CopyHigh(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out out double[] openArray)
        %% int CopyHigh(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out double[] openArray)
        %% int CopyHigh(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out double[] openArray)    
        % The function gets into high_array the history data of bar open prices for the selected symbol-period pair in the specified quantity
        %
        % The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        % Returns information about the state of historical data.
        % "symbolName" > Symbol name       
        % "timeframe"  > Period
        % "count"      > Data count to copy
        % "startPos"   > The start position for the first element to copy
        % "startTime"  > The start time for the first element to copy
        % "stopTime"   > Bar time, corresponding to the last element to copy
        % "openArray"  > Array of double type

            
        
        cApiResult = cntMt5Api_apiResult;
         
        [cApiResult.Int32, cApiResult.sDoubles] = self.cMt5Api.h.CopyHigh(symbolName, timeframe, value1, value2 );
         
        if isinteger(cApiResult.Int32) && cApiResult.Int32 > 0  
            cApiResult.setSuccess(); 
        else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError('Error from Mt5');
        end
        
        
    end                      % Test ok
    function cApiResult = CopyLow(self,symbolName, timeframe, value1, value2)
        %% int CopyLow(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out out double[] openArray)
        %% int CopyLow(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out double[] openArray)
        %% int CopyLow(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out double[] openArray)    
        % The function gets into low_array the history data of bar open prices for the selected symbol-period pair in the specified quantity
        %
        % The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        % Returns information about the state of historical data.
        % "symbolName" > Symbol name       
        % "timeframe"  > Period
        % "count"      > Data count to copy
        % "startPos"   > The start position for the first element to copy
        % "startTime"  > The start time for the first element to copy
        % "stopTime"   > Bar time, corresponding to the last element to copy
        % "openArray"  > Array of double type

            
        
        cApiResult = cntMt5Api_apiResult;
         
        [cApiResult.Int32, cApiResult.sDoubles] = self.cMt5Api.h.CopyLow(symbolName, timeframe, value1, value2 );
         
        if isinteger(cApiResult.Int32) && cApiResult.Int32 > 0  
            cApiResult.setSuccess(); 
        else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError('Error from Mt5');
        end
        
        
    end                       % Test ok
    function cApiResult = CopyClose(self,symbolName, timeframe, value1, value2)
        %% int CopyClose(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out out double[] openArray)
        %% int CopyClose(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out double[] openArray)
        %% int CopyClose(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out double[] openArray)    
        % The function gets into close_array the history data of bar open prices for the selected symbol-period pair in the specified quantity
        %
        % The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        % Returns information about the state of historical data.
        % "symbolName" > Symbol name       
        % "timeframe"  > Period
        % "count"      > Data count to copy
        % "startPos"   > The start position for the first element to copy
        % "startTime"  > The start time for the first element to copy
        % "stopTime"   > Bar time, corresponding to the last element to copy
        % "openArray"  > Array of double type

            
        
        cApiResult = cntMt5Api_apiResult;
         
        [cApiResult.Int32, cApiResult.sDoubles] = self.cMt5Api.h.CopyClose(symbolName, timeframe, value1, value2 );
         
        if isinteger(cApiResult.Int32) && cApiResult.Int32 > 0  
            cApiResult.setSuccess(); 
        else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError('Error from Mt5');
        end
        
        
    end                     % Test ok
    function cApiResult = CopyTickVolume(self,symbolName, timeframe, value1, value2)
        %% int CopyTickVolume(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out long[] volumeArray)
        %% int CopyTickVolume(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out long[] volumeArray)
        %% int CopyTickVolume(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out long[] volumeArray) 
        % The function gets into volume_array the history data of tick volumes for the selected symbol-period pair in the specified quantity
        %
        % The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        % Returns information about the state of historical data.
        % "symbolName" > Symbol name       
        % "timeframe"  > Period
        % "count"      > Data count to copy
        % "startPos"   > The start position for the first element to copy
        % "startTime"  > The start time for the first element to copy
        % "stopTime"   > Bar time, corresponding to the last element to copy
        % "volumeArray"  > Array of long type

            
        
        cApiResult = cntMt5Api_apiResult;
         
        [cApiResult.Int32, cApiResult.sLongs] = self.cMt5Api.h.CopyTickVolume(symbolName, timeframe, value1, value2 );
         
        if isinteger(cApiResult.Int32) && cApiResult.Int32 > 0  
            cApiResult.setSuccess(); 
        else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError('Error from Mt5');
        end
        
        
    end                % Test ok
    function cApiResult = CopyRealVolume(self,symbolName, timeframe, value1, value2)
        %% int CopyRealVolume(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out long[] volumeArray)
        %% int CopyRealVolume(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out long[] volumeArray)
        %% int CopyRealVolume(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out long[] volumeArray) 
        % The function gets into volume_array the history data of trade volumes for the selected symbol-period pair in the specified quantity
        %
        % The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        % Returns information about the state of historical data.
        % "symbolName" > Symbol name       
        % "timeframe"  > Period
        % "count"      > Data count to copy
        % "startPos"   > The start position for the first element to copy
        % "startTime"  > The start time for the first element to copy
        % "stopTime"   > Bar time, corresponding to the last element to copy
        % "volumeArray"  > Array of long type

            
        
        cApiResult = cntMt5Api_apiResult;
         
        [cApiResult.Int32, cApiResult.sLongs] = self.cMt5Api.h.CopyRealVolume(symbolName, timeframe, value1, value2 );
         
        if isinteger(cApiResult.Int32) && cApiResult.Int32 > 0  
            cApiResult.setSuccess(); 
        else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError('Error from Mt5');
        end
        
        
    end                % Test ok
    function cApiResult = CopySpread(self,symbolName, timeframe, value1, value2)
        %% int CopySpread(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out int[] spreadArray)
        %% int CopySpread(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out int[] spreadArray)
        %% int CopySpread(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out int[] spreadArray)
        % The function gets into spread_array the history data of spread values for the selected symbol-period pair in the specified quantity
        %
        % The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        % Returns information about the state of historical data.
        % "symbolName"   > Symbol name       
        % "timeframe"    > Period
        % "count"        > Data count to copy
        % "startPos"     > The start position for the first element to copy
        % "startTime"    > The start time for the first element to copy
        % "stopTime"     > Bar time, corresponding to the last element to copy
        % "spreadArray"  > Array of int type

            
        
        cApiResult = cntMt5Api_apiResult;
         
        [cApiResult.Int32, cApiResult.sInts] = self.cMt5Api.h.CopySpread(symbolName, timeframe, value1, value2 );
         
        if isinteger(cApiResult.Int32) && cApiResult.Int32 > 0  
            cApiResult.setSuccess(); 
        else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError('Error from Mt5');
        end
        
        
    end                    % Test ok
    function cApiResult = CopyTicks(self,symbolName, flags, from, count)
        %% List<MqlTick> CopyTicks(string symbolName, CopyTicksFlag flags = CopyTicksFlag.All, ulong from = 0, uint count = 0)
        % The function receives ticks in the MqlTick format into ticks_array. In this case, ticks are indexed from the past to the present, i.e. the 0 indexed tick is the oldest one in the array
        % For tick analysis, check the flags field, which shows what exactly has changed in the tick.
        % 
        % 
        % "symbolName"   > Symbol name       
        % "flags"        > The flag that determines the type of received ticks.     
        % "from"         > The date from which you want to request ticks.  In milliseconds since 1970.01.01. If from=0, the last count ticks will be returned
        % "count"        > The number of ticks that you want to receive. If the 'from' and 'count' parameters are not specified, all available recent ticks (but not more than 2000) will be written to result
        

            
        
        cApiResult = cntMt5Api_apiResult;
         
        cApiResult.sListMqlTick = self.cMt5Api.h.CopyTicks(symbolName, flags, from, count );
         
        if cApiResult.sListMqlTick.Count == count  
            cApiResult.setSuccess(); 
        else
                  self.cLogger.error('Error from Mt5 ');                
                  cApiResult.setError('Error from Mt5');
        end
        
        
    end                            % Test ok  
%% class functions
     
 % Event Listener
 
    function r = setQuoteListener(self,hBars)
        r = addlistener(self.cMt5Api.h, 'QuoteUpdate', @(src,event)self.quoteListener(src,event,hBars));
      
    end    
    function quoteListener(self,~,event,hBars)
        
    persistent errCount ;
    
    if ~feature('IsDebugMode')   
        
%       disp('Quote Update '); 
       
     if isempty(errCount)
         errCount = 0;
     end
     
     eTF = MtApi5.ENUM_TIMEFRAMES.PERIOD_M1;
     
         if self.mIsNewBar(eTF)
%             self.cLogger.info('New Bar detected');
            cApiResult = self.CopyRates(self.mSymbol,eTF, 0, 1);
            if errCount < 5
               if ~self.mUpdateBars(hBars,cApiResult.MqlRates(1))
                  errCount = errCount+1;
                  if errCount == 5
                      self.cLogger.error('Update Bars stopped while error count > 5');
                  end
               end
            end
                  
        end
    else
%         disp('Quote Update in Debug Mode'); 
    end   
  end
 % Data Management
 
   function [ok, bars] = mCreateBars(self,enTimeframe,chSymbol,iSize)
       % [ok, bars] = mCreateBars(MtApi5.ENUM_TIMEFRAMES ,char Symbol,int32 Size)
       
       bars = DataStore.Bars(chSymbol, enTimeframe, iSize);
       
       bars.chSymbol = chSymbol;
       bars.iSize = iSize;
       
       ok = true;
       
      
   end
   function ok   = mUpdateBars(self,hBars,hMqlRate)
     
     if hBars.iSize == hBars.iLast
         self.cLogger.error('Can not store more mqlRates, Bars Size Limit reached');
         ok = false;
         return
     end
     
     
     if ~isempty(hBars)
       idx = hBars.iLast;
     else
       idx = 0;
     end
     
     
     
     if idx > 0
%         hBars.mqlRates(idx+1) = hMqlRate;
        hBars.dOpen(idx+1)      = hMqlRate.open;
        hBars.dHigh(idx+1)      = hMqlRate.high;
        hBars.dLow(idx+1)       = hMqlRate.low;
        hBars.dClose(idx+1)     = hMqlRate.close;
        hBars.i64TickVol(idx+1) = hMqlRate.tick_volume;
        hBars.sdtTime(idx+1)    = hMqlRate.time;
        hBars.i64MTtime(idx+1)  = hMqlRate.mt_time;
        hBars.dtTime(idx+1)     = datetime(hMqlRate.mt_time , 'ConvertFrom', 'posixtime');
        
        hBars.iLast = idx+1;
     else
         hBars.sdOpen(1)= hMqlRate.open;
         
         hBars.iLast = 1;
     end
       
     ok = true;  
       
   end
   function ok   = fn_UpdateBars_big(self,hBars,hMqlRate)
     
     if hBars.eurusd.iSize == hBars.eurusd.iLast
         self.cLogger.error('Can not store more mqlRates, Bars Size Limit reached');
         ok = false;
         return
     end
     
     
     if ~isempty(hBars)
       idx = hBars.eurusd.iLast;
     else
       idx = 0;
     end
     
     
     
     if idx > 0
%         hBars.mqlRates(idx+1) = hMqlRate;
        hBars.dOpen(idx+1)      = hMqlRate.open;
        hBars.dHigh(idx+1)      = hMqlRate.high;
        hBars.dLow(idx+1)       = hMqlRate.low;
        hBars.dClose(idx+1)     = hMqlRate.close;
        hBars.i64TickVol(idx+1) = hMqlRate.tick_volume;
        hBars.sdtTime(idx+1)    = hMqlRate.time;
        hBars.i64MTtime(idx+1)  = hMqlRate.mt_time;
        hBars.dtTime(idx+1)     = datetime(hMqlRate.mt_time , 'ConvertFrom', 'posixtime');
        
        hBars.iLast = idx+1;
     else
         hBars.sdOpen(1)= hMqlRate.open;
         
         hBars.iLast = 1;
     end
       
     ok = true;  
       
   end 
   function [ok, bars] = mCreateFilledBars(self,chSymbol,enTimeframe,iStartPos,iCount,iSize)
    
      self.cLogger.debug('Create empty Bars' );  
      bars = DataStore.Bars(chSymbol, enTimeframe, iSize);
      self.cLogger.debug('Create ready' );  
     
      try
            
        self.cLogger.debug('API: copyrates...' );  
        [copied, bars.mqlRates] = self.cMt5Api.h.CopyRates(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);    
        self.cLogger.debug('API: ready' ); 
        
        
        if copied > 0
              
            self.cLogger.debug('Copy mqlRates to Bars System.Values...' );  
            for idx=1:1:copied
                
                bars.i64MTtime(idx) = bars.mqlRates(idx).mt_time;
                bars.dtTime(idx)    = datetime(bars.i64MTtime(idx) , 'ConvertFrom', 'posixtime');
%                 bars.sdOpen(idx)   = mqlRates(idx).open;
%                 bars.sdHigh(idx)   = mqlRates(idx).high;
%                 bars.sdLow(idx)    = mqlRates(idx).low;
%                 bars.sdClose(idx)  = mqlRates(idx).close;
            end
            
            bars.iLast = copied;
            
            self.cLogger.debug('Copy mqlRates ready' );  
                      
        else
          self.cLogger.error('CopyRates has 0 copied');  
          ok = false;
          return;
        end
 
        
 
        self.cLogger.debug('API: copy open,high,low,close,time,volume' );  
        
        
        [copiedOpen,  sdOpen]              = self.cMt5Api.h.CopyOpen(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);       
         bars.dOpen(1:iCount)              = double(sdOpen);
         
        [copiedHigh,  sdHigh]              = self.cMt5Api.h.CopyHigh(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);
         bars.dHigh(1:iCount)              = double(sdHigh);
         
        [copiedLow,   sdLow]               = self.cMt5Api.h.CopyLow(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);
         bars.dLow(1:iCount)               = double(sdLow);
         
        [copiedClose, sdClose]             = self.cMt5Api.h.CopyClose(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);
         bars.dClose(1:iCount)             = double(sdClose);
        
        [copiedSpread, si32Spread]         = self.cMt5Api.h.CopySpread(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);
         bars.i32Spread(1:iCount)          = int32(si32Spread);

        [copiedTickVol, si64TickVol]       = self.cMt5Api.h.CopyTickVolume(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);
         bars.i64TickVol(1:iCount)         = int64(si64TickVol);
  
        [copiedRealVol, si64RealVol]       = self.cMt5Api.h.CopyRealVolume(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);
         bars.i64RealVol(1:iCount)         = int64(si64RealVol);
         
        [copiedTime, sdtTime]              = self.cMt5Api.h.CopyTime(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);
        
        sdtTime.CopyTo(bars.sdtTime,0);
        


          self.cLogger.debug('API: Copy OHLC ready' );  


      catch ME
          
           if(isa(ME, 'NET.NetException'))
             
             BE = ME.ExceptionObject.GetBaseException;
          
             self.cLogger.error(sprintf('Matlab Execption:  %s',char(ME.message)));
             self.cLogger.error(sprintf('Base   Execption:  %s',char(BE.StackTrace)));
           
           elseif(isa(ME, 'MException'))
             
               disp(ME.message);
           else
             error('Unknown Exeption Type');
           end
      
      
          ok = false;
          return;
      end
      
      
      
      
      if bars.iLast     == iCount &&...
          copiedOpen    == iCount &&...
          copiedHigh    == iCount &&...
          copiedLow     == iCount &&...
          copiedClose   == iCount &&...
          copiedTime    == iCount &&...
          copiedSpread  == iCount &&...
          copiedTickVol == iCount &&...
          copiedRealVol == iCount 

          bars.sdtStart = bars.mqlRates(1).time;
          bars.sdtEnd   = bars.mqlRates(bars.iLast).time;
          
          bars.sStart   = char(bars.sdtStart.ToString);
          bars.sEnd     = char(bars.sdtEnd.ToString);
          
          self.cLogger.debug(sprintf('%s Bars object created',bars.chSymbol));
          self.cLogger.debug(sprintf('Size: %d Elements',bars.iSize));
          self.cLogger.debug(sprintf('Last: %d Element',bars.iLast));
          self.cLogger.debug(sprintf('Start: %s ',char(bars.sdtStart.ToString)));
          self.cLogger.debug(sprintf('End  : %s ',char(bars.sdtEnd.ToString)));
          
         ok = true;
      else
          self.cLogger.error(sprintf('Creating Bars Object :%s ',"Error"));
          ok = false;
      end
      
     
   end          % Test ok 
   function [ok, bars] = mCreateFilledBarsPARALEL(self,chSymbol,enTimeframe,iStartPos,iCount,iSize)
       
      self.cLogger.debug('Parfor Mode' );  
      
      self.cLogger.debug('Create empty Bars' );  
      bars = DataStore.Bars(chSymbol, enTimeframe, iSize);
      self.cLogger.debug('Create ready' );  
      
      try
            
        self.cLogger.debug('API: copyrates...' );  
        [copied, bars.mqlRates] = self.cMt5Api.h.CopyRates(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);    
        self.cLogger.debug('API: ready' ); 
        
        
        if copied > 0
              
            self.cLogger.debug('Copy mqlRates to Bars System.Values...' );  
            
            parRes1 = zeros(1,copied);
            parRes2 = zeros(1,copied);
            parfor idx=1:1:copied
                
                parRes1(idx)    = bars.mqlRates(idx).mt_time;
                parRes2(idx)    = datetime(parRes1(idx) , 'ConvertFrom', 'posixtime');
%                 bars.sdOpen(idx)   = mqlRates(idx).open;
%                 bars.sdHigh(idx)   = mqlRates(idx).high;
%                 bars.sdLow(idx)    = mqlRates(idx).low;
%                 bars.sdClose(idx)  = mqlRates(idx).close;
            end
            
%               bars.i64MTtime(idx)
%             bars.dtTime(idx)
            bars.iLast = copied;
            
            self.cLogger.debug('Copy mqlRates ready' );  
                      
        else
          self.cLogger.error('CopyRates has 0 copied');  
          ok = false;
          return;
        end
 
        
 
        self.cLogger.debug('API: copy open,high,low,close,time,volume' );  
        
        
        [copiedOpen,  sdOpen]              = self.cMt5Api.h.CopyOpen(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);       
         bars.dOpen(1:iCount)              = double(sdOpen);
         
        [copiedHigh,  sdHigh]              = self.cMt5Api.h.CopyHigh(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);
         bars.dHigh(1:iCount)              = double(sdHigh);
         
        [copiedLow,   sdLow]               = self.cMt5Api.h.CopyLow(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);
         bars.dLow(1:iCount)               = double(sdLow);
         
        [copiedClose, sdClose]             = self.cMt5Api.h.CopyClose(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);
         bars.dClose(1:iCount)             = double(sdClose);
        
        [copiedSpread, si32Spread]         = self.cMt5Api.h.CopySpread(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);
         bars.i32Spread(1:iCount)          = int32(si32Spread);

        [copiedTickVol, si64TickVol]       = self.cMt5Api.h.CopyTickVolume(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);
         bars.i64TickVol(1:iCount)         = int64(si64TickVol);
  
        [copiedRealVol, si64RealVol]       = self.cMt5Api.h.CopyRealVolume(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);
         bars.i64RealVol(1:iCount)         = int64(si64RealVol);
         
        [copiedTime, sdtTime]              = self.cMt5Api.h.CopyTime(bars.chSymbol, bars.enTimeframe, iStartPos, iCount);
        
        sdtTime.CopyTo(bars.sdtTime,0);
        


          self.cLogger.debug('Copy OHLC ready' );  


      catch ME
          
           if(isa(ME, 'NET.NetException'))
             
             BE = ME.ExceptionObject.GetBaseException;
          
             self.cLogger.error(sprintf('Matlab Execption:  %s',char(ME.message)));
             self.cLogger.error(sprintf('Base   Execption:  %s',char(BE.StackTrace)));
           
           elseif(isa(ME, 'MException'))
             
               disp(ME.message);
           else
             error('Unknown Exeption Type');
           end
      
      
          ok = false;
          return;
      end
      
      
      
      
      if bars.iLast     == iCount &&...
          copiedOpen    == iCount &&...
          copiedHigh    == iCount &&...
          copiedLow     == iCount &&...
          copiedClose   == iCount &&...
          copiedTime    == iCount &&...
          copiedSpread  == iCount &&...
          copiedTickVol == iCount &&...
          copiedRealVol == iCount 

          bars.sdtStart = bars.mqlRates(1).time;
          bars.sdtEnd   = bars.mqlRates(bars.iLast).time;
          
          bars.sStart   = char(bars.sdtStart.ToString);
          bars.sEnd     = char(bars.sdtEnd.ToString);
          
          self.cLogger.debug(sprintf('%s Bars object created',bars.chSymbol));
          self.cLogger.debug(sprintf('Size: %d Elements',bars.iSize));
          self.cLogger.debug(sprintf('Last: %d Element',bars.iLast));
          self.cLogger.debug(sprintf('Start: %s ',char(bars.sdtStart.ToString)));
          self.cLogger.debug(sprintf('End  : %s ',char(bars.sdtEnd.ToString)));
          
         ok = true;
      else
          self.cLogger.error(sprintf('Creating Bars Object :%s ',"Error"));
          ok = false;
      end
      
     
   end     
   function         ok = mGetAndStoreNewBar(self,hBars)
    ok = false;
    eTF = MtApi5.ENUM_TIMEFRAMES.PERIOD_M1;
       
    cApiResult = self.CopyRates(self.mSymbol,eTF, 0, 1);
    
    if cApiResult.isSuccess()
        
      if self.mUpdateBars(hBars,cApiResult.MqlRates(1))
%         self.cLogger.trace('New Bar added to Bars Object');
        ok = true;
      else
         self.cLogger.error('Can`t add new Bar to Bars Object');
         ok = false;
      end 
        
     end     
   end  
   function         ok = fn_GetAndStoreNewBar_multisymbols(self,hBars,opts)
    ok = false;
    eTF = MtApi5.ENUM_TIMEFRAMES.PERIOD_M1;
    
    max = opts.symbols.num ;
    
    for idx=1:1:max
        
        cApiResult = self.CopyRates((upper(opts.symbols.Chars{idx})),eTF, 0, 1);

        if cApiResult.isSuccess()

          if self.mUpdateBars(hBars.(opts.symbols.Chars{idx}),cApiResult.MqlRates(1))
    %         self.cLogger.trace('New Bar added to Bars Object');
            ok = true;
          else
             self.cLogger.error('Can`t add new Bar to Bars Object');
             ok = false;
          end 

        end     
    end  
   end
   function         ok = mIsNewBar(self,enum_TF)
       
     ok = false;
     persistent I_LastBarTime;
     
     
     
     I_CurrentBarTime = self.cMt5Api.h.SeriesInfoInteger(self.mSymbol, enum_TF, MtApi5.ENUM_SERIES_INFO_INTEGER.SERIES_LASTBAR_DATE);
     
     
       if isempty(I_LastBarTime)      
           I_LastBarTime = I_CurrentBarTime;
%            disp('First Init of LastBarTime');
           return 
       end
     
       if I_CurrentBarTime > I_LastBarTime
          I_LastBarTime = I_CurrentBarTime;
          ok = true;
          return
       else
           I_LastBarTime = I_CurrentBarTime;
       end
     

   
   end                                                        % Test ok
   
   
   end
end