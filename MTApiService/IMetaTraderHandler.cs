using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTApiService
{
    public interface IMetaTraderHandler
    {
        void SendTickToMetaTrader(int handle);
    }
}
