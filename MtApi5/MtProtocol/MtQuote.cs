namespace MtApi5.MtProtocol
{
    internal class MtQuote
    {
        public MtTick? Tick { get; set; }
        public string? Instrument { get; set; }
        public int ExpertHandle { get; set; }
    }
}