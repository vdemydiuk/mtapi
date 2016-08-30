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

        public DateTime OpenTime
        {
            get { return MtApiTimeConverter.ConvertFromMtTime(MtOpenTime); }
        }

        public DateTime CloseTime
        {
            get { return MtApiTimeConverter.ConvertFromMtTime(MtCloseTime); }
        }

    }
}
