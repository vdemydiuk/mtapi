using System;

namespace MtApi
{
    public class MqlRates
    {
        public int MtTime { get; set; }

        public DateTime Time
        {
            get { return MtApiTimeConverter.ConvertFromMtTime(MtTime); }
        }

        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public long TickVolume { get; set; }
        public int Spread { get; set; }
        public long RealVolume { get; set; }
    }
}
