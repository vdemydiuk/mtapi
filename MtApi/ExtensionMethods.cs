namespace MtApi
{
    static class ExtensionMethods
    {
        public static MtQuote Convert(this MTApiService.MtQuote quote)
        {
            return (quote != null) ? new MtQuote(quote.Instrument, quote.Bid, quote.Ask) : null;
        }
    }
}
