using System;

namespace MtApi5
{
    public interface IMt5Quote
    {
        double Ask { get; }
        double Bid { get; }
        int ExpertHandle { get; set; }
        string Instrument { get; }
        double Last { get; set; }
        DateTime Time { get; set; }
        ulong Volume { get; set; }
    }
}