namespace MtApi
{
    public class MtQuote
    {
        public string Instrument { get; private set; }
        public double Bid { get; private set; }
        public double Ask { get; private set; }
        public int ExpertHandle { get; private set; }

        public MtQuote(string instrument, double bid, double ask)
        {
            Instrument = instrument;
            Bid = bid;
            Ask = ask;
        }

        internal MtQuote(MTApiService.MtQuote quote)
        {
            Instrument = quote.Instrument;
            Bid = quote.Bid;
            Ask = quote.Ask;
            ExpertHandle = quote.ExpertHandle;
        }
    }
}
