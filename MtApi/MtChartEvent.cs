namespace MtApi
{
    internal class MtChartEvent
    {
        public long ChartId { get; set; }
        public int EventId { get; set; }
        public long Lparam { get; set; }
        public double Dparam { get; set; }
        public string Sparam { get; set; }
    }
}