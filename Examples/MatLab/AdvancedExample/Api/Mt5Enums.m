classdef (Sealed) Mt5Enums 
%         properties
%           cMt5Api;
%         end
%     function obj = cntMtApi_enums(api)
%         obj.cMt5Api = api;
%     end
% %% Constructor

   properties (Constant)
      %% ENUM_TIMEFRAMES
      % ok
        TF_CURRENT = MtApi5.ENUM_TIMEFRAMES.PERIOD_CURRENT;
        TF_M1      = MtApi5.ENUM_TIMEFRAMES.PERIOD_M1;
        TF_M2      = MtApi5.ENUM_TIMEFRAMES.PERIOD_M2;
        TF_M3      = MtApi5.ENUM_TIMEFRAMES.PERIOD_M3;
        TF_M4      = MtApi5.ENUM_TIMEFRAMES.PERIOD_M4;
        TF_M5      = MtApi5.ENUM_TIMEFRAMES.PERIOD_M5;
        TF_M6      = MtApi5.ENUM_TIMEFRAMES.PERIOD_M6;
        TF_M10     = MtApi5.ENUM_TIMEFRAMES.PERIOD_M10;
        TF_M12     = MtApi5.ENUM_TIMEFRAMES.PERIOD_M12;
        TF_M15     = MtApi5.ENUM_TIMEFRAMES.PERIOD_M15;
        TF_M20     = MtApi5.ENUM_TIMEFRAMES.PERIOD_M20;
        TF_M30     = MtApi5.ENUM_TIMEFRAMES.PERIOD_M30;
        TF_H1      = MtApi5.ENUM_TIMEFRAMES.PERIOD_H1;
        TF_H2      = MtApi5.ENUM_TIMEFRAMES.PERIOD_H2;
        TF_H3      = MtApi5.ENUM_TIMEFRAMES.PERIOD_H3;
        TF_H4      = MtApi5.ENUM_TIMEFRAMES.PERIOD_H4;
        TF_H6      = MtApi5.ENUM_TIMEFRAMES.PERIOD_H6;
        TF_H8      = MtApi5.ENUM_TIMEFRAMES.PERIOD_H8;
        TF_H12     = MtApi5.ENUM_TIMEFRAMES.PERIOD_H12;
        TF_D1      = MtApi5.ENUM_TIMEFRAMES.PERIOD_D1;
        TF_W1      = MtApi5.ENUM_TIMEFRAMES.PERIOD_W1;
        TF_MN1     = MtApi5.ENUM_TIMEFRAMES.PERIOD_MN1;  
      %% ENUM_TERMINAL_INFO_INTEGER
    
        TER_BUILD                 = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_BUILD;
        TER_COMMUNITY_ACCOUNT     = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_COMMUNITY_ACCOUNT;
        TER_COMMUNITY_CONNECTION  = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_COMMUNITY_CONNECTION;
        TER_CONNECTED             = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_CONNECTED;
        TER_DLLS_ALLOWED          = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_DLLS_ALLOWED;
        TER_TRADE_ALLOWED         = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_TRADE_ALLOWED;
        TER_EMAIL_ENABLED         = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_EMAIL_ENABLED;
        TER_FTP_ENABLED           = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_FTP_ENABLED;
        TER_NOTIFICATIONS_ENABLED = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_NOTIFICATIONS_ENABLED;
        TER_MAXBARS               = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_MAXBARS;
        TER_MQID                  = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_MQID;
        TER_CODEPAGE              = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_CODEPAGE;
        TER_CPU_CORES             = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_CPU_CORES;
        TER_DISK_SPACE            = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_DISK_SPACE;
        TER_MEMORY_PHYSICAL       = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_MEMORY_PHYSICAL
        TER_MEMORY_TOTAL          = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_MEMORY_TOTAL
        TER_MEMORY_AVAILABLE      = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_MEMORY_AVAILABLE
        TER_MEMORY_USED           = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_MEMORY_USED
        TER_X64                   = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_X64
        TER_OPENCL_SUPPORT        = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_OPENCL_SUPPORT
        TER_SCREEN_DPI            = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_SCREEN_DPI
        TER_PING_LAST             = MtApi5.ENUM_TERMINAL_INFO_INTEGER.TERMINAL_PING_LAST
      %% ENUM_TERMINAL_INFO_DOUBLE  
      
        TER_COMMUNITY_BALANCE = MtApi5.ENUM_TERMINAL_INFO_DOUBLE.TERMINAL_COMMUNITY_BALANCE; 
      %% ENUM_TERMINAL_INFO_STRING
    
        TER_LANGUAGE        = MtApi5.ENUM_TERMINAL_INFO_STRING.TERMINAL_LANGUAGE;
        TER_COMPANY         = MtApi5.ENUM_TERMINAL_INFO_STRING.TERMINAL_COMPANY;
        TER_NAME            = MtApi5.ENUM_TERMINAL_INFO_STRING.TERMINAL_NAME;
        TER_PATH            = MtApi5.ENUM_TERMINAL_INFO_STRING.TERMINAL_PATH;
        TER_DATA_PATH       = MtApi5.ENUM_TERMINAL_INFO_STRING.TERMINAL_DATA_PATH;
        TER_COMMONDATA_PATH = MtApi5.ENUM_TERMINAL_INFO_STRING.TERMINAL_COMMONDATA_PATH;   
      %% ENUM_SYMBOL_INFO_INTEGER
    
        SYMBOL_CUSTOM = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_CUSTOM;
        SYMBOL_BACKGROUND_COLOR = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_BACKGROUND_COLOR;
        SYMBOL_CHART_MODE = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_CHART_MODE;
        SYMBOL_SELECT = 0,
        % FIXME: SYMBOL_VISIBLE not found in MQL5 environment!
        % SYMBOL_VISIBLE = ?
        SYMBOL_SESSION_DEALS = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_SESSION_DEALS;
        SYMBOL_SESSION_BUY_ORDERS = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_SESSION_BUY_ORDERS;
        SYMBOL_SESSION_SELL_ORDERS = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_SESSION_SELL_ORDERS;
        SYMBOL_VOLUME = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_VOLUME;
        SYMBOL_VOLUMEHIGH = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_VOLUMEHIGH;
        SYMBOL_VOLUMELOW = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_VOLUMELOW;
        SYMBOL_TIME = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_TIME;
        SYMBOL_DIGITS = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_DIGITS;
        SYMBOL_SPREAD_FLOAT = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_SPREAD_FLOAT;
        SYMBOL_SPREAD = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_SPREAD;
        SYMBOL_TICKS_BOOKDEPTH = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_TICKS_BOOKDEPTH;
        SYMBOL_TRADE_CALC_MODE = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_TRADE_CALC_MODE;
        SYMBOL_TRADE_MODE = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_TRADE_MODE;
        SYMBOL_START_TIME = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_START_TIME;
        SYMBOL_EXPIRATION_TIME = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_EXPIRATION_TIME;
        SYMBOL_TRADE_STOPS_LEVEL = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_TRADE_STOPS_LEVEL;
        SYMBOL_TRADE_FREEZE_LEVEL = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_TRADE_FREEZE_LEVEL;
        SYMBOL_TRADE_EXEMODE = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_TRADE_EXEMODE;
        SYMBOL_SWAP_MODE = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_SWAP_MODE;
        SYMBOL_SWAP_ROLLOVER3DAYS = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_SWAP_ROLLOVER3DAYS
        SYMBOL_MARGIN_HEDGED_USE_LEG = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_MARGIN_HEDGED_USE_LEG;
        SYMBOL_EXPIRATION_MODE = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_EXPIRATION_MODE;
        SYMBOL_FILLING_MODE = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_FILLING_MODE;
        SYMBOL_ORDER_MODE = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_ORDER_MODE;
        SYMBOL_ORDER_GTC_MODE = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_ORDER_GTC_MODE;
        SYMBOL_ORDER_CLOSEBY = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_ORDER_CLOSEBY;
        SYMBOL_OPTION_MODE = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_OPTION_MODE;
        SYMBOL_OPTION_RIGHT = MtApi5.ENUM_SYMBOL_INFO_INTEGER.SYMBOL_OPTION_RIGHT; 
      %% ENUM_SYMBOL_INFO_DOUBLE
    
        SYM_BID                        = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_BID
        SYM_BIDHIGH                    = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_BIDHIGH
        SYM_BIDLOW                     = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_BIDLOW
        SYM_ASK                        = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_ASK
        SYM_ASKHIGH                    = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_ASKHIGH
        SYM_ASKLOW                     = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_ASKLOW
        SYM_LAST                       = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_LAST
        SYM_LASTHIGH                   = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_LASTHIGH
        SYM_LASTLOW                    = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_LASTLOW
        SYM_OPTION_STRIKE              = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_OPTION_STRIKE
        SYM_POINT                      = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_POINT
        SYM_TRADE_TICK_VALUE           = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_TRADE_TICK_VALUE
        SYM_TRADE_TICK_VALUE_PROFIT    = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_TRADE_TICK_VALUE_PROFIT
        SYM_TRADE_TICK_VALUE_LOSS      = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_TRADE_TICK_VALUE_LOSS
        SYM_TRADE_TICK_SIZE            = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_TRADE_TICK_SIZE
        SYM_TRADE_CONTRACT_SIZE        = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_TRADE_CONTRACT_SIZE
        SYM_TRADE_ACCRUED_INTEREST     = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_TRADE_ACCRUED_INTEREST
        SYM_TRADE_FACE_VALUE           = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_TRADE_FACE_VALUE
        SYM_TRADE_LIQUIDITY_RATE       = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_TRADE_LIQUIDITY_RATE
        SYM_VOLUME_MIN                 = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_VOLUME_MIN
        SYM_VOLUME_MAX                 = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_VOLUME_MAX
        SYM_VOLUME_STEP                = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_VOLUME_STEP
        SYM_VOLUME_LIMIT               = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_VOLUME_LIMIT
        SYM_SWAP_LONG                  = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_SWAP_LONG
        SYM_SWAP_SHORT                 = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_SWAP_SHORT
        SYM_MARGIN_INITIAL             = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_MARGIN_INITIAL
        SYM_MARGIN_MAINTENANCE         = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_MARGIN_MAINTENANCE
        SYM_MARGIN_LONG                = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_MARGIN_LONG     % FIXME: Undocumented!
        SYM_MARGIN_SHORT               = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_MARGIN_SHORT      % FIXME: Undocumented!
        SYM_MARGIN_LIMIT               = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_MARGIN_LIMIT       % FIXME: Undocumented!
        SYM_MARGIN_STOP                = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_MARGIN_STOP       % FIXME: Undocumented!
        SYM_MARGIN_STOPLIMIT           = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_MARGIN_STOPLIMIT  % FIXME: Undocumented!
        SYM_SESSION_VOLUME             = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_SESSION_VOLUME
        SYM_SESSION_TURNOVER           = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_SESSION_TURNOVER
        SYM_SESSION_INTEREST           = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_SESSION_INTEREST
        SYM_SESSION_BUY_ORDERS_VOLUME  = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_SESSION_BUY_ORDERS_VOLUME
        SYM_SESSION_SELL_ORDERS_VOLUME = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_SESSION_SELL_ORDERS_VOLUME
        SYM_SESSION_OPEN               = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_SESSION_OPEN
        SYM_SESSION_CLOSE              = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_SESSION_CLOSE
        SYM_SESSION_AW                 = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_SESSION_AW
        SYM_SESSION_PRICE_SETTLEMENT   = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_SESSION_PRICE_SETTLEMENT
        SYM_SESSION_PRICE_LIMIT_MIN    = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_SESSION_PRICE_LIMIT_MIN
        SYM_SESSION_PRICE_LIMIT_MAX    = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_SESSION_PRICE_LIMIT_MAX
        SYM_MARGIN_HEDGED              = MtApi5.ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_MARGIN_HEDGED
      %% ENUM_SYMBOL_INFO_STRING
    
        SYM_BASIS           = MtApi5.ENUM_SYMBOL_INFO_STRING.SYMBOL_BASIS
        SYM_CURRENCY_BASE   = MtApi5.ENUM_SYMBOL_INFO_STRING.SYMBOL_CURRENCY_BASE
        SYM_CURRENCY_PROFIT = MtApi5.ENUM_SYMBOL_INFO_STRING.SYMBOL_CURRENCY_PROFIT
        SYM_CURRENCY_MARGIN = MtApi5.ENUM_SYMBOL_INFO_STRING.SYMBOL_CURRENCY_MARGIN
        SYM_BANK            = MtApi5.ENUM_SYMBOL_INFO_STRING.SYMBOL_BANK
        SYM_DESCRIPTION     = MtApi5.ENUM_SYMBOL_INFO_STRING.SYMBOL_DESCRIPTION
        SYM_FORMULA         = MtApi5.ENUM_SYMBOL_INFO_STRING.SYMBOL_FORMULA
        SYM_PAGE            = MtApi5.ENUM_SYMBOL_INFO_STRING.SYMBOL_PAGE
        SYM_ISIN            = MtApi5.ENUM_SYMBOL_INFO_STRING.SYMBOL_ISIN
        SYM_PATH            = MtApi5.ENUM_SYMBOL_INFO_STRING.SYMBOL_PATH
      %% ENUM_SYMBOL_CHART_MODE 
    
         SYM_CHART_MODE_BID  =  MtApi5.ENUM_SYMBOL_CHART_MODE.SYMBOL_CHART_MODE_BID
         SYM_CHART_MODE_LAST =  MtApi5.ENUM_SYMBOL_CHART_MODE.SYMBOL_CHART_MODE_LAST  
      %% ENUM_SYMBOL_ORDER_GTC_MODE 
    
         SYM_ORDERS_GTC                   = MtApi5.ENUM_SYMBOL_ORDER_GTC_MODE.SYMBOL_ORDERS_GTC
         SYM_ORDERS_DAILY                 = MtApi5.ENUM_SYMBOL_ORDER_GTC_MODE.SYMBOL_ORDERS_DAILY
         SYM_ORDERS_DAILY_EXCLUDING_STOPS = MtApi5.ENUM_SYMBOL_ORDER_GTC_MODE.SYMBOL_ORDERS_DAILY_EXCLUDING_STOPS   
      %% ENUM_SYMBOL_OPTION_RIGHT
    
        SYM_OPTION_RIGHT_CALL = MtApi5.ENUM_SYMBOL_OPTION_RIGHT.SYMBOL_OPTION_RIGHT_CALL
        SYM_OPTION_RIGHT_PUT  = MtApi5.ENUM_SYMBOL_OPTION_RIGHT.SYMBOL_OPTION_RIGHT_PUT         
      %% ENUM_SYMBOL_OPTION_MODE
    
        SYM_OPTION_MODE_EUROPEAN = MtApi5.ENUM_SYMBOL_OPTION_MODE.SYMBOL_OPTION_MODE_EUROPEAN
        SYM_OPTION_MODE_AMERICAN = MtApi5.ENUM_SYMBOL_OPTION_MODE.SYMBOL_OPTION_MODE_AMERICAN        
      %% ENUM_SYMBOL_CALC_MODE
    
        SYM_CALC_MODE_FOREX              = MtApi5.ENUM_SYMBOL_CALC_MODE.SYMBOL_CALC_MODE_FOREX
        SYM_CALC_MODE_FUTURES            = MtApi5.ENUM_SYMBOL_CALC_MODE.SYMBOL_CALC_MODE_FUTURES
        SYM_CALC_MODE_CFD                = MtApi5.ENUM_SYMBOL_CALC_MODE.SYMBOL_CALC_MODE_CFD
        SYM_CALC_MODE_CFDINDEX           = MtApi5.ENUM_SYMBOL_CALC_MODE.SYMBOL_CALC_MODE_CFDINDEX
        SYM_CALC_MODE_CFDLEVERAGE        = MtApi5.ENUM_SYMBOL_CALC_MODE.SYMBOL_CALC_MODE_CFDLEVERAGE
        SYM_CALC_MODE_EXCH_STOCKS        = MtApi5.ENUM_SYMBOL_CALC_MODE.SYMBOL_CALC_MODE_EXCH_STOCKS
        SYM_CALC_MODE_EXCH_FUTURES       = MtApi5.ENUM_SYMBOL_CALC_MODE.SYMBOL_CALC_MODE_EXCH_FUTURES
        SYM_CALC_MODE_EXCH_FUTURES_FORTS = MtApi5.ENUM_SYMBOL_CALC_MODE.SYMBOL_CALC_MODE_EXCH_FUTURES_FORTS
        SYM_CALC_MODE_SERV_COLLATERAL    = MtApi5.ENUM_SYMBOL_CALC_MODE.SYMBOL_CALC_MODE_SERV_COLLATERAL
      %% ENUM_SYMBOL_TRADE_MODE
    
        SYM_TRADE_MODE_DISABLED  = MtApi5.ENUM_SYMBOL_TRADE_MODE.SYMBOL_TRADE_MODE_DISABLED
        SYM_TRADE_MODE_LONGONLY  = MtApi5.ENUM_SYMBOL_TRADE_MODE.SYMBOL_TRADE_MODE_LONGONLY
        SYM_TRADE_MODE_SHORTONLY = MtApi5.ENUM_SYMBOL_TRADE_MODE.SYMBOL_TRADE_MODE_SHORTONLY
        SYM_TRADE_MODE_CLOSEONLY = MtApi5.ENUM_SYMBOL_TRADE_MODE.SYMBOL_TRADE_MODE_CLOSEONLY
        SYM_TRADE_MODE_FULL      = MtApi5.ENUM_SYMBOL_TRADE_MODE.SYMBOL_TRADE_MODE_FULL
      %% ENUM_SYMBOL_TRADE_EXECUTION
    
        SYM_TRADE_EXECUTION_REQUEST  = MtApi5.ENUM_SYMBOL_TRADE_EXECUTION.SYMBOL_TRADE_EXECUTION_REQUEST
        SYM_TRADE_EXECUTION_INSTANT  = MtApi5.ENUM_SYMBOL_TRADE_EXECUTION.SYMBOL_TRADE_EXECUTION_INSTANT
        SYM_TRADE_EXECUTION_MARKET   = MtApi5.ENUM_SYMBOL_TRADE_EXECUTION.SYMBOL_TRADE_EXECUTION_MARKET
        SYM_TRADE_EXECUTION_EXCHANGE = MtApi5.ENUM_SYMBOL_TRADE_EXECUTION.SYMBOL_TRADE_EXECUTION_EXCHANGE
      %% ENUM_SYMBOL_SWAP_MODE
    
        SYM_SWAP_MODE_DISABLED         = MtApi5.ENUM_SYMBOL_SWAP_MODE.SYMBOL_SWAP_MODE_DISABLED
        SYM_SWAP_MODE_POINTS           = MtApi5.ENUM_SYMBOL_SWAP_MODE.SYMBOL_SWAP_MODE_POINTS
        SYM_SWAP_MODE_CURRENCY_SYMBOL  = MtApi5.ENUM_SYMBOL_SWAP_MODE.SYMBOL_SWAP_MODE_CURRENCY_SYMBOL
        SYM_SWAP_MODE_CURRENCY_MARGIN  = MtApi5.ENUM_SYMBOL_SWAP_MODE.SYMBOL_SWAP_MODE_CURRENCY_MARGIN
        SYM_SWAP_MODE_CURRENCY_DEPOSIT = MtApi5.ENUM_SYMBOL_SWAP_MODE.SYMBOL_SWAP_MODE_CURRENCY_DEPOSIT
        SYM_SWAP_MODE_INTEREST_CURRENT = MtApi5.ENUM_SYMBOL_SWAP_MODE.SYMBOL_SWAP_MODE_INTEREST_CURRENT
        SYM_SWAP_MODE_INTEREST_OPEN    = MtApi5.ENUM_SYMBOL_SWAP_MODE.SYMBOL_SWAP_MODE_INTEREST_OPEN
        SYM_SWAP_MODE_REOPEN_CURRENT   = MtApi5.ENUM_SYMBOL_SWAP_MODE.SYMBOL_SWAP_MODE_REOPEN_CURRENT
        SYM_SWAP_MODE_REOPEN_BID       = MtApi5.ENUM_SYMBOL_SWAP_MODE.SYMBOL_SWAP_MODE_REOPEN_BID
      %% ENUM_DAY_OF_WEEK
    
        DAY_SUNDAY    = MtApi5.ENUM_DAY_OF_WEEK.SUNDAY
        DAY_MONDAY    = MtApi5.ENUM_DAY_OF_WEEK.MONDAY
        DAY_TUESDAY   = MtApi5.ENUM_DAY_OF_WEEK.TUESDAY
        DAY_WEDNESDAY = MtApi5.ENUM_DAY_OF_WEEK.WEDNESDAY
        DAY_THURSDAY  = MtApi5.ENUM_DAY_OF_WEEK.THURSDAY
        DAY_FRIDAY    = MtApi5.ENUM_DAY_OF_WEEK.FRIDAY
        DAY_SATURDAY  = MtApi5.ENUM_DAY_OF_WEEK.SATURDAY 
      %% ENUM_ACCOUNT_INFO_INTEGER
    
        ACC_LOGIN          = MtApi5.ENUM_ACCOUNT_INFO_INTEGER.ACCOUNT_LOGIN             % Account number
        ACC_TRADE_MODE     = MtApi5.ENUM_ACCOUNT_INFO_INTEGER.ACCOUNT_TRADE_MODE        % Account trade mode
        ACC_LEVERAGE       = MtApi5.ENUM_ACCOUNT_INFO_INTEGER.ACCOUNT_LEVERAGE          % Account leverage
        ACC_LIMIT_ORDERS   = MtApi5.ENUM_ACCOUNT_INFO_INTEGER.ACCOUNT_LIMIT_ORDERS      % Maximum allowed number of active pending orders
        ACC_MARGIN_SO_MODE = MtApi5.ENUM_ACCOUNT_INFO_INTEGER.ACCOUNT_MARGIN_SO_MODE    % Mode for setting the minimal allowed margin
        ACC_TRADE_ALLOWED  = MtApi5.ENUM_ACCOUNT_INFO_INTEGER.ACCOUNT_TRADE_ALLOWED     % Allowed trade for the current account
        ACC_TRADE_EXPERT   = MtApi5.ENUM_ACCOUNT_INFO_INTEGER.ACCOUNT_TRADE_EXPERT      % Allowed trade for an Expert Advisor
        ACC_MARGIN_MODE    = MtApi5.ENUM_ACCOUNT_INFO_INTEGER.ACCOUNT_MARGIN_MODE       % Margin calculation mode   
      %% ENUM_ACCOUNT_INFO_DOUBLE
    
        ACC_BALANCE            = MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_BALANCE               % Account balance in the deposit currency
        ACC_CREDIT             = MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_CREDIT                % Account credit in the deposit currency
        ACC_PROFIT             = MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_PROFIT                % Current profit of an account in the deposit currency
        ACC_EQUITY             = MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_EQUITY                % Account equity in the deposit currency
        ACC_MARGIN             = MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_MARGIN                % Account margin used in the deposit currency
        ACC_MARGIN_FREE        = MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_MARGIN_FREE           % Free margin of an account in the deposit currency
        ACC_MARGIN_LEVEL       = MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_MARGIN_LEVEL          % Account margin level in percents
        ACC_MARGIN_SO_CALL     = MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_MARGIN_SO_CALL        % Margin call level
        ACC_MARGIN_SO_SO       = MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_MARGIN_SO_SO          % Margin stop out level
        ACC_MARGIN_INITIAL     = MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_MARGIN_INITIAL        % Initial margin
        ACC_MARGIN_MAINTENANCE = MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_MARGIN_MAINTENANCE    % Maintenance margin
        ACC_ASSETS             = MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_ASSETS                % The current assets of an account
        ACC_LIABILITIES        = MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_LIABILITIES           % The current liabilities on an account
        ACC_COMMISSION_BLOCKED = MtApi5.ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_COMMISSION_BLOCKED    % The current blocked commission amount on an account  
      %% ENUM_ACCOUNT_INFO_STRING
    
        ACC_NAME      = MtApi5.ENUM_ACCOUNT_INFO_STRING.ACCOUNT_NAME            % Client name
        ACC_SERVER    = MtApi5.ENUM_ACCOUNT_INFO_STRING.ACCOUNT_SERVER          % Trade server name
        ACC_CURRENCY  = MtApi5.ENUM_ACCOUNT_INFO_STRING.ACCOUNT_CURRENCY        % Account currency
        ACC_COMPANY   = MtApi5.ENUM_ACCOUNT_INFO_STRING.ACCOUNT_COMPANY         % Name of a company that serves the account  
      %% ENUM_ACCOUNT_TRADE_MODE
    
        ACC_TRADE_MODE_DEMO    = MtApi5.ENUM_ACCOUNT_TRADE_MODE.ACCOUNT_TRADE_MODE_DEMO         % Demo account
        ACC_TRADE_MODE_CONTEST = MtApi5.ENUM_ACCOUNT_TRADE_MODE.ACCOUNT_TRADE_MODE_CONTEST      % Contest account
        ACC_TRADE_MODE_REAL    = MtApi5.ENUM_ACCOUNT_TRADE_MODE.ACCOUNT_TRADE_MODE_REAL         % Real account   
      %% ENUM_ACCOUNT_STOPOUT_MODE
    
        ACC_STOPOUT_MODE_PERCENT = MtApi5.ENUM_ACCOUNT_STOPOUT_MODE.ACCOUNT_STOPOUT_MODE_PERCENT    % Account stop out mode in percents
        ACC_STOPOUT_MODE_MONEY   = MtApi5.ENUM_ACCOUNT_STOPOUT_MODE.ACCOUNT_STOPOUT_MODE_MONEY      % Account stop out mode in money
      %% ENUM_ACCOUNT_MARGIN_MODE
    
        ACC_MARGIN_MODE_RETAIL_NETTING = MtApi5.ENUM_ACCOUNT_MARGIN_MODE.ACCOUNT_MARGIN_MODE_RETAIL_NETTING     % Used for the OTC markets to interpret positions in the "netting" mode
        ACC_MARGIN_MODE_EXCHANGE       = MtApi5.ENUM_ACCOUNT_MARGIN_MODE.ACCOUNT_MARGIN_MODE_EXCHANGE           % Used for the exchange markets
        ACC_MARGIN_MODE_RETAIL_HEDGING = MtApi5.ENUM_ACCOUNT_MARGIN_MODE.ACCOUNT_MARGIN_MODE_RETAIL_HEDGING     % Used for the exchange markets where individual positions are possible    
      %% ENUM_SERIES_INFO_INTEGER
    
        SER_BARS_COUNT         = MtApi5.ENUM_SERIES_INFO_INTEGER.SERIES_BARS_COUNT              % Bars count for the symbol-period for the current moment
        SER_FIRSTDATE          = MtApi5.ENUM_SERIES_INFO_INTEGER.SERIES_FIRSTDATE               % The very first date for the symbol-period for the current moment
        SER_LASTBAR_DATE       = MtApi5.ENUM_SERIES_INFO_INTEGER.SERIES_LASTBAR_DATE            % Open time of the last bar of the symbol-period
        SER_SERVER_FIRSTDATE   = MtApi5.ENUM_SERIES_INFO_INTEGER.SERIES_SERVER_FIRSTDATE        % The very first date in the history of the symbol on the server regardless of the timeframe
        SER_TERMINAL_FIRSTDATE = MtApi5.ENUM_SERIES_INFO_INTEGER.SERIES_TERMINAL_FIRSTDATE      % The very first date in the history of the symbol in the client terminal, regardless of the timeframe
        SER_SYNCHRONIZED       = MtApi5.ENUM_SERIES_INFO_INTEGER.SERIES_SYNCHRONIZED            % Symbol/period data synchronization flag for the current moment
      %% ENUM_ORDER_PROPERTY_INTEGER
    
        order_TICKET          = MtApi5.ENUM_ORDER_PROPERTY_INTEGER.ORDER_TICKET           % Order ticket. Unique number assigned to each order
        order_TIME_SETUP      = MtApi5.ENUM_ORDER_PROPERTY_INTEGER.ORDER_TIME_SETUP       % Order setup time
        order_TYPE            = MtApi5.ENUM_ORDER_PROPERTY_INTEGER.ORDER_TYPE             % Order type
        order_STATE           = MtApi5.ENUM_ORDER_PROPERTY_INTEGER.ORDER_STATE            % Order state
        order_TIME_EXPIRATION = MtApi5.ENUM_ORDER_PROPERTY_INTEGER.ORDER_TIME_EXPIRATION  % Order expiration time
        order_TIME_DONE       = MtApi5.ENUM_ORDER_PROPERTY_INTEGER.ORDER_TIME_DONE        % Order execution or cancellation time
        order_TIME_SETUP_MSC  = MtApi5.ENUM_ORDER_PROPERTY_INTEGER.ORDER_TIME_SETUP_MSC   % The time of placing an order for execution in milliseconds since 01.01.1970
        order_TIME_DONE_MSC   = MtApi5.ENUM_ORDER_PROPERTY_INTEGER.ORDER_TIME_DONE_MSC    % Order execution/cancellation time in milliseconds since 01.01.1970
        order_TYPE_FILLING    = MtApi5.ENUM_ORDER_PROPERTY_INTEGER.ORDER_TYPE_FILLING     % Order filling type
        order_TYPE_TIME       = MtApi5.ENUM_ORDER_PROPERTY_INTEGER.ORDER_TYPE_TIME        % Order lifetime
        order_MAGIC           = MtApi5.ENUM_ORDER_PROPERTY_INTEGER.ORDER_MAGIC            % ID of an Expert Advisor that has placed the order (designed to ensure that each Expert Advisor places its own unique number)
        order_REASON          = MtApi5.ENUM_ORDER_PROPERTY_INTEGER.ORDER_REASON           % The reason or source for placing an order
        order_POSITION_ID     = MtApi5.ENUM_ORDER_PROPERTY_INTEGER.ORDER_POSITION_ID      % Position identifier that is set to an order as soon as it is executed. Each executed order results in a deal that opens or modifies an already existing position. The identifier of exactly this position is set to the executed order at this moment.
        order_POSITION_BY_ID  = MtApi5.ENUM_ORDER_PROPERTY_INTEGER.ORDER_POSITION_BY_ID   % Identifier of an opposite position used for closing by order order_TYPE_CLOSE_BY
      %% ENUM_ORDER_PROPERTY_DOUBLE
    
        order_VOLUME_INITIAL  = MtApi5.ENUM_ORDER_PROPERTY_DOUBLE.ORDER_VOLUME_INITIAL   % Order initial volume
        order_VOLUME_CURRENT  = MtApi5.ENUM_ORDER_PROPERTY_DOUBLE.ORDER_VOLUME_CURRENT   % Order current volume
        order_PRICE_OPEN      = MtApi5.ENUM_ORDER_PROPERTY_DOUBLE.ORDER_PRICE_OPEN       % Price specified in the order
        order_SL              = MtApi5.ENUM_ORDER_PROPERTY_DOUBLE.ORDER_SL               % Stop Loss value
        order_TP              = MtApi5.ENUM_ORDER_PROPERTY_DOUBLE.ORDER_TP               % Take Profit value
        order_PRICE_CURRENT   = MtApi5.ENUM_ORDER_PROPERTY_DOUBLE.ORDER_PRICE_CURRENT    % The current price of the order symbol
        order_PRICE_STOPLIMIT = MtApi5.ENUM_ORDER_PROPERTY_DOUBLE.ORDER_PRICE_STOPLIMIT  % The Limit order price for the StopLimit order   
      %% ENUM_ORDER_PROPERTY_STRING
    
        order_SYMBOL      = MtApi5.ENUM_ORDER_PROPERTY_STRING.ORDER_SYMBOL           % Symbol of the order
        order_COMMENT     = MtApi5.ENUM_ORDER_PROPERTY_STRING.ORDER_COMMENT          % Order comment
        order_EXTERNAL_ID = MtApi5.ENUM_ORDER_PROPERTY_STRING.ORDER_EXTERNAL_ID      % Order identifier in an external trading system (on the Exchange)
      %% ENUM_ORDER_TYPE
    
        order_TYPE_BUY             = MtApi5.ENUM_ORDER_TYPE.ORDER_TYPE_BUY              % Market Buy order
        order_TYPE_SELL            = MtApi5.ENUM_ORDER_TYPE.ORDER_TYPE_SELL             % Market Sell order
        order_TYPE_BUY_LIMIT       = MtApi5.ENUM_ORDER_TYPE.ORDER_TYPE_BUY_LIMIT        % Buy Limit pending order
        order_TYPE_SELL_LIMIT      = MtApi5.ENUM_ORDER_TYPE.ORDER_TYPE_SELL_LIMIT       % Sell Limit pending order
        order_TYPE_BUY_STOP        = MtApi5.ENUM_ORDER_TYPE.ORDER_TYPE_BUY_STOP         % Buy Stop pending order
        order_TYPE_SELL_STOP       = MtApi5.ENUM_ORDER_TYPE.ORDER_TYPE_SELL_STOP        % Sell Stop pending order
        order_TYPE_BUY_STOP_LIMIT  = MtApi5.ENUM_ORDER_TYPE.ORDER_TYPE_BUY_STOP_LIMIT   % Upon reaching the order price, a pending Buy Limit order is places at the StopLimit price
        order_TYPE_SELL_STOP_LIMIT = MtApi5.ENUM_ORDER_TYPE.ORDER_TYPE_SELL_STOP_LIMIT  % Upon reaching the order price, a pending Sell Limit order is places at the StopLimit price
        order_TYPE_CLOSE_BY        = MtApi5.ENUM_ORDER_TYPE.ORDER_TYPE_CLOSE_BY         % Order to close a position by an opposite one
      %% ENUM_ORDER_STATE
    
        order_STATE_STARTED        = MtApi5.ENUM_ORDER_STATE.ORDER_STATE_STARTED            % Order checked, but not yet accepted by broker
        order_STATE_PLACED         = MtApi5.ENUM_ORDER_STATE.ORDER_STATE_PLACED             % Order accepted
        order_STATE_CANCELED       = MtApi5.ENUM_ORDER_STATE.ORDER_STATE_CANCELED           % Order canceled by client
        order_STATE_PARTIAL        = MtApi5.ENUM_ORDER_STATE.ORDER_STATE_PARTIAL            % Order partially executed
        order_STATE_FILLED         = MtApi5.ENUM_ORDER_STATE.ORDER_STATE_FILLED             % Order fully executed
        order_STATE_REJECTED       = MtApi5.ENUM_ORDER_STATE.ORDER_STATE_REJECTED           % Order rejected
        order_STATE_EXPIRED        = MtApi5.ENUM_ORDER_STATE.ORDER_STATE_EXPIRED            % Order expired
        order_STATE_REQUEST_ADD    = MtApi5.ENUM_ORDER_STATE.ORDER_STATE_REQUEST_ADD        % Order is being registered (placing to the trading system)
        order_STATE_REQUEST_MODIFY = MtApi5.ENUM_ORDER_STATE.ORDER_STATE_REQUEST_MODIFY     % Order is being modified (changing its parameters)
        order_STATE_REQUEST_CANCEL = MtApi5.ENUM_ORDER_STATE.ORDER_STATE_REQUEST_CANCEL     % Order is being deleted (deleting from the trading system)
      %% ENUM_ORDER_TYPE_FILLING
    
        order_FILLING_FOK    = MtApi5.ENUM_ORDER_TYPE_FILLING.ORDER_FILLING_FOK
        order_FILLING_IOC    = MtApi5.ENUM_ORDER_TYPE_FILLING.ORDER_FILLING_IOC
        order_FILLING_RETURN = MtApi5.ENUM_ORDER_TYPE_FILLING.ORDER_FILLING_RETURN
      %% ENUM_ORDER_TYPE_TIME
    
        order_TIME_GTC           = MtApi5.ENUM_ORDER_TYPE_TIME.ORDER_TIME_GTC
        order_TIME_DAY           = MtApi5.ENUM_ORDER_TYPE_TIME.ORDER_TIME_DAY
        order_TIME_SPECIFIED     = MtApi5.ENUM_ORDER_TYPE_TIME.ORDER_TIME_SPECIFIED
        order_TIME_SPECIFIED_DAY = MtApi5.ENUM_ORDER_TYPE_TIME.ORDER_TIME_SPECIFIED_DAY
      %% ENUM_ORDER_REASON
    
        order_REASON_CLIENT = MtApi5.ENUM_ORDER_REASON.ORDER_REASON_CLIENT    % The order was placed from a desktop terminal
        order_REASON_MOBILE = MtApi5.ENUM_ORDER_REASON.ORDER_REASON_MOBILE    % The order was placed from a mobile application
        order_REASON_WEB    = MtApi5.ENUM_ORDER_REASON.ORDER_REASON_WEB       % The order was placed from a web platform
        order_REASON_EXPERT = MtApi5.ENUM_ORDER_REASON.ORDER_REASON_EXPERT    % The order was placed from an MQL5-program, i.e. by an Expert Advisor or a script
        order_REASON_SL     = MtApi5.ENUM_ORDER_REASON.ORDER_REASON_SL        % The order was placed as a result of Stop Loss activation
        order_REASON_TP     = MtApi5.ENUM_ORDER_REASON.ORDER_REASON_TP        % The order was placed as a result of Take Profit activation
        order_REASON_SO     = MtApi5.ENUM_ORDER_REASON.ORDER_REASON_SO        % The order was placed as a result of the Stop Out event 
      %% ENUM_POSITION_PROPERTY_INTEGER
   
        POSITION_TICKET          = MtApi5.ENUM_POSITION_PROPERTY_INTEGER.POSITION_TICKET            % Position ticket
        POSITION_TIME            = MtApi5.ENUM_POSITION_PROPERTY_INTEGER.POSITION_TIME              % Position open time
        POSITION_TIME_MSC        = MtApi5.ENUM_POSITION_PROPERTY_INTEGER.POSITION_TIME_MSC          % Position opening time in milliseconds since 01.01.1970
        POSITION_TIME_UPDATE     = MtApi5.ENUM_POSITION_PROPERTY_INTEGER.POSITION_TIME_UPDATE       % Position changing time in seconds since 01.01.1970
        POSITION_TIME_UPDATE_MSC = MtApi5.ENUM_POSITION_PROPERTY_INTEGER.POSITION_TIME_UPDATE_MSC   % Position changing time in milliseconds since 01.01.1970
        POSITION_TYPE            = MtApi5.ENUM_POSITION_PROPERTY_INTEGER.POSITION_TYPE              % Position type
        POSITION_MAGIC           = MtApi5.ENUM_POSITION_PROPERTY_INTEGER.POSITION_MAGIC             % Position magic number
        POSITION_IDENTIFIER      = MtApi5.ENUM_POSITION_PROPERTY_INTEGER.POSITION_IDENTIFIER        % Position identifier is a unique number that is assigned to every newly opened position and doesn't change during the entire lifetime of the position. Position turnover doesn't change its identifier.
        POSITION_REASON          = MtApi5.ENUM_POSITION_PROPERTY_INTEGER.POSITION_REASON            % The reason for opening a position
      %% ENUM_POSITION_PROPERTY_DOUBLE
    
        POSITION_VOLUME        = MtApi5.ENUM_POSITION_PROPERTY_DOUBLE.POSITION_VOLUME            % Position volume
        POSITION_PRICE_OPEN    = MtApi5.ENUM_POSITION_PROPERTY_DOUBLE.POSITION_PRICE_OPEN        % Position open price
        POSITION_SL            = MtApi5.ENUM_POSITION_PROPERTY_DOUBLE.POSITION_SL                % Stop Loss level of opened position
        POSITION_TP            = MtApi5.ENUM_POSITION_PROPERTY_DOUBLE.POSITION_TP                % Take Profit level of opened position
        POSITION_PRICE_CURRENT = MtApi5.ENUM_POSITION_PROPERTY_DOUBLE.POSITION_PRICE_CURRENT     % Current price of the position symbol
        POSITION_COMMISSION    = MtApi5.ENUM_POSITION_PROPERTY_DOUBLE.POSITION_COMMISSION        % FIXME: Undocumented!
        POSITION_SWAP          = MtApi5.ENUM_POSITION_PROPERTY_DOUBLE.POSITION_SWAP              % Cumulative swap
        POSITION_PROFIT        = MtApi5.ENUM_POSITION_PROPERTY_DOUBLE.POSITION_PROFIT            % Current profit      
      %% ENUM_POSITION_PROPERTY_STRING
    
        POSITION_SYMBOL  = MtApi5.ENUM_POSITION_PROPERTY_STRING.POSITION_SYMBOL    % Symbol of the position
        POSITION_COMMENT = MtApi5.ENUM_POSITION_PROPERTY_STRING.POSITION_COMMENT   % Position comment       
      %% ENUM_POSITION_TYPE
    
        POSITION_TYPE_BUY  = MtApi5.ENUM_POSITION_TYPE.POSITION_TYPE_BUY      % Buy
        POSITION_TYPE_SELL = MtApi5.ENUM_POSITION_TYPE.POSITION_TYPE_SELL     % Sell       
      %% ENUM_POSITION_REASON
    
        POSITION_REASON_CLIENT = MtApi5.ENUM_POSITION_REASON.POSITION_REASON_CLIENT  % The position was opened as a result of activation of an order placed from a desktop terminal
        POSITION_REASON_MOBILE = MtApi5.ENUM_POSITION_REASON.POSITION_REASON_MOBILE  % The position was opened as a result of activation of an order placed from a mobile application
        POSITION_REASON_WEB    = MtApi5.ENUM_POSITION_REASON.POSITION_REASON_WEB     % The position was opened as a result of activation of an order placed from the web platform
        POSITION_REASON_EXPERT = MtApi5.ENUM_POSITION_REASON.POSITION_REASON_EXPERT  % The position was opened as a result of activation of an order placed from an MQL5 program   
      %% ENUM_DEAL_PROPERTY_INTEGER
    
        DEAL_TICKET      = MtApi5.ENUM_DEAL_PROPERTY_INTEGER.DEAL_TICKET            % Deal ticket. Unique number assigned to each deal
        DEAL_ORDER       = MtApi5.ENUM_DEAL_PROPERTY_INTEGER.DEAL_ORDER             % Deal order number
        DEAL_TIME        = MtApi5.ENUM_DEAL_PROPERTY_INTEGER.DEAL_TIME              % Deal time
        DEAL_TIME_MSC    = MtApi5.ENUM_DEAL_PROPERTY_INTEGER.DEAL_TIME_MSC          % The time of a deal execution in milliseconds since 01.01.1970
        DEAL_TYPE        = MtApi5.ENUM_DEAL_PROPERTY_INTEGER.DEAL_TYPE              % Deal type
        DEAL_ENTRY       = MtApi5.ENUM_DEAL_PROPERTY_INTEGER.DEAL_ENTRY             % Deal entry - entry in, entry out, reverse
        DEAL_MAGIC       = MtApi5.ENUM_DEAL_PROPERTY_INTEGER.DEAL_MAGIC             % Deal magic number
        DEAL_REASON      = MtApi5.ENUM_DEAL_PROPERTY_INTEGER.DEAL_REASON            % The reason or source for deal execution
        DEAL_POSITION_ID = MtApi5.ENUM_DEAL_PROPERTY_INTEGER.DEAL_POSITION_ID       % Identifier of a position
      %% ENUM_DEAL_PROPERTY_DOUBLE
  
        DEAL_VOLUME     = MtApi5.ENUM_DEAL_PROPERTY_DOUBLE.DEAL_VOLUME         % Deal volume
        DEAL_PRICE      = MtApi5.ENUM_DEAL_PROPERTY_DOUBLE.DEAL_PRICE          % Deal price
        DEAL_COMMISSION = MtApi5.ENUM_DEAL_PROPERTY_DOUBLE.DEAL_COMMISSION     % Deal commission
        DEAL_SWAP       = MtApi5.ENUM_DEAL_PROPERTY_DOUBLE.DEAL_SWAP           % Cumulative swap on close
        DEAL_PROFIT     = MtApi5.ENUM_DEAL_PROPERTY_DOUBLE.DEAL_PROFIT         % Deal profit  
      %% ENUM_DEAL_PROPERTY_STRING
   
        DEAL_SYMBOL      = MtApi5.ENUM_DEAL_PROPERTY_STRING.DEAL_SYMBOL        % Deal symbol
        DEAL_COMMENT     = MtApi5.ENUM_DEAL_PROPERTY_STRING.DEAL_COMMENT       % Deal comment
        DEAL_EXTERNAL_ID = MtApi5.ENUM_DEAL_PROPERTY_STRING.DEAL_EXTERNAL_ID   % Deal identifier in an external trading system (on the Exchange)   
      %% ENUM_DEAL_TYPE
    
        DEAL_TYPE_BUY                      = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_BUY                          % Buy
        DEAL_TYPE_SELL                     = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_SELL                         % Sell
        DEAL_TYPE_BALANCE                  = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_BALANCE                      % Balance
        DEAL_TYPE_CREDIT                   = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_CREDIT                       % Credit
        DEAL_TYPE_CHARGE                   = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_CHARGE                       % Additional charge
        DEAL_TYPE_CORRECTION               = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_CORRECTION                   % Correction
        DEAL_TYPE_BONUS                    = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_BONUS                        % Bonus
        DEAL_TYPE_COMMISSION               = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_COMMISSION                   % Additional commission
        DEAL_TYPE_COMMISSION_DAILY         = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_COMMISSION_DAILY             % Daily commission
        DEAL_TYPE_COMMISSION_MONTHLY       = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_COMMISSION_MONTHLY           % Monthly commission
        DEAL_TYPE_COMMISSION_AGENT_DAILY   = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_COMMISSION_AGENT_DAILY       % Daily agent commission
        DEAL_TYPE_COMMISSION_AGENT_MONTHLY = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_COMMISSION_AGENT_MONTHLY     % Monthly agent commission
        DEAL_TYPE_INTEREST                 = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_INTEREST                     % Interest rate
        DEAL_TYPE_BUY_CANCELED             = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_BUY_CANCELED                 % Canceled buy deal
        DEAL_TYPE_SELL_CANCELED            = MtApi5.ENUM_DEAL_TYPE.DEAL_TYPE_SELL_CANCELED                % Canceled sell deal
        DEAL_DIVIDEND                      = MtApi5.ENUM_DEAL_TYPE.DEAL_DIVIDEND                          % Dividend operations
        DEAL_DIVIDEND_FRANKED              = MtApi5.ENUM_DEAL_TYPE.DEAL_DIVIDEND_FRANKED                  % Franked (non-taxable) dividend operations
        DEAL_TAX                           = MtApi5.ENUM_DEAL_TYPE.DEAL_TAX                               % Tax charges   
      %% ENUM_DEAL_ENTRY
    
        DEAL_ENTRY_IN    = MtApi5.ENUM_DEAL_ENTRY.DEAL_ENTRY_IN          % Entry in
        DEAL_ENTRY_OUT   = MtApi5.ENUM_DEAL_ENTRY.DEAL_ENTRY_OUT         % Entry out
        DEAL_ENTRY_INOUT = MtApi5.ENUM_DEAL_ENTRY.DEAL_ENTRY_INOUT       % Reverse
        DEAL_ENTRY_STATE = MtApi5.ENUM_DEAL_ENTRY.DEAL_ENTRY_STATE       % Close a position by an opposite one   
      %% ENUM_DEAL_REASON
    
        DEAL_REASON_CLIENT   = MtApi5.ENUM_DEAL_REASON.DEAL_REASON_CLIENT         % The deal was executed as a result of activation of an order placed from a desktop terminal
        DEAL_REASON_MOBILE   = MtApi5.ENUM_DEAL_REASON.DEAL_REASON_MOBILE         % The deal was executed as a result of activation of an order placed from a mobile application
        DEAL_REASON_WEB      = MtApi5.ENUM_DEAL_REASON.DEAL_REASON_WEB            % The deal was executed as a result of activation of an order placed from the web platform
        DEAL_REASON_EXPERT   = MtApi5.ENUM_DEAL_REASON.DEAL_REASON_EXPERT         % The deal was executed as a result of activation of an order placed from an MQL5 program, i.e. an Expert Advisor or a script
        DEAL_REASON_SL       = MtApi5.ENUM_DEAL_REASON.DEAL_REASON_SL             % The deal was executed as a result of Stop Loss activation
        DEAL_REASON_TP       = MtApi5.ENUM_DEAL_REASON.DEAL_REASON_TP             % The deal was executed as a result of Take Profit activation
        DEAL_REASON_SO       = MtApi5.ENUM_DEAL_REASON.DEAL_REASON_SO             % The deal was executed as a result of the Stop Out event
        DEAL_REASON_ROLLOVER = MtApi5.ENUM_DEAL_REASON.DEAL_REASON_ROLLOVER       % The deal was executed due to a rollover
        DEAL_REASON_VMARGIN  = MtApi5.ENUM_DEAL_REASON.DEAL_REASON_VMARGIN        % The deal was executed after charging the variation margin
        DEAL_REASON_SPLIT    = MtApi5.ENUM_DEAL_REASON.DEAL_REASON_SPLIT           % The deal was executed after the split (price reduction) of an instrument, which had an open position during split announcement  
      %% ENUM_TRADE_REQUEST_ACTIONS
    
        TRADE_ACTION_DEAL     = MtApi5.ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_DEAL      % Place a trade order for an immediate execution with the specified parameters (market order)
        TRADE_ACTION_PENDING  = MtApi5.ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_PENDING   % Place a trade order for the execution under specified conditions (pending order)
        TRADE_ACTION_SLTP     = MtApi5.ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_SLTP      % Modify Stop Loss and Take Profit values of an opened position
        TRADE_ACTION_MODIFY   = MtApi5.ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_MODIFY    % Modify the parameters of the order placed previously
        TRADE_ACTION_REMOVE   = MtApi5.ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_REMOVE    % Delete the pending order placed previously
        TRADE_ACTION_CLOSE_BY = MtApi5.ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_CLOSE_BY  % Close a position by an opposite one    
      %% ENUM_TRADE_TRANSACTION_TYPE
    
        TRANSACTION_ORDER_ADD      = MtApi5.ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_ORDER_ADD            % Adding a new open order
        TRANSACTION_ORDER_UPDATE   = MtApi5.ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_ORDER_UPDATE         % Updating an open order. The updates include not only evident changes from the client terminal or a trade server sides but also changes of an order state when setting it (for example, transition from ORDER_STATE_STARTED to ORDER_STATE_PLACED or from ORDER_STATE_PLACED to ORDER_STATE_PARTIAL, etc.).
        TRANSACTION_ORDER_DELETE   = MtApi5.ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_ORDER_DELETE         % Removing an order from the list of the open ones. An order can be deleted from the open ones as a result of setting an appropriate request or execution (filling) and moving to the history.
        TRANSACTION_DEAL_ADD       = MtApi5.ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_DEAL_ADD             % Adding a deal to the history. The action is performed as a result of an order execution or performing operations with an account balance.
        TRANSACTION_DEAL_UPDATE    = MtApi5.ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_DEAL_UPDATE          % Updating a deal in the history. There may be cases when a previously executed deal is changed on a server. For example, a deal has been changed in an external trading system (exchange) where it was previously transferred by a broker.
        TRANSACTION_DEAL_DELETE    = MtApi5.ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_DEAL_DELETE          % Deleting a deal from the history. There may be cases when a previously executed deal is deleted from a server. For example, a deal has been deleted in an external trading system (exchange) where it was previously transferred by a broker.
        TRANSACTION_HISTORY_ADD    = MtApi5.ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_HISTORY_ADD          % Adding an order to the history as a result of execution or cancellation.
        TRANSACTION_HISTORY_UPDATE = MtApi5.ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_HISTORY_UPDATE       % Changing an order located in the orders history. This type is provided for enhancing functionality on a trade server side.
        TRANSACTION_HISTORY_DELETE = MtApi5.ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_HISTORY_DELETE       % Deleting an order from the orders history. This type is provided for enhancing functionality on a trade server side.
        TRANSACTION_POSITION       = MtApi5.ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_POSITION             % Changing a position not related to a deal execution. This type of transaction shows that a position has been changed on a trade server side. Position volume, open price, Stop Loss and Take Profit levels can be changed. Data on changes are submitted in MqlTradeTransaction structure via OnTradeTransaction handler. Position change (adding, changing or closing), as a result of a deal execution, does not lead to the occurrence of TRADE_TRANSACTION_POSITION transaction.
        TRANSACTION_REQUEST        = MtApi5.ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_REQUEST              % Notification of the fact that a trade request has been processed by a server and processing result has been received. Only type field (trade transaction type) must be analyzed for such transactions in MqlTradeTransaction structure. The second and third parameters of OnTradeTransaction (request and result) must be analyzed for additional data.
      %% ENUM_BOOK_TYPE
    
        BOOK_TYPE_SELL        = MtApi5.ENUM_BOOK_TYPE.BOOK_TYPE_SELL             % Sell order (Offer)
        BOOK_TYPE_BUY         = MtApi5.ENUM_BOOK_TYPE.BOOK_TYPE_BUY              % Buy order (Bid)
        BOOK_TYPE_SELL_MARKET = MtApi5.ENUM_BOOK_TYPE.BOOK_TYPE_SELL_MARKET      % Sell order by Market
        BOOK_TYPE_BUY_MARKET  = MtApi5.ENUM_BOOK_TYPE.BOOK_TYPE_BUY_MARKET       % Buy order by Market  
      %% ENUM_OBJECT
    
        OBJ_VLINE = 0,              %  Vertical Line
        OBJ_HLINE = 1,              %  Horizontal Line
        OBJ_TREND = 2,              %  Trend Line
        OBJ_TRENDBYANGLE = 3,       %  Trend Line By Angle
        OBJ_CYCLES = 4,             %  Cycle Lines
        OBJ_ARROWED_LINE = 108,     %  Arrowed Line
        OBJ_CHANNEL = 5,            %  Equidistant Channel
        OBJ_STDDEVCHANNEL = 6,      %  Standard Deviation Channel
        OBJ_REGRESSION = 7,         %  Linear Regression Channel
        OBJ_PITCHFORK = 8,          %  Andrews? Pitchfork
        OBJ_GANNLINE = 9,           %  Gann Line
        OBJ_GANNFAN = 10,           %  Gann Fan
        OBJ_GANNGRID = 11,          %  Gann Grid
        OBJ_FIBO = 12,              %  Fibonacci Retracement
        OBJ_FIBOTIMES = 13,         %  Fibonacci Time Zones
        OBJ_FIBOFAN = 14,           %  Fibonacci Fan
        OBJ_FIBOARC = 15,           %  Fibonacci Arcs
        OBJ_FIBOCHANNEL = 16,       %  Fibonacci Channel
        OBJ_EXPANSION = 17,         %  Fibonacci Expansion
        OBJ_ELLIOTWAVE5 = 18,       %  Elliott Motive Wave
        OBJ_ELLIOTWAVE3 = 19,       %  Elliott Correction Wave
        OBJ_RECTANGLE = 20,         %  Rectangle
        OBJ_TRIANGLE = 21,          %  Triangle
        OBJ_ELLIPSE = 22,           %  Ellipse
        OBJ_ARROW_THUMB_UP = 23,    %  Thumbs Up
        OBJ_ARROW_THUMB_DOWN = 24,  %  Thumbs Down
        OBJ_ARROW_UP = 25,          %  Arrow Up
        OBJ_ARROW_DOWN = 26,        %  Arrow Down
        OBJ_ARROW_STOP = 27,        %  Stop Sign
        OBJ_ARROW_CHECK = 28,       %  Check Sign
        OBJ_ARROW_LEFT_PRICE = 29,  %  Left Price Label
        OBJ_ARROW_RIGHT_PRICE = 30, %  Right Price Label
        OBJ_ARROW_BUY = 31,         %  Buy Sign
        OBJ_ARROW_SELL = 32,        %  Sell Sign
        OBJ_ARROW = 100,            %  Arrow
        OBJ_TEXT = 101,             %  Text
        OBJ_LABEL = 102,            %  Label
        OBJ_BUTTON = 103,           %  Button
        OBJ_CHART = 104,            %  Chart
        OBJ_BITMAP = 105,           %  Bitmap
        OBJ_BITMAP_LABEL = 106,     %  Bitmap Label
        OBJ_EDIT = 107,             %  Edit
        OBJ_EVENT = 109,            %  The "Event" object corresponding to an event in the economic calendar
        OBJ_RECTANGLE_LABEL = 110   %  The "Rectangle label" object for creating and designing the custom graphical interface.   
      %% ENUM_OBJECT_PROPERTY_DOUBLE
    
        OBJPROP_PRICE = 9,          %  Price coordinate
        OBJPROP_LEVELVALUE = 204,   %  Level value
        OBJPROP_SCALE = 1006,       %  Scale (properties of Gann objects and Fibonacci Arcs)
        OBJPROP_ANGLE = 1007,       %  Angle.  For the objects with no angle specified, created from a program, the value is equal to EMPTY_VALUE
        OBJPROP_DEVIATION = 1010    %  Deviation for the Standard Deviation Channel
      %% ENUM_OBJECT_PROPERTY_INTEGER
    
        OBJPROP_COLOR = 0,          %  Color
        OBJPROP_STYLE = 1,          %  Style
        OBJPROP_WIDTH = 2,          %  Line thickness
        OBJPROP_BACK = 3,           %  Object in the background
        OBJPROP_ZORDER = 207,       %  Priority of a graphical object for receiving events of clicking on a chart (CHARTEVENT_CLICK). The default zero value is set when creating an object; the priority can be increased if necessary. When objects are placed one atop another, only one of them with the highest priority will receive the CHARTEVENT_CLICK event.
        OBJPROP_FILL = 1031,        %  Fill an object with color (for OBJ_RECTANGLE, OBJ_TRIANGLE, OBJ_ELLIPSE, OBJ_CHANNEL, OBJ_STDDEVCHANNEL, OBJ_REGRESSION)
        OBJPROP_HIDDEN = 208,       %  Prohibit showing of the name of a graphical object in the list of objects from the terminal menu "Charts" - "Objects" - "List of objects". The true value allows to hide an object from the list. By default, true is set to the objects that display calendar events, trading history and to the objects created from MQL5 programs. To see such graphical objects and access their properties, click on the "All" button in the "List of objects" window.
        OBJPROP_SELECTED = 4,       %  Object is selected
        OBJPROP_READONLY = 1028,    %  Ability to edit text in the Edit object
        OBJPROP_TYPE = 7,           %  Object type
        OBJPROP_TIME = 8,           %  Time coordinate
        OBJPROP_SELECTABLE = 10,    %  Object availability
        OBJPROP_CREATETIME = 11,    %  Time of object creation
        OBJPROP_LEVELS = 200,       %  Number of levels
        OBJPROP_LEVELCOLOR = 201,   %  Color of the line-level
        OBJPROP_LEVELSTYLE = 202,   %  Style of the line-level
        OBJPROP_LEVELWIDTH = 203,   %  Thickness of the line-level
        OBJPROP_ALIGN = 1036,       %  Horizontal text alignment in the "Edit" object (OBJ_EDIT)
        OBJPROP_FONTSIZE = 1002,    %  Font size
        OBJPROP_RAY_LEFT = 1003,    %  Ray goes to the left
        OBJPROP_RAY_RIGHT = 1004,   %  Ray goes to the right
        OBJPROP_RAY = 1032,         %  A vertical line goes through all the windows of a chart
        OBJPROP_ELLIPSE = 1005,     %  Showing the full ellipse of the Fibonacci Arc object (OBJ_FIBOARC)
        OBJPROP_ARROWCODE = 1008,   %  Arrow code for the Arrow object
        OBJPROP_TIMEFRAMES = 12,    %  Visibility of an object at timeframes
        OBJPROP_ANCHOR = 1011,      %  Location of the anchor point of a graphical object
        OBJPROP_XDISTANCE = 1012,   %  The distance in pixels along the X axis from the binding corner
        OBJPROP_YDISTANCE = 1013,   %  The distance in pixels along the Y axis from the binding corner
        OBJPROP_DIRECTION = 1014,   %  Trend of the Gann object
        OBJPROP_DEGREE = 1015,      %  Level of the Elliott Wave Marking
        OBJPROP_DRAWLINES = 1016,   %  Displaying lines for marking the Elliott Wave
        OBJPROP_STATE = 1018,       %  Button state (pressed / depressed)
        OBJPROP_CHART_ID = 1030,    %  ID of the "Chart" object (OBJ_CHART). It allows working with the properties of this object like with a normal chart using the functions described in Chart Operations, but there some exceptions.
        OBJPROP_XSIZE = 1019,       %  The object's width along the X axis in pixels. Specified for  OBJ_LABEL (read only), OBJ_BUTTON, OBJ_CHART, OBJ_BITMAP, OBJ_BITMAP_LABEL, OBJ_EDIT, OBJ_RECTANGLE_LABEL objects.
        OBJPROP_YSIZE = 1020,       %  The object's height along the Y axis in pixels. Specified for  OBJ_LABEL (read only), OBJ_BUTTON, OBJ_CHART, OBJ_BITMAP, OBJ_BITMAP_LABEL, OBJ_EDIT, OBJ_RECTANGLE_LABEL objects.
        OBJPROP_XOFFSET = 1033,     %  The X coordinate of the upper left corner of the rectangular visible area in the graphical objects "Bitmap Label" and "Bitmap" (OBJ_BITMAP_LABEL and OBJ_BITMAP). The value is set in pixels relative to the upper left corner of the original image.
        OBJPROP_YOFFSET = 1034,     %  The Y coordinate of the upper left corner of the rectangular visible area in the graphical objects "Bitmap Label" and "Bitmap" (OBJ_BITMAP_LABEL and OBJ_BITMAP). The value is set in pixels relative to the upper left corner of the original image.
        OBJPROP_PERIOD = 1022,      %  Timeframe for the Chart object
        OBJPROP_DATE_SCALE = 1023,  %  Displaying the time scale for the Chart object
        OBJPROP_PRICE_SCALE = 1024, %  Displaying the price scale for the Chart object
        OBJPROP_CHART_SCALE = 1027, %  The scale for the Chart object
        OBJPROP_BGCOLOR = 1025,     %  The background color for  OBJ_EDIT, OBJ_BUTTON, OBJ_RECTANGLE_LABEL
        OBJPROP_CORNER = 1026,      %  The corner of the chart to link a graphical object
        OBJPROP_BORDER_TYPE = 1029, %  Border type for the "Rectangle label" object
        OBJPROP_BORDER_COLOR = 1035 %  Border color for the OBJ_EDIT and OBJ_BUTTON objects
      %% ENUM_OBJECT_PROPERTY_STRING
    
        OBJPROP_NAME = 5,           %  Object name
        OBJPROP_TEXT = 6,           %  Description of the object (the text contained in the object)
        OBJPROP_TOOLTIP = 206,      %  The text of a tooltip. If the property is not set, then the tooltip generated automatically by the terminal is shown. A tooltip can be disabled by setting the "\n" (line feed) value to it
        OBJPROP_LEVELTEXT = 205,    %  Level description
        OBJPROP_FONT = 1001,        %  Font
        OBJPROP_BMPFILE = 1017,     %  The name of BMP-file for Bitmap Label.
        OBJPROP_SYMBOL = 1021       %  Symbol for the Chart object  
      %% ENUM_BORDER_TYPE
    
        BORDER_FLAT = 0,    %  Flat form
        BORDER_RAISED = 1,  %  Prominent form
        BORDER_SUNKEN = 2   %  Concave form
      %% ENUM_ALIGN_MODE
    
        ALIGN_LEFT = 1,     %  Left alignment
        ALIGN_CENTER = 2,   %  Centered (only for the Edit object)
        ALIGN_RIGHT = 0,    %  Right alignment
      %% ENUM_APPLIED_PRICE
    
        PRICE_CLOSE    = MtApi5.ENUM_APPLIED_PRICE.PRICE_CLOSE    % Close price
        PRICE_OPEN     = MtApi5.ENUM_APPLIED_PRICE.PRICE_OPEN     % Open price
        PRICE_HIGH     = MtApi5.ENUM_APPLIED_PRICE.PRICE_HIGH     % The maximum price for the period
        PRICE_LOW      = MtApi5.ENUM_APPLIED_PRICE.PRICE_LOW      % The minimum price for the period
        PRICE_MEDIAN   = MtApi5.ENUM_APPLIED_PRICE.PRICE_MEDIAN   % Median price, (high + low)/2
        PRICE_TYPICAL  = MtApi5.ENUM_APPLIED_PRICE.PRICE_TYPICAL  % Typical price, (high + low + close)/3
        PRICE_WEIGHTED = MtApi5.ENUM_APPLIED_PRICE.PRICE_WEIGHTED  % Average price, (high + low + close + close)/4
      %% ENUM_APPLIED_VOLUME
    
        VOLUME_TICK = MtApi5.ENUM_APPLIED_VOLUME.VOLUME_TICK    % Tick volume
        VOLUME_REAL = MtApi5.ENUM_APPLIED_VOLUME.VOLUME_REAL     % Trade volume
      %% ENUM_STO_PRICE
    
        STO_LOWHIGH    = MtApi5.ENUM_STO_PRICE.STO_LOWHIGH    % Calculation is based on Low/High prices
        STO_CLOSECLOSE = MtApi5.ENUM_STO_PRICE.STO_CLOSECLOSE  % Calculation is based on Close/Close prices  
      %% ENUM_MA_METHOD
    
        MODE_SMA = 0,   % Simple averaging
        MODE_EMA = 1,   % Exponential averaging
        MODE_SMMA = 2,  % Smoothed averaging
        MODE_LWMA = 3   % Linear-weighted averaging
      %% ENUM_INDICATOR
    
        IND_AC          = 5,    %  Accelerator Oscillator
        IND_AD          = 6,    %  Accumulation/Distribution
        IND_ADX         = 8,    %  Average Directional Index
        IND_ADXW        = 9,    %  ADX by Welles Wilder
        IND_ALLIGATOR   = 7,    %  Alligator
        IND_AMA         = 40,   %  Adaptive Moving Average
        IND_AO          = 11,   %  Awesome Oscillator
        IND_ATR         = 10,   %  Average True Range
        IND_BANDS       = 13,   %  Bollinger Bands®
        IND_BEARS       = 12,   %  Bears Power
        IND_BULLS       = 14,   %  Bulls Power
        IND_BWMFI       = 22,   %  Market Facilitation Index
        IND_CCI         = 15,   %  Commodity Channel Index
        IND_CHAIKIN     = 41,   %  Chaikin Oscillator
        IND_CUSTOM      = 43,   %  Custom indicator
        IND_DEMA        = 36,   %  Double Exponential Moving Average
        IND_DEMARKER    = 16,   %  DeMarker
        IND_ENVELOPES   = 17,   %  Envelopes
        IND_FORCE       = 18,   %  Force Index
        IND_FRACTALS    = 19,   %  Fractals
        IND_FRAMA       = 39,   %  Fractal Adaptive Moving Average
        IND_GATOR       = 20,   %  Gator Oscillator
        IND_ICHIMOKU    = 21,   %  Ichimoku Kinko Hyo
        IND_MA          = 26,   %  Moving Average
        IND_MACD        = 23,   %  MACD
        IND_MFI         = 25,   %  Money Flow Index
        IND_MOMENTUM    = 24,   %  Momentum
        IND_OBV         = 28,   %  On Balance Volume
        IND_OSMA        = 27,   %  OsMA
        IND_RSI         = 30,   %  Relative Strength Index
        IND_RVI         = 31,   %  Relative Vigor Index
        IND_SAR         = 29,   %  Parabolic SAR
        IND_STDDEV      = 32,   %  Standard Deviation
        IND_STOCHASTIC  = 33,   %  Stochastic Oscillator
        IND_TEMA        = 37,   %  Triple Exponential Moving Average
        IND_TRIX        = 38,   %  Triple Exponential Moving Averages Oscillator
        IND_VIDYA       = 42,   %  Variable Index Dynamic Average
        IND_VOLUMES     = 34,   %  Volumes
        IND_WPR         = 35    %  Williams' Percent Ranges
      %% ENUM_DATATYPE
    
        TYPE_BOOL       = MtApi5.ENUM_DATATYPE.TYPE_BOOL
        TYPE_CHAR       = MtApi5.ENUM_DATATYPE.TYPE_CHAR
        TYPE_UCHAR      = MtApi5.ENUM_DATATYPE.TYPE_UCHAR
        TYPE_SHORT      = MtApi5.ENUM_DATATYPE.TYPE_SHORT
        TYPE_USHORT     = MtApi5.ENUM_DATATYPE.TYPE_USHORT
        TYPE_COLOR      = MtApi5.ENUM_DATATYPE.TYPE_COLOR
        TYPE_INT        = MtApi5.ENUM_DATATYPE.TYPE_INT
        TYPE_UINT       = MtApi5.ENUM_DATATYPE.TYPE_UINT
        TYPE_DATETIME   = MtApi5.ENUM_DATATYPE.TYPE_DATETIME
        TYPE_LONG       = MtApi5.ENUM_DATATYPE.TYPE_LONG
        TYPE_ULONG      = MtApi5.ENUM_DATATYPE.TYPE_ULONG
        TYPE_FLOAT      = MtApi5.ENUM_DATATYPE.TYPE_FLOAT
        TYPE_DOUBLE     = MtApi5.ENUM_DATATYPE.TYPE_DOUBLE
        TYPE_STRING     = MtApi5.ENUM_DATATYPE.TYPE_STRING  
   end
end