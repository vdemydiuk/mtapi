// ReSharper disable InconsistentNaming
using System;

namespace MtApi5
{
    public class MqlTick
    {
        public MqlTick(DateTime time, double bid, double ask, double last, ulong volume)
        {
            MtTime = Mt5TimeConverter.ConvertToMtTime(time);
            this.Bid = bid;
            this.Ask = ask;
            this.Last = last;
            this.Volume = volume;
        }

        public MqlTick()
        {
        }

        public long MtTime { get; set; }          // Time of the last prices update

        public double Bid { get; set; }           // Current Bid price
        public double Ask { get; set; }           // Current Ask price
        public double Last { get; set; }          // Price of the last deal (Last)
        public ulong Volume { get; set; }         // Volume for the current Last price
        public double Volume_real { get; set; }   // Volume for the current Last price with greater accuracy 

        public DateTime Time => Mt5TimeConverter.ConvertFromMtTime(MtTime);
    }
}
