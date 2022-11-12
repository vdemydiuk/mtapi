// ReSharper disable InconsistentNaming

namespace MtApi5
{
    public interface IMqlTradeResult
    {
        double Ask { get; }
        double Bid { get; }
        string Comment { get; }
        ulong Deal { get; }
        ulong Order { get; }
        double Price { get; }
        uint Request_id { get; }
        uint Retcode { get; }
        double Volume { get; }
    }
}