namespace MtApi.Requests
{
    internal class OrderSendRequest: RequestBase
    {
        public string Symbol { get; set; }
        public int Cmd { get; set; }
        public double Volume { get; set; }

        public double? Price { get; set; }
        public int? Slippage { get; set; }
        public double? Stoploss { get; set; }
        public double? Takeprofit { get; set; }
        public string Comment { get; set; }
        public int? Magic { get; set; }
        public int? Expiration { get; set; }
        public int? ArrowColor { get; set; }

        public override RequestType RequestType => RequestType.OrderSend;
    }
}