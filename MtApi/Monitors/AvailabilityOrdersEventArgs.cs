using System;
using System.Collections.Generic;

namespace MtApi.Monitors
{
    public class AvailabilityOrdersEventArgs : EventArgs
    {
        public AvailabilityOrdersEventArgs(List<MtOrder> opened, List<MtOrder> closed)
        {
            Opened = opened;
            Closed = closed;
        }

        public List<MtOrder> Opened { get; private set; }
        public List<MtOrder> Closed { get; private set; }
    }
}
