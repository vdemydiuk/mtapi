using MtApi.Monitors.Triggers;

namespace MtApi.Monitors
{
    public class TimeframeTradeMonitor : TradeMonitor
    {
        public TimeframeTradeMonitor(MtApiClient apiClient)
            : base(apiClient, new NewBarTrigger(apiClient))
        {
            SyncTrigger = true; //Sync-Trigger set to true, to have the same behavior as before
        }
    }
}