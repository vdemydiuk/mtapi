classdef TradingData < handle
    %TRADINGDATA Summary of this class goes here
    %   Detailed explanation goes here
    
    properties
        core_trades
        net_signal
        net_class
        net_score
        net_input
               
    end
    
    properties (SetAccess=protected)
        size
        cnt
    end
    
    methods
        function obj = TradingData()
            %TRADINGDATA Construct an instance of this class
            %   Create datastore for logging all data  
            
        end
        
        function ok = setup(self,size,sizeTrades)
            %SETUP Summary of this method goes here
            %   Setup Object 
            %  args: 
            %  1 = Array Size (Bars)
            %  2 = Array Size Trades (default 1000)
            
            self.size = size;
            
            self.core_trades = zeros(sizeTrades,4);
            self.net_signal  = zeros(size,1);
            self.net_signal  = zeros(size,1);
            self.net_score   = zeros(size,4,'single');
            
            a = repmat([0],3,500);
            self.net_input = repmat({a},size,1);
            
            classArray(size,1)  = categorical(0);
            self.net_class = classArray;
            ok = true;       
        end
        function set_Count(self,cnt)
            self.cnt = cnt;
        end
        function store_NetInput(self,data)
            self.net_input{self.cnt,1} = data{1,1};
        end
        function store_NetOutSignal(self,data)
            self.net_signal(self.cnt) = data;
        end
        function store_NetOutClass(self,data)
            self.net_class(self.cnt,1) = data;
        end
        function store_NetOutScore(self,data)
            size = length(data);
            self.net_score(self.cnt,1:size) = data(:);
        end
        
        function reorganizeData(self)
            
            
            
            
            
        end
% Getters
function data = get_Data(self)
    
end
   

    end
end

