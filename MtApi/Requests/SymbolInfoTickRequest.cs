namespace MtApi.Requests
{
    internal class SymbolInfoTickRequest: RequestBase
    {
        public override RequestType RequestType => RequestType.SymbolInfoTick;

        public string Symbol { get; set; }
    }
}