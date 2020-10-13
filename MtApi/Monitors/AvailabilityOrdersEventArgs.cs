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
        /// <summary>
        /// Contains all newly opened orders since the last time the monitor checked the open orders.
        /// </summary>
        public List<MtOrder> Opened { get; private set; }
        /// <summary>
        /// Contains all newly closed orders since the last time the monitor checked the open orders.
        /// </summary>
        public List<MtOrder> Closed { get; private set; }
    }
}
