namespace MtApi5
{
    public class Mt5Quote
    {
        public string Instrument { get; }
        public double Bid { get; }
        public double Ask { get; }

        public Mt5Quote(string instrument, double bid, double ask)
        {
            Instrument = instrument;
            Bid = bid;
            Ask = ask;
        }
    }
}
