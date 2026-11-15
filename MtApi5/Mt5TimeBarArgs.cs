using System;

namespace MtApi5
{
    public class Mt5TimeBarArgs: EventArgs
    {
        internal Mt5TimeBarArgs(int expertHandle, string symbol, ENUM_TIMEFRAMES timeframe, MqlRates rates)
        {
            ExpertHandle = expertHandle;
            Rates = rates;
            Symbol = symbol;
            Timeframe = timeframe;
        }

        public int ExpertHandle { get; }
        public string Symbol { get; }
        public ENUM_TIMEFRAMES Timeframe { get; }
        public MqlRates Rates { get; }
    }
}
