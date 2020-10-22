using MtApi.Monitors.Triggers;

namespace MtApi.Monitors
{
    public class TimeframeTradeMonitor : TradeMonitor
    {
        /// <summary>
        /// Constructor for initializing a new instance with a trigger instance of <see cref="NewBarTrigger"/>.
        /// <para>SyncTrigger is set to true by default</para>
        /// </summary>
        /// <param name="apiClient">The <see cref="MtApiClient"/> which will be used to communicate with MetaTrader.</param>
        public TimeframeTradeMonitor(MtApiClient apiClient)
            : base(apiClient, new NewBarTrigger(apiClient))
        {
            SyncTrigger = true; //Sync-Trigger set to true, to have the same behavior as before
        }
    }
}