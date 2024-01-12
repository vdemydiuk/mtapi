namespace MtApi5
{
    public class Mt5QuotesEventArgs(IEnumerable<Mt5Quote> quotes) : EventArgs
    {
        public IEnumerable<Mt5Quote> Quotes { get; } = quotes;

    }
}
