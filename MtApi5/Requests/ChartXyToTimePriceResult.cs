using System;

namespace MtApi5.Requests
{
    internal class ChartXyToTimePriceResult
    {
        public bool RetVal { get; set; }
        public int SubWindow { get; set; }
        public DateTime? Time => Mt5TimeConverter.ConvertFromMtTime(MtTime);
        public double Price { get; set; }

        public int MtTime { get; set; }
    }
}