using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestServer
{
    enum CodeErrorTypes
    {
        ERR_NO_ERROR                    =   0,
        ERR_NO_RESULT                   =   1,
        ERR_COMMON_ERROR                =   2,
        ERR_INVALID_TRADE_PARAMETERS    =   3,
        ERR_SERVER_BUSY                 =   4,
        ERR_OLD_VERSION                 =   5,
        ERR_NO_CONNECTION               =   6,
        ERR_NOT_ENOUGH_RIGHTS           =   7,
        ERR_TOO_FREQUENT_REQUESTS       =   8,
        ERR_MALFUNCTIONAL_TRADE         =   9,
        ERR_ACCOUNT_DISABLED            =   64,
        ERR_INVALID_ACCOUNT             =   65,
        ERR_TRADE_TIMEOUT               =   128,
        ERR_INVALID_PRICE               =   129,
        ERR_INVALID_STOPS               =   130,
        ERR_INVALID_TRADE_VOLUME        =   131,
        ERR_MARKET_CLOSED               =   132,
        ERR_TRADE_DISABLED              =   133,
        ERR_NOT_ENOUGH_MONEY            =   134,
        ERR_PRICE_CHANGED               =   135,
        ERR_OFF_QUOTES                  =   136,
        ERR_BROKER_BUSY                 =   137,
        ERR_REQUOTE                     =   138,
        ERR_ORDER_LOCKED                =   139,
        ERR_LONG_POSITIONS_ONLY_ALLOWED =   140,
        ERR_TOO_MANY_REQUESTS           =   141,
        ERR_TRADE_MODIFY_DENIED         =   145,
        ERR_TRADE_CONTEXT_BUSY          =   146,
        ERR_TRADE_EXPIRATION_DENIED     =   147,
        ERR_TRADE_TOO_MANY_ORDERS       =   148,
        ERR_TRADE_HEDGE_PROHIBITED      =   149,
        ERR_TRADE_PROHIBITED_BY_FIFO    =   150
    }
}
