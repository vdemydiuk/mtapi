namespace MtApi5.Events
{
    internal class OnTradeTransactionEvent
    {
        public MqlTradeTransaction Trans { get; set; }
        public MqlTradeRequest Request { get; set; }
        public MqlTradeResult Result { get; set; }
    }
}