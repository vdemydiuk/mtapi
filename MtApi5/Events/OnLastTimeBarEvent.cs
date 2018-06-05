namespace MtApi5.Events
{
    public class OnLastTimeBarEvent
    {
        public MqlRates Rates { get; set; }
        public string Instrument { get; set; }
        public int ExpertHandle { get; set; }
    }
}