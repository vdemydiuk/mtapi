using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtApi5
{
    public class Mt5QuoteEventArgs: EventArgs
    {
        public Mt5Quote Quote { get; private set; }

        public Mt5QuoteEventArgs(Mt5Quote quote)
        {
            Quote = quote;
        }
    }
}
