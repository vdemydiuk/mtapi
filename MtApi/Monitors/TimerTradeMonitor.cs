using System.Timers;

namespace MtApi.Monitors
{
    public class TimerTradeMonitor : TradeMonitor
    {
        #region Fields
        private readonly Timer _timer = new Timer();
        #endregion

        #region ctor
        public TimerTradeMonitor(MtApiClient apiClient) 
            : base(apiClient)
        {
            _timer.Interval = 10000; //default interval 10 sec
            _timer.Elapsed += _timer_Elapsed;

        }
        #endregion

        #region Public Methods
        //
        // Summary:
        //     Gets or sets the interval, expressed in milliseconds, at which to check orders
        //
        // Returns:
        //     The time, in milliseconds, between checking events. The value
        //     must be greater than zero, and less than or equal to System.Int32.MaxValue. 
        //     The default is 10000 milliseconds.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     The interval is less than or equal to zero.-or-The interval is greater than System.Int32.MaxValue,
        //     and the PositionMonitor is currently started.
        public double Interval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        //
        // Summary:
        //     Gets a value indicating whether the PositionMonitor should raise checking orders
        //
        // Returns:
        //     true if TimerTradeMonitor should check orders
        //     otherwise, false.
        public override bool IsStarted
        {
            get { return _timer.Enabled; }
        }
        #endregion

        #region Protected Methods
        protected override void OnStart()
        {
            if (IsMtConnected)
            {
                _timer.Start();
            }
        }

        protected override void OnStop()
        {
            _timer.Stop();
        }

        protected override void OnMtConnected()
        {
            _timer.Start();
        }

        protected override void OnMtDisconnected()
        {
            _timer.Stop();
        }
        #endregion

        #region Private Methods
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Elapsed -= _timer_Elapsed; //unregister from events to prevent rise condition during work with orders

            Check();

            _timer.Elapsed += _timer_Elapsed; //register again
        }
        #endregion
    }
}
