using MTApiService;

namespace MtApi5
{
    public class Mt5Quote
    {
        public string Instrument { get; }
        public double Bid { get; }
        public double Ask { get; }
        public int ExpertHandle { get; set; }

        private Mt5Quote(string instrument, double bid, double ask)
        {
            Instrument = instrument;
            Bid = bid;
            Ask = ask;
        }

        internal Mt5Quote(MtQuote quote)
            :this(quote.Instrument, quote.Bid, quote.Ask)
        {
            ExpertHandle = quote.ExpertHandle;
        }
    }
}
