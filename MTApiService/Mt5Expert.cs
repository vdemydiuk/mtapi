using log4net;

namespace MTApiService
{
    public class Mt5Expert : MtExpert
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MtExpert));
        private const int StopExpertInterval = 2000; // 2 sec for testing mode
        private readonly System.Timers.Timer _stopTimer = new System.Timers.Timer();


        public Mt5Expert(int handle, string symbol, double bid, double ask, IMetaTraderHandler mtHandler, bool isTestMode) : 
            base(handle, symbol, bid, ask, mtHandler)
        {
            IsTestMode = isTestMode;
            _stopTimer.Interval = StopExpertInterval;
            _stopTimer.Elapsed += _stopTimer_Elapsed;
        }

        public bool IsTestMode { get; }

        public override void UpdateQuote(MtQuote quote)
        {
            Log.Debug("UpdateQuote: begin.");

            base.UpdateQuote(quote);

            if (IsTestMode)
            {
                //reset timer
                _stopTimer.Stop();
                _stopTimer.Start();
            }

            Log.Debug("UpdateQuote: end.");
        }

        private void _stopTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Log.Debug("_stopTimer_Elapsed: begin.");

            Log.Warn("Mt5Expert has received new tick during 2 sec in testing mode. The possible cause: user has stopped the tester manually in MetaTrader 5.");
            Deinit();

            Log.Debug("_stopTimer_Elapsed: end.");
        }
    }
}