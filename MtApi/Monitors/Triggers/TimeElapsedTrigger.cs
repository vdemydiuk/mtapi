using System;
using System.Timers;

namespace MtApi.Monitors.Triggers
{
    public class TimeElapsedTrigger : IMonitorTrigger
    {
        #region Fields
        readonly Timer _timer;
        #endregion

        #region Properties
        /// <summary>
        /// Interval for raising the trigger
        /// </summary>
        public TimeSpan Interval
        {
            get => TimeSpan.FromMilliseconds(_timer.Interval);
            set => _timer.Interval = value.TotalMilliseconds;
        }

        /// <summary>
        ///  Returns true if the trigger is started, otherwise false
        /// </summary>
        public bool IsStarted => _timer.Enabled;

        /// <summary>
        /// If true, the trigger will raise continuosly after elapsed <see cref="Interval"/>, otherwise the trigger will raise only once after elapsed <see cref="Interval"/>.
        /// </summary>
        public bool AutoReset { get => _timer.AutoReset; set => _timer.AutoReset = value; }
        #endregion

        #region Events
        /// <summary>
        /// Returns true if the trigger is started, otherwise false
        /// </summary>
        public event EventHandler Raised;
        #endregion

        #region ctor
        /// <summary>
        /// Constructor for initializing TimeElapsedTrigger
        /// </summary>
        /// <param name="time">Defines the interval for raising the event.</param>
        /// <param name="autoReset">If true, the trigger will raise continuosly after elapsed <see cref="Interval"/>, otherwise the trigger will raise only once after elapsed <see cref="Interval"/>.</param>
        public TimeElapsedTrigger(TimeSpan time, bool autoReset = true)
        {
            _timer = new Timer(time.TotalMilliseconds);
            _timer.Elapsed += _timer_Elapsed;
            AutoReset = autoReset;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Starts the trigger
        /// </summary>
        public void Start() => _timer.Start();
        /// <summary>
        /// Stops the trigger
        /// </summary>
        public void Stop() => _timer.Stop();
        #endregion

        #region private methods
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Elapsed -= _timer_Elapsed;
            Raised?.Invoke(this, EventArgs.Empty);
            _timer.Elapsed += _timer_Elapsed;
        }
        #endregion
    }
}