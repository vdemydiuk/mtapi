classdef Bars < handle
    %BARS Summary of this class goes here
    %   Detailed explanation goes here
    
    properties
        
        
        chSymbol    = 'EURUSD'     ; 
        iSize       = int32(0)     ;
        iLast       = int32(0)     ;
        sStart      = ""           ;
        sEnd        = ""           ;
        sdtStart    = System.DateTime.Now;
        sdtEnd      = System.DateTime.Now;
        enTimeframe = MtApi5.ENUM_TIMEFRAMES;

        mqlRates    = NET.createArray('MtApi5.MqlRates', 0);                   
        sdOpen      = NET.createArray('System.Double[]', 3)   ;
        sdHigh      = NET.createArray('System.Double[]', 3)   ;       
        sdLow       = NET.createArray('System.Double[]', 3)   ;       
        sdClose     = NET.createArray('System.Double[]', 3)   ;          
        sdtTime     = NET.createArray('System.DateTime[]', 3)   ;  
        si32Spread  = NET.createArray('System.Int32[]', 3)   ;   
        si64TickVolume = NET.createArray('System.Int64[]', 3)   ; 
        si64RealVolume = NET.createArray('System.Int64[]', 3)   ; 
        si64RealVolumeH = NET.createArray('System.Int64[]', 3)   ; 
        si64RealVolumeL = NET.createArray('System.Int64[]', 3)   ; 
       
        dOpen       = double(0);
        dHigh       = double(0);   
        dLow        = double(0);
        dClose      = double(0);
        i64MTtime   = int64(0) ;
        dtTime      = datetime();
        i32Spread   = int32(0) ;
        i64TickVol  = int64(0) ;
        i64RealVol  = int64(0) ;
        i64RealVolH = int64(0) ;
        i64RealVolL = int64(0) ;
    end
    
    methods
        
        function obj = Bars(chSymbol,enTF,iSize)

              switch nargin
                  case 0
                      obj.chSymbol = 'EURUSD'; 
                      obj.iSize = 1000;
                      obj.enTimeframe = MtApi5.ENUM_TIMEFRAMES.PERIOD_M1;
                      return
                      
                  case 1
                  case 2
                  case 3
                      
                       obj.chSymbol = chSymbol;
                       obj.iSize = iSize;
                       obj.enTimeframe = enTF;
                      
                  otherwise
              end
  
          
                 
              

           
                
              

              obj.mqlRates = NET.createArray('MtApi5.MqlRates', obj.iSize);


              obj.sdOpen  = NET.createArray('System.Double', obj.iSize);
              obj.sdHigh  = NET.createArray('System.Double', obj.iSize);
              obj.sdLow   = NET.createArray('System.Double', obj.iSize);
              obj.sdClose = NET.createArray('System.Double', obj.iSize);

              obj.sdtTime = NET.createArray('System.DateTime', obj.iSize);

              obj.si32Spread     = NET.createArray('System.Int32', obj.iSize);
              obj.si64TickVolume = NET.createArray('System.Int64', obj.iSize);
              obj.si64RealVolume = NET.createArray('System.Int64', obj.iSize);

              obj.sdtStart   = System.DateTime.Now;
              obj.sdtEnd     = System.DateTime.Now;

              obj.sStart     = char(obj.sdtStart.ToString);
              obj.sEnd       = char(obj.sdtStart.ToString);

              obj.enTimeframe = enTF;


              obj.dOpen  = zeros(obj.iSize,1,'double') ;
              obj.dHigh  = zeros(obj.iSize,1,'double') ;
              obj.dLow   = zeros(obj.iSize,1,'double') ;
              obj.dClose = zeros(obj.iSize,1,'double') ;

              obj.i32Spread    = zeros(obj.iSize,1,'int32') ; 
              obj.i64TickVol   = zeros(obj.iSize,1,'int64') ; 
              obj.i64RealVol   = zeros(obj.iSize,1,'int64') ;
              obj.i64RealVolH  = zeros(obj.iSize,1,'int64') ;
              obj.i64RealVolL  = zeros(obj.iSize,1,'int64') ;
              obj.i64MTtime    = zeros(obj.iSize,1,'int64') ;


              obj.dtTime(iSize,1)    = datetime;

              obj.dtTime.Format = 'default';



        end      
        function setMqlRates(self,MqlRates)

            self.MqlRates = MqlRates;

        end
        function saveDataAsByteStream(self,filename)

            mc = ?DataStore.Bars;

            propList = mc.PropertyList;
            propCnt  = length(propList);

            for i=1:1:propCnt
                bytestream.(propList(i).Name) = [];
            end
            formatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;

            for idx=1:1:propCnt

             if isa(propList(idx).DefaultValue,'System.Object')   
               stream = System.IO.MemoryStream;
               formatter.Serialize(stream,self.(propList(idx).Name));
               data = uint8(stream.ToArray);

               bytestream.(propList(idx).Name) = data;
             else
    %            stream = System.IO.MemoryStream;
    %            formatter.Serialize(stream,self.(propList(idx).Name));
    %            data = uint8(stream.ToArray);

               bytestream.(propList(idx).Name) = self.(propList(idx).Name);  
             end



            end

            save(filename,'bytestream');

        end
        function ok = loadDataAsByteStream(self,filename)
            ok = false;
            load(filename);
            mc = ?DataStore.Bars;

            propList = mc.PropertyList;
            propCnt  = length(propList);


            formatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;

            for idx=1:1:propCnt

             if isa(propList(idx).DefaultValue,'System.Object') 
               data = bytestream.(propList(idx).Name);  
               stream = System.IO.MemoryStream(data);
               self.(propList(idx).Name) = formatter.Deserialize(stream);            
             else
                data = bytestream.(propList(idx).Name);  
                self.(propList(idx).Name) = data;  
             end



            end
            ok = true;
        end
        function saveableData = Get_Saveable_MATfileData(self,varargin)



            switch nargin

                case 1 % no options

                    first = 1;
                    last = self.iLast;

                case 2 % first = 1 , last = input

                    first = 1;
                    last = varargin{2};

                case 3 

                    first = varargin{1};
                    last  = varargin{2};

            end


            if last > self.iLast
                last = self.iLast;
                warn('Index is out of array')
                warn('Using iLast for last')
            end





            saveableData.chSymbol   = self.chSymbol;
            saveableData.iSize      = self.iLast;
            saveableData.iLast      = self.iLast; 
            saveableData.sStart     = self.sStart; 
            saveableData.sEnd       = self.sEnd;

            saveableData.dOpen      = self.dOpen(first:last,1);       
            saveableData.dHigh      = self.dHigh(first:last,1); 
            saveableData.dLow       = self.dLow(first:last,1); 
            saveableData.dClose     = self.dClose(first:last,1);    
            saveableData.dtTime     = self.dtTime(first:last,1); 
            saveableData.i64MTtime  = self.i64MTtime(first:last,1); 
            saveableData.i32Spread  = self.i32Spread(first:last,1); 
            saveableData.i64TickVol = self.i64TickVol(first:last,1); 
            saveableData.i64RealVol = self.i64RealVol(first:last,1); 



        end
        function tt= Get_TimeTable(self,varargin)


            switch nargin

                case 1 % no options

                    first = 1;
                    last = self.iLast;

                case 2 % first = 1 , last = input

                    first = 1;
                    last = varargin{2};

                case 3 

                    first = varargin{1};
                    last  = varargin{2};

            end


            if last > self.iLast
                last = self.iLast;
                warn('Index is out of array')
                warn('Using iLast for last')
            end

            Open      = self.dOpen(first:last,1);       
            High      = self.dHigh(first:last,1); 
            Low       = self.dLow(first:last,1); 
            Close     = self.dClose(first:last,1);    
            Time      = self.dtTime(first:last,1); 
            TicklVol   = self.i64TickVol(first:last,1); 





            tt = timetable(Time,Open,High,Low,Close,TicklVol);

            tt.Properties.VariableNames = {'Open','High','Low','Close','TickVol'};

        end
    
    end
end

