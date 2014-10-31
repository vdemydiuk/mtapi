using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtApi5
{
    public class MqlRates
    {
        public MqlRates(DateTime time, double open, double high, double low, double close, long tick_volume, int spread, long real_volume)
        {
            this.time = time;
            this.open = open;
            this.high = high;
            this.low = low;
            this.close = close;
            this.tick_volume = tick_volume;
            this.spread = spread;
            this.real_volume = real_volume;
        }

        public DateTime time { get; private set; }         // Period start time
        public double open { get; private set; }         // Open price
        public double high { get; private set; }         // The highest price of the period
        public double low { get; private set; }          // The lowest price of the period
        public double close { get; private set; }        // Close price
        public long tick_volume { get; private set; }  // Tick volume
        public int spread { get; private set; }       // Spread
        public long real_volume { get; private set; }  // Trade volume
    }
}
