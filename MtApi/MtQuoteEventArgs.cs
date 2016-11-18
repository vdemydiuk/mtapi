using System;

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
