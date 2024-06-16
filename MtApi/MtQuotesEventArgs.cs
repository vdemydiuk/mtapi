namespace MtApi
{
    public class MtQuotesEventArgs(IEnumerable<MtQuote> quotes) : EventArgs
    {
        public IEnumerable<MtQuote> Quotes { get; } = quotes;

    }
}
