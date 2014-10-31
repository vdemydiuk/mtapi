using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtApi
{
    public class MtQuote
    {
        public string Instrument { get; private set; }
        public double Bid { get; private set; }
        public double Ask { get; private set; }

        public MtQuote(string instrument, double bid, double ask)
        {
            Instrument = instrument;
            Bid = bid;
            Ask = ask;
        }
    }
}
