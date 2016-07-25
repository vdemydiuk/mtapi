namespace MtApi
{
    public enum ENUM_TERMINAL_INFO_STRING
    {
        TERMINAL_LANGUAGE           = 13,
        TERMINAL_COMPANY            = 0,
        TERMINAL_NAME               = 1,
        TERMINAL_PATH               = 2,
        TERMINAL_DATA_PATH          = 3,
        TERMINAL_COMMONDATA_PATH    = 4
    }

    public enum ENUM_SYMBOL_INFO_STRING
    {
        SYMBOL_CURRENCY_BASE        = 22,
        SYMBOL_CURRENCY_PROFIT      = 23,
        SYMBOL_CURRENCY_MARGIN      = 24,
        SYMBOL_DESCRIPTION          = 20,
        SYMBOL_PATH                 = 21
    }

    public enum ENUM_TIMEFRAMES
    {
        PERIOD_CURRENT  = 0,
        PERIOD_M1       = 1,
        PERIOD_M2       = 2,
        PERIOD_M3       = 3,
        PERIOD_M4       = 4,
        PERIOD_M5       = 5,
        PERIOD_M6       = 6,
        PERIOD_M10      = 10,
        PERIOD_M12      = 12,
        PERIOD_M15      = 15,
        PERIOD_M20      = 20,
        PERIOD_M30      = 30,
        PERIOD_H1       = 60,
        PERIOD_H2       = 120,
        PERIOD_H3       = 180,
        PERIOD_H4       = 240,
        PERIOD_H6       = 360,
        PERIOD_H8       = 480,
        PERIOD_H12      = 720,
        PERIOD_D1       = 1440,
        PERIOD_W1       = 10080,
        PERIOD_MN1      = 43200
    }
}
