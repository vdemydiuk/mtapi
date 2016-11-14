using System;

namespace MtApi
{
    public class MqlTick
    {
        public int MtTime { get; set; }
        public DateTime Time => MtApiTimeConverter.ConvertFromMtTime(MtTime);
        public double Bid { get; set; }
        public double Ask { get; set; }
        public double Last { get; set; }
        public ulong Volume { get; set; }
    }
}