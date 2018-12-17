using System;

namespace MtApi5
{
    public class Mt5LockTicksEventArgs : EventArgs
    {
        internal Mt5LockTicksEventArgs(string symbol)
        {
            Symbol = symbol;
        }

        public string Symbol { get; }
    }
}