using System;
using MtApi.Monitors.Triggers;

namespace MtApi.Monitors
{
    public class TimerTradeMonitor : TradeMonitor
    {
        private readonly TimeElapsedTrigger _timeElapsedTrigger;
        public double Interval
        {
            get => _timeElapsedTrigger.Interval.TotalMilliseconds;
            set => _timeElapsedTrigger.Interval = TimeSpan.FromMilliseconds(value);
        }

        public TimerTradeMonitor(MtApiClient apiClient)
            : this(apiClient, new TimeElapsedTrigger(TimeSpan.FromSeconds(10)))
        {
            SyncTrigger = true; //Sync-Trigger set to true, to have the same behavior as before
        }
        public TimerTradeMonitor(MtApiClient apiClient, TimeElapsedTrigger timeElapsedTrigger)
            : base(apiClient, timeElapsedTrigger)
        {
            _timeElapsedTrigger = timeElapsedTrigger;
        }
    }
}