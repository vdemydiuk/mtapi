using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtApi
{
    public class MtQuoteEventArgs : EventArgs
    {
        public MtQuote Quote { get; private set; }

        public MtQuoteEventArgs(MtQuote quote)
        {
            Quote = quote;
        }
    }
}
