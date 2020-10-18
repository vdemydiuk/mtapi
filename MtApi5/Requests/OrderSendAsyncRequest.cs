namespace MtApi5.Requests
{
    internal class OrderSendAsyncRequest : RequestBase
    {
        public override RequestType RequestType => RequestType.OrderSendAsync;

        public MqlTradeRequest TradeRequest { get; set; }
    }
}