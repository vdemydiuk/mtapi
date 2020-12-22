using System;

namespace MtApi
{
    public class MtLockTicksEventArgs : EventArgs
    {
        internal MtLockTicksEventArgs(int expertHandle, string symbol)
        {
            ExpertHandle = expertHandle;
            Symbol = symbol;
        }

        public int ExpertHandle { get; }
        public string Symbol { get; }
    }
}