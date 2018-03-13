using System;

namespace MtApi5.Requests
{
    internal class ChartXyToTimePriceRequest : RequestBase
    {
        public override RequestType RequestType => RequestType.ChartXYToTimePrice;

        public long ChartId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}