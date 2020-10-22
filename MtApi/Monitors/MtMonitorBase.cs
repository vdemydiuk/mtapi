using System;
using MtApi.Monitors.Triggers;

namespace MtApi.Monitors
{
    public abstract class MtMonitorBase
    {
        #region Fields
        private volatile bool _isStarted = false;
        private bool _syncTrigger;
        #endregion

        #region Properties
        /// <summary>
        /// ApiClient
        /// </summary>
        protected MtApiClient ApiClient { get; }
        /// <summary>
        /// Returns true if the <see cref="ApiClient"/> is connected.
        /// </summary>
        public bool IsMtConnected => ApiClient.ConnectionState == MtConnectionState.Connected;
        /// <summary>
        /// Returns the trigger which will be used to raise the monitoring call.
        /// </summary>
        public IMonitorTrigger MonitorTrigger { get; }
        /// <summary>
        /// Returns true if the Monitor is started.
        /// </summary>
        public bool IsStarted { get => _isStarted; }
        /// <summary>
        /// If true, the <see cref="MonitorTrigger"/> will be stopped or started automatically when <see cref="Start"/> or <see cref="Stop"/> will be called.
        /// <para>CAUTION: If you use the MonitorTrigger for different Monitors, this will stop all monitors if you call stop and <see cref="SyncTrigger"/> is <c>true</c>.</para>
        /// </summary>
        public bool SyncTrigger { get => _syncTrigger; set => _syncTrigger = value; }
        #endregion

        #region ctor
        /// <summary>
        /// Default constructor for Monitors
        /// </summary>
        /// <param name="apiClient">The apiClient which will be used to work with.</param>
        /// <param name="monitorTrigger">The trigger which lead this Monitor to do his work.</param>
        /// <param name="syncTrigger">See property <see cref="SyncTrigger"/>.</param>
        public MtMonitorBase(MtApiClient apiClient, IMonitorTrigger monitorTrigger, bool syncTrigger = false)
        {
            ApiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            MonitorTrigger = monitorTrigger ?? throw new ArgumentNullException(nameof(monitorTrigger));
            SyncTrigger = syncTrigger;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Let the monitor listen to the <see cref="MonitorTrigger"/>.
        /// </summary>
        public virtual void Start()
        {
            if (!_isStarted)
            {
                ApiClient.ConnectionStateChanged += ApiClientConnectionStateChanged;
                MonitorTrigger.Raised += MonitorTriggerRaised;
                _isStarted = true;
                OnStart();
                if (SyncTrigger)
                    MonitorTrigger.Start();
            }
        }
        /// <summary>
        /// Let the monitor stop listening to the <see cref="MonitorTrigger"/>.
        /// </summary>
        public virtual void Stop()
        {
            if (_isStarted)
            {
                MonitorTrigger.Raised -= MonitorTriggerRaised;
                ApiClient.ConnectionStateChanged -= ApiClientConnectionStateChanged;
                _isStarted = false;
                OnStop();
                if (SyncTrigger)
                    MonitorTrigger.Stop();
            }
        }
        private void MonitorTriggerRaised(object sender, EventArgs e) => OnTriggerRaised();
        private void ApiClientConnectionStateChanged(object sender, MtConnectionEventArgs e)
        {
            if (e.Status == MtConnectionState.Connected)
                OnMtConnected();
            else if (e.Status == MtConnectionState.Failed || e.Status == MtConnectionState.Disconnected)
                OnMtDisconnected();
        }
        /// <summary>
        /// Will be called when <see cref="Start"/> will be called.
        /// </summary>
        protected virtual void OnStart() { }
        /// <summary>
        /// Will be called when <see cref="Stop"/> will be called.
        /// </summary>
        protected virtual void OnStop() { }
        /// <summary>
        /// Will be called when the <see cref="ApiClient"/> is successfully connected.
        /// </summary>
        protected virtual void OnMtConnected() { }
        /// <summary>
        /// Will be called when <see cref="ApiClient"/> is disconnected.
        /// </summary>
        protected virtual void OnMtDisconnected() { }
        /// <summary>
        /// Will be called when the <see cref="MonitorTrigger"/> raised.
        /// </summary>
        protected abstract void OnTriggerRaised();
        #endregion
    }
}