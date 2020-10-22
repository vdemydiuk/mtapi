using System;

namespace MtApi.Monitors.Triggers
{
    /// <summary>
    /// Raises the <see cref="Raised"/> event if a bar is closed and a new one started.
    /// </summary>
    public class NewBarTrigger : IMonitorTrigger
    {
        #region Fields
        private volatile bool _isStarted;
        private readonly MtApiClient _apiClient;
        #endregion

        #region Properties
        /// <summary>
        /// Returns true if the trigger is started, otherwise false
        /// </summary>
        public bool IsStarted => _isStarted;
        #endregion

        #region Events
        /// <summary>
        /// Event will be called if the trigger raised.
        /// </summary>
        public event EventHandler Raised;
        #endregion

        #region ctor
        public NewBarTrigger(MtApiClient apiClient)
        {
            _apiClient = apiClient;
            _apiClient.OnLastTimeBar += _apiClient_OnLastTimeBar;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Starts the trigger
        /// </summary>
        public void Start() => SetIsStarted(true);
        /// <summary>
        /// Stops the trigger
        /// </summary>
        public void Stop() => SetIsStarted(false);
        #endregion

        #region Private methods
        private void _apiClient_OnLastTimeBar(object sender, TimeBarArgs e)
        {
            if (_isStarted)
                Raised?.Invoke(this, EventArgs.Empty);
        }

        private void SetIsStarted(bool value)
        {
            if (value != _isStarted)
            {
                _isStarted = value;
                if (value)
                    _apiClient.OnLastTimeBar += _apiClient_OnLastTimeBar;
                else
                    _apiClient.OnLastTimeBar -= _apiClient_OnLastTimeBar;
            }
        }
        #endregion
    }
}
