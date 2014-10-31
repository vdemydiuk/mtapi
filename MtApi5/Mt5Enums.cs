namespace MtApi5
{
    public enum ENUM_TRADE_REQUEST_ACTIONS
    {
        TRADE_ACTION_DEAL = 1,      //Place a trade order for an immediate execution with the specified parameters (market order)
        TRADE_ACTION_PENDING = 5,   //Place a trade order for an immediate execution with the specified parameters (market order)
        TRADE_ACTION_SLTP = 6,      //Place a trade order for an immediate execution with the specified parameters (market order)
        TRADE_ACTION_MODIFY = 7,    //Place a trade order for an immediate execution with the specified parameters (market order)
        TRADE_ACTION_REMOVE = 8     //Place a trade order for an immediate execution with the specified parameters (market order)
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
        ORDER_TYPE_SELL_STOP_LIMIT = 7,  //Upon reaching the order price, a pending Sell Limit order is places at the StopLimit price
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

    public enum ENUM_POSITION_PROPERTY_DOUBLE
    {
        POSITION_VOLUME = 3,            //Position volume
        POSITION_PRICE_OPEN = 4,        //Position open price
        POSITION_SL = 6,                //Stop Loss level of opened position
        POSITION_TP = 7,                //Take Profit level of opened position
        POSITION_PRICE_CURRENT = 5,     //Current price of the position symbol
        POSITION_COMMISSION = 8,        //Commission
        POSITION_SWAP = 9,              //Cumulative swap
        POSITION_PROFIT = 10            //Current profit
    }

    public enum ENUM_POSITION_PROPERTY_INTEGER
    {
        POSITION_TIME = 1,           //Position open time
        POSITION_TYPE = 2,           //Position type
        POSITION_MAGIC = 12,         //Position magic number
        POSITION_IDENTIFIER = 13     //Position identifier is a unique number that is assigned to every newly opened position and doesn't change during the entire lifetime of the position. Position turnover doesn't change its identifier.
    }

    public enum ENUM_POSITION_TYPE
    {
        POSITION_TYPE_BUY = 0,      //Buy
        POSITION_TYPE_SELL = 1      //Sell
    }

    public enum ENUM_POSITION_PROPERTY_STRING
    {
        POSITION_SYMBOL = 0,    //Symbol of the position
        POSITION_COMMENT = 11   //Position comment
    }

    public enum ENUM_ORDER_PROPERTY_DOUBLE
    {
        ORDER_VOLUME_INITIAL = 7,   //Order initial volume
        ORDER_VOLUME_CURRENT = 8,   //Order current volume
        ORDER_PRICE_OPEN = 9,       //Price specified in the order
        ORDER_SL = 12,               //Stop Loss value
        ORDER_TP = 13,               //Take Profit value
        ORDER_PRICE_CURRENT = 10,    //The current price of the order symbol
        ORDER_PRICE_STOPLIMIT = 11   //The Limit order price for the StopLimit order
    }

    public enum ENUM_ORDER_PROPERTY_STRING
    {
        ORDER_SYMBOL = 0,           //Symbol of the order
        ORDER_COMMENT = 16           //Order comment
    }

    public enum ENUM_ORDER_PROPERTY_INTEGER
    {
        ORDER_TIME_SETUP = 1,       //Order setup time
        ORDER_TYPE = 4,             //Order type
        ORDER_STATE = 14,            //Order state
        ORDER_TIME_EXPIRATION = 2,  //Order expiration time
        ORDER_TIME_DONE = 3,        //Order execution or cancellation time
        ORDER_TYPE_FILLING = 5,     //Order filling type
        ORDER_TYPE_TIME = 6,        //Order lifetime
        ORDER_MAGIC = 15,            //ID of an Expert Advisor that has placed the order (designed to ensure that each Expert Advisor places its own unique number)
        ORDER_POSITION_ID = 17       //Position identifier that is set to an order as soon as it is executed. Each executed order results in a deal that opens or modifies an already existing position. The identifier of exactly this position is set to the executed order at this moment.
    }

    public enum ENUM_DEAL_PROPERTY_DOUBLE
    {
        DEAL_VOLUME = 5,
        DEAL_PRICE = 6,
        DEAL_COMMISSION = 7,
        DEAL_SWAP = 8,
        DEAL_PROFIT = 9
    }

    public enum ENUM_DEAL_PROPERTY_STRING
    {
        DEAL_SYMBOL = 0,
        DEAL_COMMENT = 10
    }

    public enum ENUM_DEAL_TYPE
    {
        DEAL_TYPE_BUY = 0,
        DEAL_TYPE_SELL = 1,
        DEAL_TYPE_BALANCE = 2,
        DEAL_TYPE_CREDIT = 3,
        DEAL_TYPE_CHARGE = 4,
        DEAL_TYPE_CORRECTION = 5,
        DEAL_TYPE_BONUS = 6,
        DEAL_TYPE_COMMISSION = 7,
        DEAL_TYPE_COMMISSION_DAILY = 8,
        DEAL_TYPE_COMMISSION_MONTHLY = 9,
        DEAL_TYPE_COMMISSION_AGENT_DAILY = 10,
        DEAL_TYPE_COMMISSION_AGENT_MONTHLY = 11,
        DEAL_TYPE_INTEREST = 12,
        DEAL_TYPE_BUY_CANCELED = 13,
        DEAL_TYPE_SELL_CANCELED = 14
    }

    public enum ENUM_DEAL_ENTRY
    {
        DEAL_ENTRY_IN = 0,
        DEAL_ENTRY_OUT = 1,
        DEAL_ENTRY_INOUT = 2,
        DEAL_ENTRY_STATE = 255
    }

    public enum ENUM_DEAL_PROPERTY_INTEGER
    {
        DEAL_ORDER = 1,
        DEAL_TIME = 2,
        DEAL_TYPE = 3,
        DEAL_ENTRY = 4,
        DEAL_MAGIC = 11,
        DEAL_POSITION_ID = 12
    }

    public enum ENUM_ACCOUNT_INFO_INTEGER
    {
        ACCOUNT_LOGIN = 0,
        ACCOUNT_TRADE_MODE = 32,
        ACCOUNT_LEVERAGE = 35,
        ACCOUNT_LIMIT_ORDERS = 47,
        ACCOUNT_MARGIN_SO_MODE = 44,
        ACCOUNT_TRADE_ALLOWED = 33,
        ACCOUNT_TRADE_EXPERT = 34
    }

    public enum ENUM_ACCOUNT_INFO_DOUBLE
    {
        ACCOUNT_BALANCE = 37,
        ACCOUNT_CREDIT = 38,
        ACCOUNT_PROFIT = 39,
        ACCOUNT_EQUITY = 40,
        ACCOUNT_MARGIN = 41,
        ACCOUNT_FREEMARGIN = 42,
        ACCOUNT_MARGIN_LEVEL = 43,
        ACCOUNT_MARGIN_SO_CALL = 45,
        ACCOUNT_MARGIN_SO_SO = 46
    }

    public enum ENUM_ACCOUNT_INFO_STRING
    {
        ACCOUNT_NAME = 1,
        ACCOUNT_SERVER = 3,
        ACCOUNT_CURRENCY = 36,
        ACCOUNT_COMPANY = 2
    }

    public enum ENUM_ACCOUNT_TRADE_MODE
    {
        ACCOUNT_TRADE_MODE_DEMO = 0,
        ACCOUNT_TRADE_MODE_CONTEST = 1,
        ACCOUNT_TRADE_MODE_REAL = 2
    }

    public enum ENUM_ACCOUNT_STOPOUT_MODE
    {
        ACCOUNT_STOPOUT_MODE_PERCENT = 0,
        ACCOUNT_STOPOUT_MODE_MONEY = 1
    }

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

    public enum ENUM_SERIES_INFO_INTEGER
    {
        SERIES_BARS_COUNT = 0,
        SERIES_FIRSTDATE = 1,
        SERIES_LASTBAR_DATE = 5,
        SERIES_SERVER_FIRSTDATE = 2,
        SERIES_TERMINAL_FIRSTDATE = 3,
        SERIES_SYNCHRONIZED = 4
    }

    public enum ENUM_SYMBOL_INFO_INTEGER
    {
        SYMBOL_SELECT = 0,
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
        SYMBOL_EXPIRATION_MODE = 49,
        SYMBOL_FILLING_MODE = 50
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
        SYMBOL_POINT = 16,
        SYMBOL_TRADE_TICK_VALUE = 26,
        SYMBOL_TRADE_TICK_VALUE_PROFIT = 53,
        SYMBOL_TRADE_TICK_VALUE_LOSS = 54,
        SYMBOL_TRADE_TICK_SIZE = 27,
        SYMBOL_TRADE_CONTRACT_SIZE = 28,
        SYMBOL_VOLUME_MIN = 34,
        SYMBOL_VOLUME_MAX = 35,
        SYMBOL_VOLUME_STEP = 36,
        SYMBOL_VOLUME_LIMIT = 55,
        SYMBOL_SWAP_LONG = 38,
        SYMBOL_SWAP_SHORT = 39,
        SYMBOL_MARGIN_INITIAL = 42,
        SYMBOL_MARGIN_MAINTENANCE = 43,
        SYMBOL_MARGIN_LONG = 44,
        SYMBOL_MARGIN_SHORT = 45,
        SYMBOL_MARGIN_LIMIT = 46,
        SYMBOL_MARGIN_STOP = 47,
        SYMBOL_MARGIN_STOPLIMIT = 48,
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
        SYMBOL_SESSION_PRICE_LIMIT_MAX = 69
    }

    public enum ENUM_SYMBOL_INFO_STRING
    {
        SYMBOL_CURRENCY_BASE = 22,
        SYMBOL_CURRENCY_PROFIT = 23,
        SYMBOL_CURRENCY_MARGIN = 24,
        SYMBOL_BANK = 19,
        SYMBOL_DESCRIPTION = 20,
        SYMBOL_ISIN = 70,
        SYMBOL_PATH = 21
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

    public enum ENUM_BOOK_TYPE
    {
        BOOK_TYPE_SELL = 1,
        BOOK_TYPE_BUY = 2,
        BOOK_TYPE_SELL_MARKET = 3,
        BOOK_TYPE_BUY_MARKET = 4
    }
}
