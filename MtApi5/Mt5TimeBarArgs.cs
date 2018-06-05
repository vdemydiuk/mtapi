using System;

namespace MtApi5
{
    public class Mt5TimeBarArgs: EventArgs
    {
        internal Mt5TimeBarArgs(int expertHandle, string symbol, MqlRates rates)
        {
            ExpertHandle = expertHandle;
            Rates = rates;
            Symbol = symbol;
        }

        public int ExpertHandle { get; }
        public string Symbol { get; }
        public MqlRates Rates { get; }
    }
}
