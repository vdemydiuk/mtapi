using System;

namespace MtApi
{
    public class MtTimeBar
    {
        public string Symbol { get; set; }
        public int MtOpenTime { get; set; }
        public int MtCloseTime { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public DateTime OpenTime => MtApiTimeConverter.ConvertFromMtTime(MtOpenTime);
        public DateTime CloseTime => MtApiTimeConverter.ConvertFromMtTime(MtCloseTime);
    }
}
