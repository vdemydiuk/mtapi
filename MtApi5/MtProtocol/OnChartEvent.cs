namespace MtApi5.MtProtocol
{
    internal class OnChartEvent
    {
        public int EventId { get; set; }
        public long Lparam { get; set; }
        public double Dparam { get; set; }
        public string? Sparam { get; set; }
    }
}
