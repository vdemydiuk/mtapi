using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtApi
{
    static class ExtensionMethods
    {
        public static MtQuote Parse(this MTApiService.MtQuote quote)
        {
            return (quote != null) ? new MtQuote(quote.Instrument, quote.Bid, quote.Ask) : null;
        }
    }
}
