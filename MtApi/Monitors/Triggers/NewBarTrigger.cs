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

        public bool IsStarted => _isStarted;
        public event EventHandler Raised;
        public NewBarTrigger(MtApiClient apiClient)
        {
            _apiClient = apiClient;
            _apiClient.OnLastTimeBar += _apiClient_OnLastTimeBar;
        }

        private void _apiClient_OnLastTimeBar(object sender, TimeBarArgs e)
        {
            if (_isStarted)
                Raised?.Invoke(this, EventArgs.Empty);
        }

        public void Start() => SetIsStarted(true);
        public void Stop() => SetIsStarted(false);
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
    }
}
