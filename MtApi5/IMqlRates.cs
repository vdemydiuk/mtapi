// ReSharper disable InconsistentNaming
using System;

namespace MtApi5
{
    public interface IMqlRates
    {
        double close { get; set; }
        double high { get; set; }
        double low { get; set; }
        long mt_time { get; set; }
        double open { get; set; }
        long real_volume { get; set; }
        int spread { get; set; }
        long tick_volume { get; set; }
        DateTime time { get; }
    }
}