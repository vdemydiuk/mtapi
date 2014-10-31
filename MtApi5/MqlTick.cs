using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtApi5
{
    public class MqlTick
    {
        public MqlTick(DateTime time, double bid, double ask, double last, ulong volume)
        {
            this.time = time;
            this.bid = bid;
            this.ask = ask;
            this.last = last;
            this.volume = volume;
        }

        public DateTime time { get; private set; }          // Time of the last prices update
        public double bid { get; private set; }           // Current Bid price
        public double ask { get; private set; }           // Current Ask price
        public double last { get; private set; }          // Price of the last deal (Last)
        public ulong volume { get; private set; }        // Volume for the current Last price
    }
}
