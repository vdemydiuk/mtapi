using System.Diagnostics.CodeAnalysis;

namespace MtApi5.Requests
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal enum RequestType
    {
        Unknown         = 0,
        CopyTicks       = 1,
        iCustom         = 2,
        OrderSend       = 3
    }
}