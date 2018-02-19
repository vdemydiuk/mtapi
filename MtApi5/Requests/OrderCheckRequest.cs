namespace MtApi5.Requests
{
    internal class OrderCheckRequest: RequestBase
    {
        public override RequestType RequestType => RequestType.OrderCheck;

        public MqlTradeRequest TradeRequest { get; set; }

    }
}