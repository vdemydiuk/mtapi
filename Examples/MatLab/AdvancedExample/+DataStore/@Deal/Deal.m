classdef Deal < handle
      %DEAL Summary of this class goes here
      %   Detailed explanation goes here
      
      properties
       % Integer
       DEAL_TICKET      = int32([]);               % Das Ticket des Trades. Das ist eine einmalige Nummer, die jedem Trade zugewiesen wird.
       DEAL_ORDER       = int32([]);               % Order, auf deren Grund der Deal abgeschlossen wurde 
       DEAL_TIME        = datetime([],[],[]);      % Zeit des Dealabschlusses 
       DEAL_TIME_MSC    = int32([])                % Zeitpunkt der Transaktion in Millisekunden seit 01.01.1970
       DEAL_TYPE        = MtApi5.ENUM_DEAL_TYPE;   % Typ des Deals
       DEAL_ENTRY       = MtApi5.ENUM_DEAL_ENTRY;  % Dealsrichtung - Markteingang, Marktausgang oder Kehrwendung 
       DEAL_MAGIC       = int32([])                % Magic number für Deal  (sehen Sie ORDER_MAGIC)
       DEAL_REASON      = MtApi5.ENUM_DEAL_REASON; % Grund oder Ursprung der Ausführung eines Abschlusses
       DEAL_POSITION_ID = int32([])                % Indetifikator der Position, an deren Öffnung, Veränderung oder Schliessung sich der Deal  teilnahm. Jede Position hat ihren unikalen Identifikator, der allen Deals zugeordnet wird, die im Instrument innerhalb des ganzen Lebens der Position abgeschlossen wurde. 

       % Double
       DEAL_VOLUME      = double([]);              % Dealvolumen
       DEAL_PRICE       = double([]);              % Dealpreis
	   DEAL_COMMISSION  = double([]);              % Dealkommission
       DEAL_SWAP        = double([]);              % Gesamtswap beim Schliessen 
       DEAL_PROFIT      = double([]);              % finanzielles Ergebnis des Deals

       % String
       DEAL_SYMBOL      = char([]);                % Dealssymbol
       DEAL_COMMENT     = char([]);                % Kommentar zum Deal
       DEAL_EXTERNAL_ID = char([]);                % Identifikator des Deals im Außenhandelssystem (an der Börse)

      end
      
      methods
            function obj = Deal()
                  %   DEAL Construct an instance of this class
                  %   Detailed explanation goes here
                  %   Create an empty Deal Object
                  
            end
            
            function outputArg = method1(obj,inputArg)
                  %METHOD1 Summary of this method goes here
                  %   Detailed explanation goes here
                  outputArg = obj.Property1 + inputArg;
            end
      end
end

