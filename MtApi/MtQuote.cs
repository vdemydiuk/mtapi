namespace MtApi
{
    public class MtQuote
    {
        public string Instrument { get; set; } = string.Empty;
        public double Bid { get; set; }
        public double Ask { get; set; }
        public int ExpertHandle { get; set; }

        public MtQuote(string instrument, double bid, double ask)
        {
            Instrument = instrument;
            Bid = bid;
            Ask = ask;
        }

        public MtQuote()
        {

        }
    }
}
