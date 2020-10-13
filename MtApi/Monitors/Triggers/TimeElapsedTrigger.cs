using System;
using System.Timers;

namespace MtApi.Monitors.Triggers
{
    public class TimeElapsedTrigger : IMonitorTrigger
    {
        readonly Timer _timer;

        public event EventHandler Raised;
        /// <summary>
        /// Interval for raising the trigger
        /// </summary>
        public TimeSpan Interval
        {
            get => TimeSpan.FromMilliseconds(_timer.Interval);
            set => _timer.Interval = value.TotalMilliseconds;
        }
        public bool IsStarted => _timer.Enabled;

        public bool AutoReset { get => _timer.AutoReset; set => _timer.AutoReset = value; }

        public TimeElapsedTrigger(TimeSpan time, bool autoReset = true)
        {
            _timer = new Timer(time.TotalMilliseconds);
            _timer.Elapsed += _timer_Elapsed;
            AutoReset = autoReset;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Elapsed -= _timer_Elapsed;
            Raised?.Invoke(this, EventArgs.Empty);
            _timer.Elapsed += _timer_Elapsed;
        }
        public void Stop() => _timer.Stop();

        public void Start() => _timer.Start();
    }
}
