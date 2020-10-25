classdef Tick < handle
    %   Tick Summary of this class goes here
    %   Detailed explanation goes here
    
    properties
        
      sdt_Time;         % Zeit des letzten Updates der Preise  
      d_Bid;            % Laufender Preis Bid 
      d_Ask;            % Laufender Preis Ask 
      d_Last;           % Laufender Preis des letzten Deals (Last) 
      uI_Volume;        % Volumen für laufenden Preis Last 
      I_Time_msc;       % Zeit der letzten Aktualisierung der Preise in Millisekunden 
      I_MtTime;         % Zeit der letzten Aktualisierung der Preise
      ui_Flags          % Tick-Flags 
      
        
    end
    
    methods
        
    function obj = Tick()
            
                   
       end      
   
    
    end
end

