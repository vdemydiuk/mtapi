classdef Position < handle
    %POSITION Summary of this class goes here
    %   Detailed explanation goes here
    
    properties
        POSITION_TIME            = datetime([],[],[]);
        POSITION_TICKET          = int64([]);    
        POSITION_TIME_MSC        = int64([]);
        POSITION_TIME_UPDATE     = int64([]);
        POSITION_TIME_UPDATE_MSC = int64([]);
        POSITION_TYPE            = MtApi5.ENUM_POSITION_TYPE;
        POSITION_MAGIC           = int64([]);
        POSITION_IDENTIFIER      = int64([]);
        POSITION_REASON          = MtApi5.ENUM_POSITION_REASON; 
        
        POSITION_VOLUME          = double([]);
        POSITION_PRICE_OPEN      = double([]);
        POSITION_SL              = double([]);
        POSITION_TP              = double([]);
        POSITION_PRICE_CURRENT   = double([]);
        POSITION_SWAP            = double([]);
        POSITION_PROFIT          = double([]);
               
        POSITION_SYMBOL          = char([]);
        POSITION_COMMENT         = char([]);        
    end
    
    methods
        function obj = Position()
            %POSITION Construct an instance of this class
            %   Detailed explanation goes here
            
        end
        
       
    end
end

