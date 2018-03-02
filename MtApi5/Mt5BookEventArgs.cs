using System;

namespace MtApi5
{
    public class Mt5BookEventArgs : EventArgs
    {
        public int ExpertHandle { get; set; }
        public string Symbol { get; set; }      //Symbol of OnBookEvent event.
    }
}