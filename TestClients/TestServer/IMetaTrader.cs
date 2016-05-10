using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestServer
{
    interface IMetaTrader
    {
        void Print(string msg);
        void MessageBoxA(string msg);

        string AccountName();
        int AccountNumber();
        bool IsDemo();
        bool IsConnected();
        string ErrorDescription(int errorCode);
        int OrderSend(string symbol, int cmd, double volume, double price, int slippage, double stoploss, double takeprofit
            , string comment, int magic, int expiration, int arrow_color);
        bool OrderClose(int ticket, double lots, double price, int slippage, int color);        
    }
}
