// ReSharper disable InconsistentNaming
using System;

namespace MtApi5
{
    public class MqlRates
    {
        public MqlRates(DateTime time, double open, double high, double low, double close, long tick_volume, int spread, long real_volume)
        {
            Mt_time = Mt5TimeConverter.ConvertToMtTime(time);
            this.Open = open;
            this.High = high;
            this.Low = low;
            this.Close = close;
            this.Tick_volume = tick_volume;
            this.Spread = spread;
            this.Real_volume = real_volume;
        }

        internal MqlRates(long time, double open, double high, double low, double close, long tick_volume, int spread, long real_volume)
        {
            Mt_time = time;
            this.Open = open;
            this.High = high;
            this.Low = low;
            this.Close = close;
            this.Tick_volume = tick_volume;
            this.Spread = spread;
            this.Real_volume = real_volume;
        }

        public MqlRates()
        {
        }

        public DateTime Time => Mt5TimeConverter.ConvertFromMtTime(Mt_time); // Period start time              
        public long Mt_time { get; set; }            // Period start time (original MT time)
        public double Open { get; set; }         // Open price
        public double High { get; set; }         // The highest price of the period
        public double Low { get; set; }          // The lowest price of the period
        public double Close { get; set; }        // Close price
        public long Tick_volume { get; set; }  // Tick volume
        public int Spread { get; set; }       // Spread
        public long Real_volume { get; set; }  // Trade volume
    }
}
