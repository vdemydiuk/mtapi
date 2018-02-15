namespace MtApi5.Requests
{
    internal class OrderSendRequest: RequestBase
    {
        public override RequestType RequestType => RequestType.OrderSend;

        public MqlTradeRequest TradeRequest { get; set; }
    }
}
