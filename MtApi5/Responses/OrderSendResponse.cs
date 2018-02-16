namespace MtApi5.Responses
{
    internal class OrderSendResult
    {
        public bool RetVal { get; set; }
        public MqlTradeResult TradeResult { get; set; }
    }

    internal class OrderSendResponse: ResponseBase
    {
        public OrderSendResult Value { get; set; }
    }
}