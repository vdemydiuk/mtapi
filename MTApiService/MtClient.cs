using System;
using System.Collections;
using System.ServiceModel;
using System.Collections.Generic;
using log4net;

namespace MTApiService
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class MtClient: IMtApiCallback, IDisposable
    {
        private const string ServiceName = "MtApiService";

        public delegate void MtQuoteHandler(MtQuote quote);

        #region Fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(MtClient));

        private MtApiProxy _proxy;
        private bool _isConnected;
        #endregion

        #region Public Methods
        public void Open(string host, int port)
        {
            Log.DebugFormat("Open: begin. host = {0}, port = {1}", host, port);

            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException(nameof(host), "host is null or empty");

            if (port < 0 || port > 65536)
                throw new ArgumentOutOfRangeException(nameof(port), "port value is invalid");

            var urlService = $"net.tcp://{host}:{port}/{ServiceName}";

            if (_proxy != null)
            {
                Log.Warn("Open: end. _proxy is not null.");
                return;
            }

            var bind = new NetTcpBinding(SecurityMode.None)
            {
                MaxReceivedMessageSize = 2147483647,
                MaxBufferSize = 2147483647,
                MaxBufferPoolSize = 2147483647,
                ReaderQuotas =
                {
                    MaxArrayLength = 2147483647,
                    MaxBytesPerRead = 2147483647,
                    MaxDepth = 2147483647,
                    MaxStringContentLength = 2147483647,
                    MaxNameTableCharCount = 2147483647
                }
            };
            // Commented next statement since it is not required

            _proxy = new MtApiProxy(new InstanceContext(this), bind, new EndpointAddress(urlService));
            _proxy.Faulted += ProxyFaulted;

            Log.Debug("Open: end.");
        }

        public void Open(int port)
        {
            Log.DebugFormat("Open: begin. port = {0}", port);

            if (port < 0 || port > 65536)
                throw new ArgumentOutOfRangeException(nameof(port), "port value is invalid");

            var urlService = $"net.pipe://localhost/{ServiceName}_{port}";

            if (_proxy != null)
            {
                Log.Warn("Open: end. _proxy is not null.");
                return;
            }

            var bind = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
            {
                MaxReceivedMessageSize = 2147483647,
                MaxBufferSize = 2147483647,
                MaxBufferPoolSize = 2147483647,
                ReaderQuotas =
                {
                    MaxArrayLength = 2147483647,
                    MaxBytesPerRead = 2147483647,
                    MaxDepth = 2147483647,
                    MaxStringContentLength = 2147483647,
                    MaxNameTableCharCount = 2147483647
                }
            };
            // Commented next statement since it is not required

            _proxy = new MtApiProxy(new InstanceContext(this), bind, new EndpointAddress(urlService));
            _proxy.Faulted += ProxyFaulted;

            Log.Debug("Open: end.");
        }

        public void Close()
        {
            Log.Debug("Close: begin.");

            if (_proxy != null)
            {
                _proxy.Faulted -= ProxyFaulted;
                _proxy.Dispose();
                _proxy = null;
            }

            _isConnected = false;

            Log.Debug("Close: end.");
        }

        /// <exception cref="CommunicationException">Thrown when connection failed</exception>
        public void Connect()
        {
            Log.Debug("Connect: begin.");

            if (_proxy == null)
            {
                Log.Error("Connect: _proxy is not defined.");
                throw new CommunicationException("Connection failed to service. Proxy is not defined (needs to call Open)");
            }

            if (_isConnected)
            {
                Log.Warn("Connected: end. Client is already connected.");
                return;
            }

            try
            {
                _isConnected = _proxy.Connect();
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Connect: Exception - {0}", ex.Message);

                Close();

                throw new CommunicationException($"Connection failed to service. {ex.Message}");
            }

            if (_isConnected == false)
            {
                Log.Error("Connect: end. Connection failed.");
                throw new CommunicationException("Connection failed");
            }

            Log.Debug("Connect: end.");
        }

        public void Disconnect()
        {
            Log.Debug("Disconnect: begin.");

            try
            {
                _isConnected = false;

                _proxy?.Disconnect();
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Disconnect: Exception - {0}", ex.Message);

                Close();
            }

            Log.Debug("Disconnect: end.");
        }

        /// <exception cref="CommunicationException">Thrown when connection failed</exception>
        public MtResponse SendCommand(int commandType, ArrayList parameters)
        {
            Log.DebugFormat("SendCommand: begin. commandType = {0}, parameters count = {1}", commandType, parameters?.Count);

            MtResponse result;

            if (_proxy == null)
            {
                Log.Error("SendCommand: Proxy is not defined.");
                throw new CommunicationException("Proxy is not defined.");
            }

            if (_isConnected == false)
            {
                Log.Error("SendCommand: Client is not connected.");
                throw new CommunicationException("Client is not connected.");
            }

            try
            {
                result = _proxy.SendCommand(new MtCommand(commandType, parameters));
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("SendCommand: Exception - {0}", ex.Message);

                Close();

                throw new CommunicationException("Service connection failed! " + ex.Message);
            }

            return result;
        }

        /// <exception cref="CommunicationException">Thrown when connection failed</exception>
        public IEnumerable<MtQuote> GetQuotes()
        {
            Log.Debug("GetQuotes: begin.");

            if (_proxy == null)
            {
                Log.Warn("GetQuotes: end. _proxy is not defined.");
                return null;
            }

            if (_isConnected == false)
            {
                Log.Warn("GetQuotes: end. Client is not connected.");
                return null;
            }

            List<MtQuote> result;

            try
            {
                result = _proxy.GetQuotes();
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("GetQuotes: Exception - {0}", ex.Message);

                Close();

                throw new CommunicationException($"Service connection failed! {ex.Message}");
            }

            Log.DebugFormat("GetQuotes: end. quotes count = {0}", result?.Count);

            return result;
        }

        #endregion

        #region IMtApiCallback Members

        public void OnQuoteUpdate(MtQuote quote)
        {
            Log.DebugFormat("OnQuoteUpdate: begin. quote = {0}", quote);

            if (quote == null) return;

            QuoteUpdated?.Invoke(quote);

            Log.Debug("OnQuoteUpdate: end.");
        }

        public void OnQuoteAdded(MtQuote quote)
        {
            Log.DebugFormat("OnQuoteAdded: begin. quote = {0}", quote);

            QuoteAdded?.Invoke(quote);

            Log.Debug("OnQuoteAdded: end.");
        }

        public void OnQuoteRemoved(MtQuote quote)
        {
            Log.DebugFormat("OnQuoteRemoved: begin. quote = {0}", quote);

            QuoteRemoved?.Invoke(quote);

            Log.Debug("OnQuoteRemoved: end.");
        }

        public void OnServerStopped()
        {
            Log.Debug("OnServerStopped: begin.");

            Close();
            ServerDisconnected?.Invoke(this, EventArgs.Empty);

            Log.Debug("OnServerStopped: end.");
        }


        public void OnMtEvent(MtEvent mtEvent)
        {
            Log.DebugFormat("OnMtEvent: begin. event = {0}", mtEvent);

            MtEventReceived?.Invoke(this, new MtEventArgs(mtEvent));

            Log.Debug("OnMtEvent: end.");
        }

        #endregion

        #region Properties
        public bool IsConnected => _proxy.State == CommunicationState.Opened && _isConnected;

        #endregion

        #region Private Methods

        private void ProxyFaulted(object sender, EventArgs e)
        {
            Log.Debug("ProxyFaulted: begin.");

            Close();
            ServerFailed?.Invoke(this, EventArgs.Empty);

            Log.Debug("ProxyFaulted: end.");
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Log.Debug("Dispose: begin.");

            Close();

            Log.Debug("Dispose: end.");
        }

        #endregion

        #region Events
        public event MtQuoteHandler QuoteAdded;
        public event MtQuoteHandler QuoteRemoved;
        public event MtQuoteHandler QuoteUpdated;
        public event EventHandler ServerDisconnected;
        public event EventHandler ServerFailed;
        public event EventHandler<MtEventArgs> MtEventReceived;
        #endregion
    }
}
