namespace MtApi.MtProtocol
{
    internal class MtRpcQuote
    {
        public MtTick? Tick { get; set; }
        public string? Instrument { get; set; }
        public int ExpertHandle { get; set; }
    }
}