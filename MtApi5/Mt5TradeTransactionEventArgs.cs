using System;

namespace MtApi5
{
    public class Mt5TradeTransactionEventArgs : EventArgs
    {
        public int ExpertHandle { get; set; }
        public MqlTradeTransaction Trans { get; set; }  // trade transaction structure 
        public MqlTradeRequest Request { get; set; }    // request structure
        public MqlTradeResult Result { get; set; }      // result structure 
    }
}