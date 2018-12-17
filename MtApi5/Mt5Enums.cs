﻿// ReSharper disable InconsistentNaming

namespace MtApi5
{

    // Chart Constants:

    #region Chart Timeframes
    
    public enum ENUM_TIMEFRAMES
    {
        PERIOD_CURRENT = 0,
        PERIOD_M1 = 1,
        PERIOD_M2 = 2,
        PERIOD_M3 = 3,
        PERIOD_M4 = 4,
        PERIOD_M5 = 5,
        PERIOD_M6 = 6,
        PERIOD_M10 = 10,
        PERIOD_M12 = 12,
        PERIOD_M15 = 15,
        PERIOD_M20 = 20,
        PERIOD_M30 = 30,
        PERIOD_H1 = 16385,
        PERIOD_H2 = 16386,
        PERIOD_H3 = 16387,
        PERIOD_H4 = 16388,
        PERIOD_H6 = 16390,
        PERIOD_H8 = 16392,
        PERIOD_H12 = 16396,
        PERIOD_D1 = 16408,
        PERIOD_W1 = 32769,
        PERIOD_MN1 = 49153
    }

    #endregion //Chart Timeframes


    #region  Charts  Properties

    public enum ENUM_CHART_PROPERTY_DOUBLE
    {
        CHART_SHIFT_SIZE        = 3,
        CHART_FIXED_POSITION    = 41,
        CHART_FIXED_MAX         = 8,
        CHART_FIXED_MIN         = 9,
        CHART_POINTS_PER_BAR    = 11,
        CHART_PRICE_MIN         = 108,
        CHART_PRICE_MAX         = 109
    }

    public enum ENUM_CHART_PROPERTY_INTEGER
    {
        CHART_SHOW                      = 46,
        CHART_IS_OBJECT                 = 111,
        CHART_BRING_TO_TOP              = 35,
        CHART_CONTEXT_MENU              = 50,
        CHART_CROSSHAIR_TOOL            = 49,
        CHART_MOUSE_SCROLL              = 42,
        CHART_EVENT_MOUSE_WHEEL         = 48,
        CHART_EVENT_MOUSE_MOVE          = 40,
        CHART_EVENT_OBJECT_CREATE       = 38,
        CHART_EVENT_OBJECT_DELETE       = 39,
        CHART_MODE                      = 0,
        CHART_FOREGROUND                = 1,
        CHART_SHIFT                     = 2,
        CHART_AUTOSCROLL                = 4,
        CHART_KEYBOARD_CONTROL          = 47,
        CHART_QUICK_NAVIGATION          = 45,
        CHART_SCALE                     = 5,
        CHART_SCALEFIX                  = 6,
        CHART_SCALEFIX_11               = 7,
        CHART_SCALE_PT_PER_BAR          = 10,
        CHART_SHOW_OHLC                 = 12,
        CHART_SHOW_BID_LINE             = 13,
        CHART_SHOW_ASK_LINE             = 14,
        CHART_SHOW_LAST_LINE            = 15,
        CHART_SHOW_PERIOD_SEP           = 16,
        CHART_SHOW_GRID                 = 17,
        CHART_SHOW_VOLUMES              = 18,
        CHART_SHOW_OBJECT_DESCR         = 19,
        CHART_VISIBLE_BARS              = 100,
        CHART_WINDOWS_TOTAL             = 101,
        CHART_WINDOW_IS_VISIBLE         = 102,
        CHART_WINDOW_HANDLE             = 103,
        CHART_WINDOW_YDISTANCE          = 110,
        CHART_FIRST_VISIBLE_BAR         = 104,
        CHART_WIDTH_IN_BARS             = 105,
        CHART_WIDTH_IN_PIXELS           = 106,
        CHART_HEIGHT_IN_PIXELS          = 107,
        CHART_COLOR_BACKGROUND          = 21,
        CHART_COLOR_FOREGROUND          = 22,
        CHART_COLOR_GRID                = 23,
        CHART_COLOR_VOLUME              = 24,
        CHART_COLOR_CHART_UP            = 25,
        CHART_COLOR_CHART_DOWN          = 26,
        CHART_COLOR_CHART_LINE          = 27,
        CHART_COLOR_CANDLE_BULL         = 28,
        CHART_COLOR_CANDLE_BEAR         = 29,
        CHART_COLOR_BID                 = 30,
        CHART_COLOR_ASK                 = 31,
        CHART_COLOR_LAST                = 32,
        CHART_COLOR_STOP_LEVEL          = 33,
        CHART_SHOW_TRADE_LEVELS         = 34,
        CHART_DRAG_TRADE_LEVELS         = 43,
        CHART_SHOW_DATE_SCALE           = 36,
        CHART_SHOW_PRICE_SCALE          = 37,
        CHART_SHOW_ONE_CLICK            = 44,
        CHART_IS_MAXIMIZED              = 115,
        CHART_IS_MINIMIZED              = 116
    }

    public enum ENUM_CHART_PROPERTY_STRING
    {
        CHART_COMMENT       = 20,
        CHART_EXPERT_NAME   = 113,
        CHART_SCRIPT_NAME   = 114
    }

    public enum ENUM_CHART_POSITION
    {
        CHART_BEGIN         = 0, // Chart beginning (the oldest prices)
        CHART_CURRENT_POS   = 1, // Current position
        CHART_END           = 2  // Chart end (the latest prices)
    }

    #endregion

    // Environment State:

    #region Client Terminal Properties

    public enum ENUM_TERMINAL_INFO_INTEGER
    {
        TERMINAL_BUILD = 5,
        TERMINAL_COMMUNITY_ACCOUNT = 23,
        TERMINAL_COMMUNITY_CONNECTION = 24,
        TERMINAL_CONNECTED = 6,
        TERMINAL_DLLS_ALLOWED = 7,
        TERMINAL_TRADE_ALLOWED = 8,
        TERMINAL_EMAIL_ENABLED = 9,
        TERMINAL_FTP_ENABLED = 10,
        TERMINAL_NOTIFICATIONS_ENABLED = 26,
        TERMINAL_MAXBARS = 11,
        TERMINAL_MQID = 22,
        TERMINAL_CODEPAGE = 12,
        TERMINAL_CPU_CORES = 21,
        TERMINAL_DISK_SPACE = 20,
        TERMINAL_MEMORY_PHYSICAL = 14,
        TERMINAL_MEMORY_TOTAL = 15,
        TERMINAL_MEMORY_AVAILABLE = 16,
        TERMINAL_MEMORY_USED = 17,
        TERMINAL_X64 = 18,
        TERMINAL_OPENCL_SUPPORT = 19,
        TERMINAL_SCREEN_DPI = 27,
        TERMINAL_PING_LAST = 29
    }

    public enum ENUM_TERMINAL_INFO_DOUBLE
    {
        TERMINAL_COMMUNITY_BALANCE = 25
    }

    public enum ENUM_TERMINAL_INFO_STRING
    {
        TERMINAL_LANGUAGE = 13,
        TERMINAL_COMPANY = 0,
        TERMINAL_NAME = 1,
        TERMINAL_PATH = 2,
        TERMINAL_DATA_PATH = 3,
        TERMINAL_COMMONDATA_PATH = 4
    }

    #endregion //Client Terminal Properties

    #region Symbol Properties

    public enum ENUM_SYMBOL_INFO_INTEGER
    {
        SYMBOL_CUSTOM = 78,
        SYMBOL_BACKGROUND_COLOR = 79,
        SYMBOL_CHART_MODE = 80,
        SYMBOL_SELECT = 0,
        SYMBOL_VISIBLE = 76,
        SYMBOL_SESSION_DEALS = 56,
        SYMBOL_SESSION_BUY_ORDERS = 60,
        SYMBOL_SESSION_SELL_ORDERS = 62,
        SYMBOL_VOLUME = 10,
        SYMBOL_VOLUMEHIGH = 11,
        SYMBOL_VOLUMELOW = 12,
        SYMBOL_TIME = 15,
        SYMBOL_DIGITS = 17,
        SYMBOL_SPREAD_FLOAT = 41,
        SYMBOL_SPREAD = 18,
        SYMBOL_TICKS_BOOKDEPTH = 25,
        SYMBOL_TRADE_CALC_MODE = 29,
        SYMBOL_TRADE_MODE = 30,
        SYMBOL_START_TIME = 51,
        SYMBOL_EXPIRATION_TIME = 52,
        SYMBOL_TRADE_STOPS_LEVEL = 31,
        SYMBOL_TRADE_FREEZE_LEVEL = 32,
        SYMBOL_TRADE_EXEMODE = 33,
        SYMBOL_SWAP_MODE = 37,
        SYMBOL_SWAP_ROLLOVER3DAYS = 40,
        SYMBOL_MARGIN_HEDGED_USE_LEG = 82,
        SYMBOL_EXPIRATION_MODE = 49,
        SYMBOL_FILLING_MODE = 50,
        SYMBOL_ORDER_MODE = 71,
        SYMBOL_ORDER_GTC_MODE = 81,
        SYMBOL_ORDER_CLOSEBY = 64,
        SYMBOL_OPTION_MODE = 75,
        SYMBOL_OPTION_RIGHT = 74,
    }

    public enum ENUM_SYMBOL_INFO_DOUBLE
    {
        SYMBOL_BID = 1,
        SYMBOL_BIDHIGH = 2,
        SYMBOL_BIDLOW = 3,
        SYMBOL_ASK = 4,
        SYMBOL_ASKHIGH = 5,
        SYMBOL_ASKLOW = 6,
        SYMBOL_LAST = 7,
        SYMBOL_LASTHIGH = 8,
        SYMBOL_LASTLOW = 9,
        SYMBOL_OPTION_STRIKE = 72,
        SYMBOL_POINT = 16,
        SYMBOL_TRADE_TICK_VALUE = 26,
        SYMBOL_TRADE_TICK_VALUE_PROFIT = 53,
        SYMBOL_TRADE_TICK_VALUE_LOSS = 54,
        SYMBOL_TRADE_TICK_SIZE = 27,
        SYMBOL_TRADE_CONTRACT_SIZE = 28,
        SYMBOL_TRADE_ACCRUED_INTEREST = 87,
        SYMBOL_TRADE_FACE_VALUE = 86,
        SYMBOL_TRADE_LIQUIDITY_RATE = 85,
        SYMBOL_VOLUME_MIN = 34,
        SYMBOL_VOLUME_MAX = 35,
        SYMBOL_VOLUME_STEP = 36,
        SYMBOL_VOLUME_LIMIT = 55,
        SYMBOL_SWAP_LONG = 38,
        SYMBOL_SWAP_SHORT = 39,
        SYMBOL_MARGIN_INITIAL = 42,
        SYMBOL_MARGIN_MAINTENANCE = 43,
        SYMBOL_MARGIN_LONG = 44,        //FIXME: Undocumented!
        SYMBOL_MARGIN_SHORT = 45,       //FIXME: Undocumented!
        SYMBOL_MARGIN_LIMIT = 46,       //FIXME: Undocumented!
        SYMBOL_MARGIN_STOP = 47,        //FIXME: Undocumented!
        SYMBOL_MARGIN_STOPLIMIT = 48,   //FIXME: Undocumented!
        SYMBOL_SESSION_VOLUME = 57,
        SYMBOL_SESSION_TURNOVER = 58,
        SYMBOL_SESSION_INTEREST = 59,
        SYMBOL_SESSION_BUY_ORDERS_VOLUME = 61,
        SYMBOL_SESSION_SELL_ORDERS_VOLUME = 63,
        SYMBOL_SESSION_OPEN = 64,
        SYMBOL_SESSION_CLOSE = 65,
        SYMBOL_SESSION_AW = 66,
        SYMBOL_SESSION_PRICE_SETTLEMENT = 67,
        SYMBOL_SESSION_PRICE_LIMIT_MIN = 68,
        SYMBOL_SESSION_PRICE_LIMIT_MAX = 69,
        SYMBOL_MARGIN_HEDGED = 77
    }

    public enum ENUM_SYMBOL_INFO_STRING
    {
        SYMBOL_BASIS = 73,
        SYMBOL_CURRENCY_BASE = 22,
        SYMBOL_CURRENCY_PROFIT = 23,
        SYMBOL_CURRENCY_MARGIN = 24,
        SYMBOL_BANK = 19,
        SYMBOL_DESCRIPTION = 20,
        SYMBOL_FORMULA = 84,
        SYMBOL_PAGE = 83,
        SYMBOL_ISIN = 70,
        SYMBOL_PATH = 21
    }

    public enum ENUM_SYMBOL_CHART_MODE
    {
        SYMBOL_CHART_MODE_BID = 0,
        SYMBOL_CHART_MODE_LAST = 1
    }

    public enum ENUM_SYMBOL_ORDER_GTC_MODE
    {
        SYMBOL_ORDERS_GTC = 0,
        SYMBOL_ORDERS_DAILY = 1,
        SYMBOL_ORDERS_DAILY_EXCLUDING_STOPS = 2
    }

    public enum ENUM_SYMBOL_CALC_MODE
    {
        SYMBOL_CALC_MODE_FOREX = 0,
        SYMBOL_CALC_MODE_FUTURES = 1,
        SYMBOL_CALC_MODE_CFD = 2,
        SYMBOL_CALC_MODE_CFDINDEX = 3,
        SYMBOL_CALC_MODE_CFDLEVERAGE = 4,
        SYMBOL_CALC_MODE_EXCH_STOCKS = 32,
        SYMBOL_CALC_MODE_EXCH_FUTURES = 33,
        SYMBOL_CALC_MODE_EXCH_FUTURES_FORTS = 34,
        SYMBOL_CALC_MODE_SERV_COLLATERAL = 64
    }

    public enum ENUM_SYMBOL_TRADE_MODE
    {
        SYMBOL_TRADE_MODE_DISABLED = 0,
        SYMBOL_TRADE_MODE_LONGONLY = 1,
        SYMBOL_TRADE_MODE_SHORTONLY = 2,
        SYMBOL_TRADE_MODE_CLOSEONLY = 3,
        SYMBOL_TRADE_MODE_FULL = 4
    }

    public enum ENUM_SYMBOL_TRADE_EXECUTION
    {
        SYMBOL_TRADE_EXECUTION_REQUEST = 0,
        SYMBOL_TRADE_EXECUTION_INSTANT = 1,
        SYMBOL_TRADE_EXECUTION_MARKET = 2,
        SYMBOL_TRADE_EXECUTION_EXCHANGE = 3
    }

    public enum ENUM_SYMBOL_SWAP_MODE
    {
        SYMBOL_SWAP_MODE_DISABLED = 0,
        SYMBOL_SWAP_MODE_POINTS = 1,
        SYMBOL_SWAP_MODE_CURRENCY_SYMBOL = 2,
        SYMBOL_SWAP_MODE_CURRENCY_MARGIN = 3,
        SYMBOL_SWAP_MODE_CURRENCY_DEPOSIT = 4,
        SYMBOL_SWAP_MODE_INTEREST_CURRENT = 5,
        SYMBOL_SWAP_MODE_INTEREST_OPEN = 6,
        SYMBOL_SWAP_MODE_REOPEN_CURRENT = 7,
        SYMBOL_SWAP_MODE_REOPEN_BID = 8
    }

    public enum ENUM_DAY_OF_WEEK
    {
        SUNDAY = 0,
        MONDAY = 1,
        TUESDAY = 2,
        WEDNESDAY = 3,
        THURSDAY = 4,
        FRIDAY = 5,
        SATURDAY = 6
    }

    public enum ENUM_SYMBOL_OPTION_RIGHT
    {
        SYMBOL_OPTION_RIGHT_CALL = 0,
        SYMBOL_OPTION_RIGHT_PUT = 1
    }

    public enum ENUM_SYMBOL_OPTION_MODE
    {
        SYMBOL_OPTION_MODE_EUROPEAN = 0,
        SYMBOL_OPTION_MODE_AMERICAN = 1
    }

    #endregion //Symbol Properties

    #region Account Properties

    public enum ENUM_ACCOUNT_INFO_INTEGER
    {
        ACCOUNT_LOGIN = 0,              //Account number
        ACCOUNT_TRADE_MODE = 32,        //Account trade mode
        ACCOUNT_LEVERAGE = 35,          //Account leverage
        ACCOUNT_LIMIT_ORDERS = 47,      //Maximum allowed number of active pending orders
        ACCOUNT_MARGIN_SO_MODE = 44,    //Mode for setting the minimal allowed margin
        ACCOUNT_TRADE_ALLOWED = 33,     //Allowed trade for the current account
        ACCOUNT_TRADE_EXPERT = 34,      //Allowed trade for an Expert Advisor
        ACCOUNT_MARGIN_MODE = 53        //Margin calculation mode
    }

    public enum ENUM_ACCOUNT_INFO_DOUBLE
    {
        ACCOUNT_BALANCE = 37,               //Account balance in the deposit currency
        ACCOUNT_CREDIT = 38,                //Account credit in the deposit currency
        ACCOUNT_PROFIT = 39,                //Current profit of an account in the deposit currency
        ACCOUNT_EQUITY = 40,                //Account equity in the deposit currency
        ACCOUNT_MARGIN = 41,                //Account margin used in the deposit currency
        ACCOUNT_MARGIN_FREE = 42,           //Free margin of an account in the deposit currency
        ACCOUNT_MARGIN_LEVEL = 43,          //Account margin level in percents
        ACCOUNT_MARGIN_SO_CALL = 45,        //Margin call level
        ACCOUNT_MARGIN_SO_SO = 46,          //Margin stop out level
        ACCOUNT_MARGIN_INITIAL = 48,        //Initial margin
        ACCOUNT_MARGIN_MAINTENANCE = 49,    //Maintenance margin
        ACCOUNT_ASSETS = 50,                //The current assets of an account
        ACCOUNT_LIABILITIES = 51,           //The current liabilities on an account
        ACCOUNT_COMMISSION_BLOCKED = 52     //The current blocked commission amount on an account
    }

    public enum ENUM_ACCOUNT_INFO_STRING
    {
        ACCOUNT_NAME = 1,           //Client name
        ACCOUNT_SERVER = 3,         //Trade server name
        ACCOUNT_CURRENCY = 36,      //Account currency
        ACCOUNT_COMPANY = 2         //Name of a company that serves the account
    }

    public enum ENUM_ACCOUNT_TRADE_MODE
    {
        ACCOUNT_TRADE_MODE_DEMO = 0,        //Demo account
        ACCOUNT_TRADE_MODE_CONTEST = 1,     //Contest account
        ACCOUNT_TRADE_MODE_REAL = 2         //Real account
    }

    public enum ENUM_ACCOUNT_STOPOUT_MODE
    {
        ACCOUNT_STOPOUT_MODE_PERCENT = 0,   //Account stop out mode in percents
        ACCOUNT_STOPOUT_MODE_MONEY = 1      //Account stop out mode in money
    }

    public enum ENUM_ACCOUNT_MARGIN_MODE
    {
        ACCOUNT_MARGIN_MODE_RETAIL_NETTING = 0,     //Used for the OTC markets to interpret positions in the "netting" mode
        ACCOUNT_MARGIN_MODE_EXCHANGE = 1,           //Used for the exchange markets
        ACCOUNT_MARGIN_MODE_RETAIL_HEDGING = 2      //Used for the exchange markets where individual positions are possible
    }

    #endregion //Account Properties

    // Trade Constants:

    #region History Database Properties

    public enum ENUM_SERIES_INFO_INTEGER
    {
        SERIES_BARS_COUNT = 0,              //Bars count for the symbol-period for the current moment
        SERIES_FIRSTDATE = 1,               //The very first date for the symbol-period for the current moment
        SERIES_LASTBAR_DATE = 5,            //Open time of the last bar of the symbol-period
        SERIES_SERVER_FIRSTDATE = 2,        //The very first date in the history of the symbol on the server regardless of the timeframe
        SERIES_TERMINAL_FIRSTDATE = 3,      //The very first date in the history of the symbol in the client terminal, regardless of the timeframe
        SERIES_SYNCHRONIZED = 4             //Symbol/period data synchronization flag for the current moment
    }

    #endregion //History Database Properties

    #region Order Properties

    public enum ENUM_ORDER_PROPERTY_INTEGER
    {
        ORDER_TICKET = 22,          //Order ticket. Unique number assigned to each order
        ORDER_TIME_SETUP = 1,       //Order setup time
        ORDER_TYPE = 4,             //Order type
        ORDER_STATE = 14,           //Order state
        ORDER_TIME_EXPIRATION = 2,  //Order expiration time
        ORDER_TIME_DONE = 3,        //Order execution or cancellation time
        ORDER_TIME_SETUP_MSC = 18,  //The time of placing an order for execution in milliseconds since 01.01.1970
        ORDER_TIME_DONE_MSC = 19,   //Order execution/cancellation time in milliseconds since 01.01.1970
        ORDER_TYPE_FILLING = 5,     //Order filling type
        ORDER_TYPE_TIME = 6,        //Order lifetime
        ORDER_MAGIC = 15,           //ID of an Expert Advisor that has placed the order (designed to ensure that each Expert Advisor places its own unique number)
        ORDER_REASON = 23,          //The reason or source for placing an order
        ORDER_POSITION_ID = 17,     //Position identifier that is set to an order as soon as it is executed. Each executed order results in a deal that opens or modifies an already existing position. The identifier of exactly this position is set to the executed order at this moment.
        ORDER_POSITION_BY_ID = 21   //Identifier of an opposite position used for closing by order ORDER_TYPE_CLOSE_BY
    }

    public enum ENUM_ORDER_PROPERTY_DOUBLE
    {
        ORDER_VOLUME_INITIAL = 7,   //Order initial volume
        ORDER_VOLUME_CURRENT = 8,   //Order current volume
        ORDER_PRICE_OPEN = 9,       //Price specified in the order
        ORDER_SL = 12,              //Stop Loss value
        ORDER_TP = 13,              //Take Profit value
        ORDER_PRICE_CURRENT = 10,   //The current price of the order symbol
        ORDER_PRICE_STOPLIMIT = 11  //The Limit order price for the StopLimit order
    }

    public enum ENUM_ORDER_PROPERTY_STRING
    {
        ORDER_SYMBOL = 0,           //Symbol of the order
        ORDER_COMMENT = 16,         //Order comment
        ORDER_EXTERNAL_ID = 20      //Order identifier in an external trading system (on the Exchange)
    }

    public enum ENUM_ORDER_TYPE
    {
        ORDER_TYPE_BUY = 0,             //Market Buy order
        ORDER_TYPE_SELL = 1,            //Market Sell order
        ORDER_TYPE_BUY_LIMIT = 2,       //Buy Limit pending order
        ORDER_TYPE_SELL_LIMIT = 3,      //Sell Limit pending order
        ORDER_TYPE_BUY_STOP = 4,        //Buy Stop pending order
        ORDER_TYPE_SELL_STOP = 5,       //Sell Stop pending order
        ORDER_TYPE_BUY_STOP_LIMIT = 6,  //Upon reaching the order price, a pending Buy Limit order is places at the StopLimit price
        ORDER_TYPE_SELL_STOP_LIMIT = 7, //Upon reaching the order price, a pending Sell Limit order is places at the StopLimit price
        ORDER_TYPE_CLOSE_BY = 8         //Order to close a position by an opposite one
    }

    public enum ENUM_ORDER_STATE
    {
        ORDER_STATE_STARTED = 0,            //Order checked, but not yet accepted by broker
        ORDER_STATE_PLACED = 1,             //Order accepted
        ORDER_STATE_CANCELED = 2,           //Order canceled by client
        ORDER_STATE_PARTIAL = 3,            //Order partially executed
        ORDER_STATE_FILLED = 4,             //Order fully executed
        ORDER_STATE_REJECTED = 5,           //Order rejected
        ORDER_STATE_EXPIRED = 6,            //Order expired
        ORDER_STATE_REQUEST_ADD = 7,        //Order is being registered (placing to the trading system)
        ORDER_STATE_REQUEST_MODIFY = 8,     //Order is being modified (changing its parameters)
        ORDER_STATE_REQUEST_CANCEL = 9      //Order is being deleted (deleting from the trading system)
    }

    public enum ENUM_ORDER_TYPE_FILLING
    {
        ORDER_FILLING_FOK = 0,
        ORDER_FILLING_IOC = 1,
        ORDER_FILLING_RETURN = 2
    }

    public enum ENUM_ORDER_TYPE_TIME
    {
        ORDER_TIME_GTC = 0,
        ORDER_TIME_DAY = 1,
        ORDER_TIME_SPECIFIED = 2,
        ORDER_TIME_SPECIFIED_DAY = 3
    }

    public enum ENUM_ORDER_REASON
    {
        ORDER_REASON_CLIENT = 0,    //The order was placed from a desktop terminal
        ORDER_REASON_MOBILE = 1,    //The order was placed from a mobile application
        ORDER_REASON_WEB = 2,       //The order was placed from a web platform
        ORDER_REASON_EXPERT = 3,    //The order was placed from an MQL5-program, i.e. by an Expert Advisor or a script
        ORDER_REASON_SL = 4,        //The order was placed as a result of Stop Loss activation
        ORDER_REASON_TP = 5,        //The order was placed as a result of Take Profit activation
        ORDER_REASON_SO = 6         //The order was placed as a result of the Stop Out event
    }

    #endregion //Order Properties

    #region Position Properties

    public enum ENUM_POSITION_PROPERTY_INTEGER
    {
        POSITION_TICKET = 17,           //Position ticket
        POSITION_TIME = 1,              //Position open time
        POSITION_TIME_MSC = 14,         //Position opening time in milliseconds since 01.01.1970
        POSITION_TIME_UPDATE = 15,      //Position changing time in seconds since 01.01.1970
        POSITION_TIME_UPDATE_MSC = 16,  //Position changing time in milliseconds since 01.01.1970
        POSITION_TYPE = 2,              //Position type
        POSITION_MAGIC = 12,            //Position magic number
        POSITION_IDENTIFIER = 13,       //Position identifier is a unique number that is assigned to every newly opened position and doesn't change during the entire lifetime of the position. Position turnover doesn't change its identifier.
        POSITION_REASON = 18            //The reason for opening a position
    }

    public enum ENUM_POSITION_PROPERTY_DOUBLE
    {
        POSITION_VOLUME = 3,            //Position volume
        POSITION_PRICE_OPEN = 4,        //Position open price
        POSITION_SL = 6,                //Stop Loss level of opened position
        POSITION_TP = 7,                //Take Profit level of opened position
        POSITION_PRICE_CURRENT = 5,     //Current price of the position symbol
        POSITION_COMMISSION = 8,        //FIXME: Undocumented!
        POSITION_SWAP = 9,              //Cumulative swap
        POSITION_PROFIT = 10            //Current profit
    }

    public enum ENUM_POSITION_PROPERTY_STRING
    {
        POSITION_SYMBOL = 0,    //Symbol of the position
        POSITION_COMMENT = 11   //Position comment
    }

    public enum ENUM_POSITION_TYPE
    {
        POSITION_TYPE_BUY = 0,      //Buy
        POSITION_TYPE_SELL = 1      //Sell
    }

    public enum ENUM_POSITION_REASON
    {
        POSITION_REASON_CLIENT = 0, //The position was opened as a result of activation of an order placed from a desktop terminal
        POSITION_REASON_MOBILE = 1, //The position was opened as a result of activation of an order placed from a mobile application
        POSITION_REASON_WEB = 2,    //The position was opened as a result of activation of an order placed from the web platform
        POSITION_REASON_EXPERT = 3  //The position was opened as a result of activation of an order placed from an MQL5 program
    }

    #endregion //Position Properties

    #region Deal Properties

    public enum ENUM_DEAL_PROPERTY_INTEGER
    {
        DEAL_TICKET = 15,           //Deal ticket. Unique number assigned to each deal
        DEAL_ORDER = 1,             //Deal order number
        DEAL_TIME = 2,              //Deal time
        DEAL_TIME_MSC = 13,         //The time of a deal execution in milliseconds since 01.01.1970
        DEAL_TYPE = 3,              //Deal type
        DEAL_ENTRY = 4,             //Deal entry - entry in, entry out, reverse
        DEAL_MAGIC = 11,            //Deal magic number
        DEAL_REASON = 16,           //The reason or source for deal execution
        DEAL_POSITION_ID = 12       //Identifier of a position
    }

    public enum ENUM_DEAL_PROPERTY_DOUBLE
    {
        DEAL_VOLUME = 5,        //Deal volume
        DEAL_PRICE = 6,         //Deal price
        DEAL_COMMISSION = 7,    //Deal commission
        DEAL_SWAP = 8,          //Cumulative swap on close
        DEAL_PROFIT = 9         //Deal profit
    }

    public enum ENUM_DEAL_PROPERTY_STRING
    {
        DEAL_SYMBOL = 0,        //Deal symbol
        DEAL_COMMENT = 10,      //Deal comment
        DEAL_EXTERNAL_ID = 14   //Deal identifier in an external trading system (on the Exchange)
    }

    public enum ENUM_DEAL_TYPE
    {
        DEAL_TYPE_BUY = 0,                          //Buy
        DEAL_TYPE_SELL = 1,                         //Sell
        DEAL_TYPE_BALANCE = 2,                      //Balance
        DEAL_TYPE_CREDIT = 3,                       //Credit
        DEAL_TYPE_CHARGE = 4,                       //Additional charge
        DEAL_TYPE_CORRECTION = 5,                   //Correction
        DEAL_TYPE_BONUS = 6,                        //Bonus
        DEAL_TYPE_COMMISSION = 7,                   //Additional commission
        DEAL_TYPE_COMMISSION_DAILY = 8,             //Daily commission
        DEAL_TYPE_COMMISSION_MONTHLY = 9,           //Monthly commission
        DEAL_TYPE_COMMISSION_AGENT_DAILY = 10,      //Daily agent commission
        DEAL_TYPE_COMMISSION_AGENT_MONTHLY = 11,    //Monthly agent commission
        DEAL_TYPE_INTEREST = 12,                    //Interest rate
        DEAL_TYPE_BUY_CANCELED = 13,                //Canceled buy deal
        DEAL_TYPE_SELL_CANCELED = 14,               //Canceled sell deal
        DEAL_DIVIDEND = 15,                         //Dividend operations
        DEAL_DIVIDEND_FRANKED = 16,                 //Franked (non-taxable) dividend operations
        DEAL_TAX = 17                               //Tax charges
    }

    public enum ENUM_DEAL_ENTRY
    {
        DEAL_ENTRY_IN = 0,          //Entry in
        DEAL_ENTRY_OUT = 1,         //Entry out
        DEAL_ENTRY_INOUT = 2,       //Reverse
        DEAL_ENTRY_STATE = 255      //Close a position by an opposite one
    }

    public enum ENUM_DEAL_REASON
    {
        DEAL_REASON_CLIENT = 0,         //The deal was executed as a result of activation of an order placed from a desktop terminal
        DEAL_REASON_MOBILE = 1,         //The deal was executed as a result of activation of an order placed from a mobile application
        DEAL_REASON_WEB = 2,            //The deal was executed as a result of activation of an order placed from the web platform
        DEAL_REASON_EXPERT = 3,         //The deal was executed as a result of activation of an order placed from an MQL5 program, i.e. an Expert Advisor or a script
        DEAL_REASON_SL = 4,             //The deal was executed as a result of Stop Loss activation
        DEAL_REASON_TP = 5,             //The deal was executed as a result of Take Profit activation
        DEAL_REASON_SO = 6,             //The deal was executed as a result of the Stop Out event
        DEAL_REASON_ROLLOVER = 7,       //The deal was executed due to a rollover
        DEAL_REASON_VMARGIN = 8,        //The deal was executed after charging the variation margin
        DEAL_REASON_SPLIT = 9           //The deal was executed after the split (price reduction) of an instrument, which had an open position during split announcement
    }

    #endregion //Deal Properties

    #region Trade Operation Types

    public enum ENUM_TRADE_REQUEST_ACTIONS
    {
        TRADE_ACTION_DEAL = 1,      //Place a trade order for an immediate execution with the specified parameters (market order)
        TRADE_ACTION_PENDING = 5,   //Place a trade order for the execution under specified conditions (pending order)
        TRADE_ACTION_SLTP = 6,      //Modify Stop Loss and Take Profit values of an opened position
        TRADE_ACTION_MODIFY = 7,    //Modify the parameters of the order placed previously
        TRADE_ACTION_REMOVE = 8,    //Delete the pending order placed previously
        TRADE_ACTION_CLOSE_BY = 10  //Close a position by an opposite one
    }

    #endregion //Trade Operation Types

    #region Trade Transaction Types

    public enum ENUM_TRADE_TRANSACTION_TYPE
    {
        TRADE_TRANSACTION_ORDER_ADD = 0,            //Adding a new open order
        TRADE_TRANSACTION_ORDER_UPDATE = 1,         //Updating an open order. The updates include not only evident changes from the client terminal or a trade server sides but also changes of an order state when setting it (for example, transition from ORDER_STATE_STARTED to ORDER_STATE_PLACED or from ORDER_STATE_PLACED to ORDER_STATE_PARTIAL, etc.).
        TRADE_TRANSACTION_ORDER_DELETE = 2,         //Removing an order from the list of the open ones. An order can be deleted from the open ones as a result of setting an appropriate request or execution (filling) and moving to the history.
        TRADE_TRANSACTION_DEAL_ADD = 6,             //Adding a deal to the history. The action is performed as a result of an order execution or performing operations with an account balance.
        TRADE_TRANSACTION_DEAL_UPDATE = 7,          //Updating a deal in the history. There may be cases when a previously executed deal is changed on a server. For example, a deal has been changed in an external trading system (exchange) where it was previously transferred by a broker.
        TRADE_TRANSACTION_DEAL_DELETE = 8,          //Deleting a deal from the history. There may be cases when a previously executed deal is deleted from a server. For example, a deal has been deleted in an external trading system (exchange) where it was previously transferred by a broker.
        TRADE_TRANSACTION_HISTORY_ADD = 3,          //Adding an order to the history as a result of execution or cancellation.
        TRADE_TRANSACTION_HISTORY_UPDATE = 4,       //Changing an order located in the orders history. This type is provided for enhancing functionality on a trade server side.
        TRADE_TRANSACTION_HISTORY_DELETE = 5,       //Deleting an order from the orders history. This type is provided for enhancing functionality on a trade server side.
        TRADE_TRANSACTION_POSITION = 9,             //Changing a position not related to a deal execution. This type of transaction shows that a position has been changed on a trade server side. Position volume, open price, Stop Loss and Take Profit levels can be changed. Data on changes are submitted in MqlTradeTransaction structure via OnTradeTransaction handler. Position change (adding, changing or closing), as a result of a deal execution, does not lead to the occurrence of TRADE_TRANSACTION_POSITION transaction.
        TRADE_TRANSACTION_REQUEST = 10              //Notification of the fact that a trade request has been processed by a server and processing result has been received. Only type field (trade transaction type) must be analyzed for such transactions in MqlTradeTransaction structure. The second and third parameters of OnTradeTransaction (request and result) must be analyzed for additional data.
    }

    #endregion //Trade Transaction Types

    #region Trade Orders in Depth Of Market

    public enum ENUM_BOOK_TYPE
    {
        BOOK_TYPE_SELL = 1,             //Sell order (Offer)
        BOOK_TYPE_BUY = 2,              //Buy order (Bid)
        BOOK_TYPE_SELL_MARKET = 3,      //Sell order by Market
        BOOK_TYPE_BUY_MARKET = 4        //Buy order by Market
    }

    #endregion //Trade Orders in Depth Of Market

    #region Object Types

    public enum ENUM_OBJECT
    {
        OBJ_VLINE = 0,              // Vertical Line
        OBJ_HLINE = 1,              // Horizontal Line
        OBJ_TREND = 2,              // Trend Line
        OBJ_TRENDBYANGLE = 3,       // Trend Line By Angle
        OBJ_CYCLES = 4,             // Cycle Lines
        OBJ_ARROWED_LINE = 108,     // Arrowed Line
        OBJ_CHANNEL = 5,            // Equidistant Channel
        OBJ_STDDEVCHANNEL = 6,      // Standard Deviation Channel
        OBJ_REGRESSION = 7,         // Linear Regression Channel
        OBJ_PITCHFORK = 8,          // Andrews Pitchfork
        OBJ_GANNLINE = 9,           // Gann Line
        OBJ_GANNFAN = 10,           // Gann Fan
        OBJ_GANNGRID = 11,          // Gann Grid
        OBJ_FIBO = 12,              // Fibonacci Retracement
        OBJ_FIBOTIMES = 13,         // Fibonacci Time Zones
        OBJ_FIBOFAN = 14,           // Fibonacci Fan
        OBJ_FIBOARC = 15,           // Fibonacci Arcs
        OBJ_FIBOCHANNEL = 16,       // Fibonacci Channel
        OBJ_EXPANSION = 17,         // Fibonacci Expansion
        OBJ_ELLIOTWAVE5 = 18,       // Elliott Motive Wave
        OBJ_ELLIOTWAVE3 = 19,       // Elliott Correction Wave
        OBJ_RECTANGLE = 20,         // Rectangle
        OBJ_TRIANGLE = 21,          // Triangle
        OBJ_ELLIPSE = 22,           // Ellipse
        OBJ_ARROW_THUMB_UP = 23,    // Thumbs Up
        OBJ_ARROW_THUMB_DOWN = 24,  // Thumbs Down
        OBJ_ARROW_UP = 25,          // Arrow Up
        OBJ_ARROW_DOWN = 26,        // Arrow Down
        OBJ_ARROW_STOP = 27,        // Stop Sign
        OBJ_ARROW_CHECK = 28,       // Check Sign
        OBJ_ARROW_LEFT_PRICE = 29,  // Left Price Label
        OBJ_ARROW_RIGHT_PRICE = 30, // Right Price Label
        OBJ_ARROW_BUY = 31,         // Buy Sign
        OBJ_ARROW_SELL = 32,        // Sell Sign
        OBJ_ARROW = 100,            // Arrow
        OBJ_TEXT = 101,             // Text
        OBJ_LABEL = 102,            // Label
        OBJ_BUTTON = 103,           // Button
        OBJ_CHART = 104,            // Chart
        OBJ_BITMAP = 105,           // Bitmap
        OBJ_BITMAP_LABEL = 106,     // Bitmap Label
        OBJ_EDIT = 107,             // Edit
        OBJ_EVENT = 109,            // The "Event" object corresponding to an event in the economic calendar
        OBJ_RECTANGLE_LABEL = 110   // The "Rectangle label" object for creating and designing the custom graphical interface.
    }

    #endregion // Object Types

    #region Object Properties

    public enum ENUM_OBJECT_PROPERTY_DOUBLE
    {
        OBJPROP_PRICE = 9,          // Price coordinate
        OBJPROP_LEVELVALUE = 204,   // Level value
        OBJPROP_SCALE = 1006,       // Scale (properties of Gann objects and Fibonacci Arcs)
        OBJPROP_ANGLE = 1007,       // Angle.  For the objects with no angle specified, created from a program, the value is equal to EMPTY_VALUE
        OBJPROP_DEVIATION = 1010    // Deviation for the Standard Deviation Channel
    }

    public enum ENUM_OBJECT_PROPERTY_INTEGER
    {
        OBJPROP_COLOR = 0,          // Color
        OBJPROP_STYLE = 1,          // Style
        OBJPROP_WIDTH = 2,          // Line thickness
        OBJPROP_BACK = 3,           // Object in the background
        OBJPROP_ZORDER = 207,       // Priority of a graphical object for receiving events of clicking on a chart (CHARTEVENT_CLICK). The default zero value is set when creating an object; the priority can be increased if necessary. When objects are placed one atop another, only one of them with the highest priority will receive the CHARTEVENT_CLICK event.
        OBJPROP_FILL = 1031,        // Fill an object with color (for OBJ_RECTANGLE, OBJ_TRIANGLE, OBJ_ELLIPSE, OBJ_CHANNEL, OBJ_STDDEVCHANNEL, OBJ_REGRESSION)
        OBJPROP_HIDDEN = 208,       // Prohibit showing of the name of a graphical object in the list of objects from the terminal menu "Charts" - "Objects" - "List of objects". The true value allows to hide an object from the list. By default, true is set to the objects that display calendar events, trading history and to the objects created from MQL5 programs. To see such graphical objects and access their properties, click on the "All" button in the "List of objects" window.
        OBJPROP_SELECTED = 4,       // Object is selected
        OBJPROP_READONLY = 1028,    // Ability to edit text in the Edit object
        OBJPROP_TYPE = 7,           // Object type
        OBJPROP_TIME = 8,           // Time coordinate
        OBJPROP_SELECTABLE = 10,    // Object availability
        OBJPROP_CREATETIME = 11,    // Time of object creation
        OBJPROP_LEVELS = 200,       // Number of levels
        OBJPROP_LEVELCOLOR = 201,   // Color of the line-level
        OBJPROP_LEVELSTYLE = 202,   // Style of the line-level
        OBJPROP_LEVELWIDTH = 203,   // Thickness of the line-level
        OBJPROP_ALIGN = 1036,       // Horizontal text alignment in the "Edit" object (OBJ_EDIT)
        OBJPROP_FONTSIZE = 1002,    // Font size
        OBJPROP_RAY_LEFT = 1003,    // Ray goes to the left
        OBJPROP_RAY_RIGHT = 1004,   // Ray goes to the right
        OBJPROP_RAY = 1032,         // A vertical line goes through all the windows of a chart
        OBJPROP_ELLIPSE = 1005,     // Showing the full ellipse of the Fibonacci Arc object (OBJ_FIBOARC)
        OBJPROP_ARROWCODE = 1008,   // Arrow code for the Arrow object
        OBJPROP_TIMEFRAMES = 12,    // Visibility of an object at timeframes
        OBJPROP_ANCHOR = 1011,      // Location of the anchor point of a graphical object
        OBJPROP_XDISTANCE = 1012,   // The distance in pixels along the X axis from the binding corner
        OBJPROP_YDISTANCE = 1013,   // The distance in pixels along the Y axis from the binding corner
        OBJPROP_DIRECTION = 1014,   // Trend of the Gann object
        OBJPROP_DEGREE = 1015,      // Level of the Elliott Wave Marking
        OBJPROP_DRAWLINES = 1016,   // Displaying lines for marking the Elliott Wave
        OBJPROP_STATE = 1018,       // Button state (pressed / depressed)
        OBJPROP_CHART_ID = 1030,    // ID of the "Chart" object (OBJ_CHART). It allows working with the properties of this object like with a normal chart using the functions described in Chart Operations, but there some exceptions.
        OBJPROP_XSIZE = 1019,       // The object's width along the X axis in pixels. Specified for  OBJ_LABEL (read only), OBJ_BUTTON, OBJ_CHART, OBJ_BITMAP, OBJ_BITMAP_LABEL, OBJ_EDIT, OBJ_RECTANGLE_LABEL objects.
        OBJPROP_YSIZE = 1020,       // The object's height along the Y axis in pixels. Specified for  OBJ_LABEL (read only), OBJ_BUTTON, OBJ_CHART, OBJ_BITMAP, OBJ_BITMAP_LABEL, OBJ_EDIT, OBJ_RECTANGLE_LABEL objects.
        OBJPROP_XOFFSET = 1033,     // The X coordinate of the upper left corner of the rectangular visible area in the graphical objects "Bitmap Label" and "Bitmap" (OBJ_BITMAP_LABEL and OBJ_BITMAP). The value is set in pixels relative to the upper left corner of the original image.
        OBJPROP_YOFFSET = 1034,     // The Y coordinate of the upper left corner of the rectangular visible area in the graphical objects "Bitmap Label" and "Bitmap" (OBJ_BITMAP_LABEL and OBJ_BITMAP). The value is set in pixels relative to the upper left corner of the original image.
        OBJPROP_PERIOD = 1022,      // Timeframe for the Chart object
        OBJPROP_DATE_SCALE = 1023,  // Displaying the time scale for the Chart object
        OBJPROP_PRICE_SCALE = 1024, // Displaying the price scale for the Chart object
        OBJPROP_CHART_SCALE = 1027, // The scale for the Chart object
        OBJPROP_BGCOLOR = 1025,     // The background color for  OBJ_EDIT, OBJ_BUTTON, OBJ_RECTANGLE_LABEL
        OBJPROP_CORNER = 1026,      // The corner of the chart to link a graphical object
        OBJPROP_BORDER_TYPE = 1029, // Border type for the "Rectangle label" object
        OBJPROP_BORDER_COLOR = 1035 // Border color for the OBJ_EDIT and OBJ_BUTTON objects
    }

    public enum ENUM_OBJECT_PROPERTY_STRING
    {
        OBJPROP_NAME = 5,           // Object name
        OBJPROP_TEXT = 6,           // Description of the object (the text contained in the object)
        OBJPROP_TOOLTIP = 206,      // The text of a tooltip. If the property is not set, then the tooltip generated automatically by the terminal is shown. A tooltip can be disabled by setting the "\n" (line feed) value to it
        OBJPROP_LEVELTEXT = 205,    // Level description
        OBJPROP_FONT = 1001,        // Font
        OBJPROP_BMPFILE = 1017,     // The name of BMP-file for Bitmap Label.
        OBJPROP_SYMBOL = 1021       // Symbol for the Chart object
    }

    public enum ENUM_BORDER_TYPE
    {
        BORDER_FLAT = 0,    // Flat form
        BORDER_RAISED = 1,  // Prominent form
        BORDER_SUNKEN = 2   // Concave form
    }

    public enum ENUM_ALIGN_MODE
    {
        ALIGN_LEFT = 1,     // Left alignment
        ALIGN_CENTER = 2,   // Centered (only for the Edit object)
        ALIGN_RIGHT = 0,    // Right alignment
    }
    #endregion //Object Properties

    #region Price Constants

    public enum ENUM_APPLIED_PRICE
    {
        PRICE_CLOSE = 1,    //Close price
        PRICE_OPEN = 2,     //Open price
        PRICE_HIGH = 3,     //The maximum price for the period
        PRICE_LOW = 4,      //The minimum price for the period
        PRICE_MEDIAN = 5,   //Median price, (high + low)/2
        PRICE_TYPICAL = 6,  //Typical price, (high + low + close)/3
        PRICE_WEIGHTED = 7  //Average price, (high + low + close + close)/4
    }

    public enum ENUM_APPLIED_VOLUME
    {
        VOLUME_TICK = 0,    //Tick volume
        VOLUME_REAL = 1     //Trade volume
    }

    public enum ENUM_STO_PRICE
    {
        STO_LOWHIGH = 0,    //Calculation is based on Low/High prices
        STO_CLOSECLOSE = 1  //Calculation is based on Close/Close prices
    }

    #endregion //Price Constants

    #region Smoothing Methods

    public enum ENUM_MA_METHOD
    {
        MODE_SMA = 0,   //Simple averaging
        MODE_EMA = 1,   //Exponential averaging
        MODE_SMMA = 2,  //Smoothed averaging
        MODE_LWMA = 3   //Linear-weighted averaging
    }

    #endregion //Smoothing Methods

    #region Indicator constants

    public enum ENUM_INDICATOR
    {
        IND_AC          = 5,    // Accelerator Oscillator
        IND_AD          = 6,    // Accumulation/Distribution
        IND_ADX         = 8,    // Average Directional Index
        IND_ADXW        = 9,    // ADX by Welles Wilder
        IND_ALLIGATOR   = 7,    // Alligator
        IND_AMA         = 40,   // Adaptive Moving Average
        IND_AO          = 11,   // Awesome Oscillator
        IND_ATR         = 10,   // Average True Range
        IND_BANDS       = 13,   // Bollinger Bands®
        IND_BEARS       = 12,   // Bears Power
        IND_BULLS       = 14,   // Bulls Power
        IND_BWMFI       = 22,   // Market Facilitation Index
        IND_CCI         = 15,   // Commodity Channel Index
        IND_CHAIKIN     = 41,   // Chaikin Oscillator
        IND_CUSTOM      = 43,   // Custom indicator
        IND_DEMA        = 36,   // Double Exponential Moving Average
        IND_DEMARKER    = 16,   // DeMarker
        IND_ENVELOPES   = 17,   // Envelopes
        IND_FORCE       = 18,   // Force Index
        IND_FRACTALS    = 19,   // Fractals
        IND_FRAMA       = 39,   // Fractal Adaptive Moving Average
        IND_GATOR       = 20,   // Gator Oscillator
        IND_ICHIMOKU    = 21,   // Ichimoku Kinko Hyo
        IND_MA          = 26,   // Moving Average
        IND_MACD        = 23,   // MACD
        IND_MFI         = 25,   // Money Flow Index
        IND_MOMENTUM    = 24,   // Momentum
        IND_OBV         = 28,   // On Balance Volume
        IND_OSMA        = 27,   // OsMA
        IND_RSI         = 30,   // Relative Strength Index
        IND_RVI         = 31,   // Relative Vigor Index
        IND_SAR         = 29,   // Parabolic SAR
        IND_STDDEV      = 32,   // Standard Deviation
        IND_STOCHASTIC  = 33,   // Stochastic Oscillator
        IND_TEMA        = 37,   // Triple Exponential Moving Average
        IND_TRIX        = 38,   // Triple Exponential Moving Averages Oscillator
        IND_VIDYA       = 42,   // Variable Index Dynamic Average
        IND_VOLUMES     = 34,   // Volumes
        IND_WPR         = 35    // Williams' Percent Ranges
    }

    public enum ENUM_DATATYPE
    {
        TYPE_BOOL       = 1,
        TYPE_CHAR       = 2,
        TYPE_UCHAR      = 3,
        TYPE_SHORT      = 4,
        TYPE_USHORT     = 5,
        TYPE_COLOR      = 6,
        TYPE_INT        = 7,
        TYPE_UINT       = 8,
        TYPE_DATETIME   = 9,
        TYPE_LONG       = 10,
        TYPE_ULONG      = 11,
        TYPE_FLOAT      = 12,
        TYPE_DOUBLE     = 13,
        TYPE_STRING     = 14
    }
    #endregion
}
