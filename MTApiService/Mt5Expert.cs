using log4net;

namespace MTApiService
{
    public class Mt5Expert : MtExpert
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Mt5Expert));
        private const int StopExpertInterval = 2000; // 2 sec for testing mode
        private System.Timers.Timer _stopTimer;


        public Mt5Expert(int handle, string symbol, double bid, double ask, IMetaTraderHandler mtHandler, bool isTestMode) : 
            base(handle, symbol, bid, ask, mtHandler)
        {
            IsTestMode = isTestMode;
        }

        public bool IsTestMode { get; }

        public override int GetCommandType()
        {
            Log.Debug("GetCommandType: called.");

            if (IsTestMode)
            {
                ResetTestModeTimer();
            }

            return base.GetCommandType();
        }

        public override void SendEvent(MtEvent mtEvent)
        {
            Log.DebugFormat("SendEvent: begin. event = {0}", mtEvent);

            if (IsTestMode)
            {
                if (_stopTimer == null)
                {
                    _stopTimer = new System.Timers.Timer
                    {
                        Interval = StopExpertInterval,
                        AutoReset = false
                    };
                    _stopTimer.Elapsed += _stopTimer_Elapsed;
                }
            }

            base.SendEvent(mtEvent);

            Log.Debug("SendEvent: end.");
        }

        private void _stopTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Log.Debug("_stopTimer_Elapsed: begin.");

            Log.Warn("Mt5Expert has received new tick during 2 sec in testing mode. The possible cause: user has stopped the tester manually in MetaTrader 5.");
            Deinit();

            _stopTimer.Elapsed -= _stopTimer_Elapsed;
            _stopTimer = null;

            Log.Debug("_stopTimer_Elapsed: end.");
        }

        private void ResetTestModeTimer()
        {
            if (_stopTimer == null)
                return;

            //reset timer
            _stopTimer.Stop();
            _stopTimer.Start();
        }
    }
}