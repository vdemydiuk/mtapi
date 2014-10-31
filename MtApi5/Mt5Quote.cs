using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtApi5
{
    public class Mt5Quote
    {
        public string Instrument { get; private set; }
        public double Bid { get; private set; }
        public double Ask { get; private set; }

        public Mt5Quote(string instrument, double bid, double ask)
        {
            Instrument = instrument;
            Bid = bid;
            Ask = ask;
        }
    }
}
