using System;
using MtApi.Monitors.Triggers;

namespace MtApi.Monitors
{
    public class TimerTradeMonitor : TradeMonitor
    {
        #region Fields
        private readonly TimeElapsedTrigger _timeElapsedTrigger;
        #endregion

        #region Properties
        /// <summary>
        /// Interval for raising the trigger
        /// </summary>
        public double Interval
        {
            get => _timeElapsedTrigger.Interval.TotalMilliseconds;
            set => _timeElapsedTrigger.Interval = TimeSpan.FromMilliseconds(value);
        }
        #endregion

        #region ctors
        /// <summary>
        /// Constructor for initializing a new instance with a default <see cref="Interval"/> of 10 seconds.
        /// <para>SyncTrigger is set to true by default</para>
        /// </summary>
        /// <param name="apiClient">The <see cref="MtApiClient"/> which will be used to communicate with MetaTrader.</param>
        public TimerTradeMonitor(MtApiClient apiClient)
            : this(apiClient, new TimeElapsedTrigger(TimeSpan.FromSeconds(10)))
        {
            SyncTrigger = true; //Sync-Trigger set to true, to have the same behavior as before
        }
        /// <summary>
        ///  Constructor for initializing a new instance with a custom instance of <see cref="TimeElapsedTrigger"/>.
        /// <para>SyncTrigger is set to false by default</para>
        /// </summary>
        /// <param name="apiClient">The <see cref="MtApiClient"/> which will be used to communicate with MetaTrader.</param>
        /// <param name="timeElapsedTrigger">The custom instance of <see cref="TimeElapsedTrigger"/> which will be used to trigger this instance of <see cref="TradeMonitor"/>.</param>
        public TimerTradeMonitor(MtApiClient apiClient, TimeElapsedTrigger timeElapsedTrigger)
            : base(apiClient, timeElapsedTrigger)
        {
            _timeElapsedTrigger = timeElapsedTrigger;
        }
        #endregion
    }
}