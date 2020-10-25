classdef cntMt5Api_apiResult < handle
    %% stores and manage date result from Mt5Api
    
    properties
        cLogger;                                % logger
        Status               = false;           % bool
        Bool                 = [];              % bool
        Double               = [];              % double
        sDoubles             = [];              % System.Double
        Int32                = [];              % int32
        UInt32               = [];              % uint32
        sInts                = [];              % System.Int
        Int64                = [];              % int64
        sLongs               = [];              % System.Long
        String               = [];              % System.String
        errString            = [];              % string
        lastError            = uint32(0);       % int32              1=No Connection / 2=Error MT5   
        lastErrorString      = "";              % string
        sDateTime            = System.DateTime; % System.DateTime
        sDTfrom              = System.DateTime; % System.DateTime
        sDTto                = System.DateTime; % System.DateTime
        sDateTimes           = [];
        sListMqlTick         = [];              % System.List*MqlTick
        MqlTick              = MtApi5.MqlTick;
        MqlBook              = MtApi5.MqlBookInfo(MtApi5.ENUM_BOOK_TYPE.BOOK_TYPE_BUY,0,0);
        MqlTradeCheckResult  = MtApi5.MqlTradeCheckResult(0,0,0,0,0,0,0,'emty')
        MqlTradeRequest      = MtApi5.MqlTradeRequest; 
        MqlTradeResult       = MtApi5.MqlTradeResult(0,0,0,0,0,0,0,'emty',0);
        MqlRates             = NET.createArray('MtApi5.MqlBookInfo',1);

    end
   
    methods
%% constructor        
        
        function obj = cntMt5Api_apiResult()
            obj.errString = 'No Error';
        end  
        
%% set functions 

        function setSuccess(obj)
            obj.Status = true;
        end       
        function setError(obj,varargin)
            obj.Status = false;
            if nargin > 1 
                
                if isnumerictype(varargin{1})
                    obj.lastError = varargin{1};
                end
            end
           if nargin > 2
               
                if ischar(varargin{2})
                 obj.errString = varargin{2};
                end
                             
            end
        end  
        
%% get functions

        function result_ok = isSuccess(self)
            result_ok = false;
            if self.Status == true
               result_ok = true;
            end
        end
        function result_ok = isError(self)
            result_ok = false;
            if self.Status == false
               result_ok = true;
            end
        end
        function isError   = getLastError(self,apiHandle)
            
            isError = false;
            
            cApiResult = apiHandle.cCheckup.GetLastError();
            self.lastError = cApiResult.Int32;
            
            if self.lastError == 0
                self.lastErrorString = "No Error";
            else
                self.lastErrorString = sprintf("Unknown Error: %d ",self.lastError);
                isError = true;
            end
        end
        
%% result convert functions  

        function charResult = MqlTradeResult_toChar(self)
            charResult =  char(self.MqlTradeResult.ToString);
        end
        function charResult = MqlTradeCheckResult_toChar(self)
            charResult =  char(self.MqlTradeCheckResult.ToString);
        end
        function charResult = MqlTradeRequest_toChar(self)
            charResult =  char(self.MqlTradeRequest.ToString);
        end
        function strResult = Int32_toString(self)
            strResult =  int2str(self.Int32);
        end
        function strResult = Int64_toString(self)
            strResult =  int2str(self.Int64);
        end
        function strResult = Double_toString(self)
%             disp(self.Double);
            strResult =  num2str(self.Double,6);
        end

 
    end
end