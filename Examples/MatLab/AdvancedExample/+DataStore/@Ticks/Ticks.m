classdef Ticks < handle
    %   Ticks Summary of this class goes here
    %   Detailed explanation goes here
    
    properties
        
    mqlTicks;      % DataStore.Tick Handles 
      
    I_Size   = 0;  % Anzahl der Ticks
        
      
       
    end
    
    methods
        
       function obj = Ticks()
            
                   
       end 
       
       function ok =  createMqlTicks(self,i_Count)
           
           if isinteger(i_Count)
               
             newticks(i_Count) = DataStore.Tick();
             self.mqlTicks     = newticks;
%              self.I_Size       = i_Count;
             ok = true;
           else
             warning('Count must be Integer');
             ok = false;  
           end
           
       end
       
       function ok = fillMqlTicks(self, I_Count, MqlTicks)
           
           if self.I_Size == 0 
               
               
           
                ok = true;
           else
                warning('mqlTicks object size must be 0')
                ok = false;
           end
          
       end
       
       function ok = storeNewMqlTick(self,MqlTick)
        
        
        I_newsize = self.I_Size+1;
        
        self.mqlTicks(I_newsize).d_Bid = MqlTick.bid;
        self.mqlTicks(I_newsize).d_Ask = MqlTick.ask;
        
        self.mqlTicks(I_newsize).I_MtTime  = MqlTick.MtTime;
        self.mqlTicks(I_newsize).sdt_Time  = MqlTick.time;
        
        self.I_Size = I_newsize;
        
        ok = true;   
       end
       function ok = storeNewQuoteEvent(self,Event)
        
        
        I_newsize = self.I_Size+1;
        
        self.mqlTicks(I_newsize).d_Bid = Event.Quote.Bid;
        self.mqlTicks(I_newsize).d_Ask = Event.Quote.Ask;
        
%         self.mqlTicks(I_newsize).I_MtTime  = MqlTick.MtTime;
%         self.mqlTicks(I_newsize).sdt_Time  = MqlTick.time;
        
        self.I_Size = I_newsize;
        
%         fprintf('%d Ticks stored \n',self.I_Size); 
%         fprintf('Bid: %d  Ask: %d  \n',Event.Quote.Bid , Event.Quote.Ask);
        
        ok = true;   
       end   
    end
end

