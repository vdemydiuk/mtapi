using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtApi
{
    class MtTypes
    {
    }

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
}
