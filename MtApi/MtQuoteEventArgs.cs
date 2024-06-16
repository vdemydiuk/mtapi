namespace MtApi
{
    public class MtQuoteEventArgs(MtQuote quote) : EventArgs
    {
        public MtQuote Quote { get; private set; } = quote;
    }
}
