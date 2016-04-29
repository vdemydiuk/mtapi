using System;

namespace MtApi
{
    public class MtOrder
    {
        public int Ticket { get; set; }
        public string Symbol { get; set; }
        public TradeOperation Operation { get; set; }
        public double OpenPrice { get; set; }
        public double ClosePrice { get; set; }
        public double Lots { get; set; }
        public int MtOpenTime { get; set; }
        public int MtCloseTime { get; set; }
        public double Profit { get; set; }
        public string Comment { get; set; }
        public double Commission { get; set; }
        public int MagicNumber { get; set; }
        public double Swap { get; set; }

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