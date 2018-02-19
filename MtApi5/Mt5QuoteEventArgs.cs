using System;

namespace MtApi5
{
    public class Mt5QuoteEventArgs: EventArgs
    {
        public Mt5Quote Quote { get; }

        public Mt5QuoteEventArgs(Mt5Quote quote)
        {
            Quote = quote;
        }
    }
}
