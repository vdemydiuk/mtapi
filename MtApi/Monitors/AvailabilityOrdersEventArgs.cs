namespace MtApi.Monitors
{
    public class AvailabilityOrdersEventArgs(List<MtOrder> opened, List<MtOrder> closed) : EventArgs
    {
        /// <summary>
        /// Contains all newly opened orders since the last time the monitor checked the open orders.
        /// </summary>
        public List<MtOrder> Opened { get; private set; } = opened;
        /// <summary>
        /// Contains all newly closed orders since the last time the monitor checked the open orders.
        /// </summary>
        public List<MtOrder> Closed { get; private set; } = closed;
    }
}
