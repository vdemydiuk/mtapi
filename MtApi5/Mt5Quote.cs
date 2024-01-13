namespace MtApi5
{
    public class Mt5Quote
    {
        public string? Instrument { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public int ExpertHandle { get; set; }
        public DateTime Time { get; set; }
        public double Last { get; set; }
        public ulong Volume { get; set; }
        //        public long TimeMsc { get; set; }
        //        public uint Flags { get; set; }
    }
}
