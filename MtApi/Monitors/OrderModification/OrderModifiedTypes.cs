using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtApi.Monitors
{
    [Flags]
    public enum OrderModifiedTypes
    {
        None = 0x0,
        TakeProfit = 1 << 0,
        StopLoss = 1 << 1,
        Operation = 1 << 2,
        All = 7
    }
}
