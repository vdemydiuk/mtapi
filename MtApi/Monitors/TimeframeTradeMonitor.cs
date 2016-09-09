
namespace MtApi.Monitors
{
    public class TimeframeTradeMonitor : TradeMonitor
    {
        #region Fields
        private volatile bool _isStarted = false;
        #endregion

        #region ctor
        public TimeframeTradeMonitor(MtApiClient apiClient) 
            : base(apiClient)
        {
            apiClient.OnLastTimeBar += ApiClient_OnLastTimeBar;
        }
        #endregion

        #region Public Methods
        //
        // Summary:
        //     Gets a value indicating whether the TimeframeTradeMonitor should raise checking orders
        //
        // Returns:
        //     true if PositionMonitor should check orders
        //     otherwise, false.
        public override bool IsStarted
        {
            get
            {
                return _isStarted;
            }
        }
        #endregion

        #region Protected Methods
        protected override void OnMtConnected() {}

        protected override void OnMtDisconnected() {}

        protected override void OnStart()
        {
            _isStarted = true;
        }

        protected override void OnStop()
        {
            _isStarted = false;
        }
        #endregion

        #region Private Methods
        private void ApiClient_OnLastTimeBar(object sender, TimeBarArgs e)
        {
            if (_isStarted)
            {
                Check();
            }
        }
        #endregion
    }
}
