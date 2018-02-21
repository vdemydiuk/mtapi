namespace MtApi5.Requests
{
    internal class MarketBookGetRequest: RequestBase
    {
        public override RequestType RequestType => RequestType.MarketBookGet;

        public string Symbol { get; set; }
    }
}