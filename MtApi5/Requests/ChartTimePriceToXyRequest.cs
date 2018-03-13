using System;

namespace MtApi5.Requests
{
    internal class ChartTimePriceToXyRequest : RequestBase
    {
        public override RequestType RequestType => RequestType.ChartTimePriceToXY;

        public long ChartId { get; set; } 
        public int SubWindow { get; set; }
        public DateTime? Time { get; set; }
        public double Price { get; set; }

        public int MtTime => Mt5TimeConverter.ConvertToMtTime(Time);
    }
}