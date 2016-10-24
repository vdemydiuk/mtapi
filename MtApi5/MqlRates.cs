using System;

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

        public MqlRates()
        {
        }

        public DateTime time { get; set; }         // Period start time
        public double open { get; set; }         // Open price
        public double high { get; set; }         // The highest price of the period
        public double low { get; set; }          // The lowest price of the period
        public double close { get; set; }        // Close price
        public long tick_volume { get; set; }  // Tick volume
        public int spread { get; set; }       // Spread
        public long real_volume { get; set; }  // Trade volume
    }
}
