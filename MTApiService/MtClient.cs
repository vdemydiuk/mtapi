using System;
using System.Diagnostics;
using System.Collections;
using System.ServiceModel;
using System.Collections.Generic;

namespace MTApiService
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class MtClient: IMtApiCallback, IDisposable
    {
        private const string ServiceName = "MtApiService";

        public delegate void MtQuoteHandler(MtQuote quote);

        #region Fields
        private MtApiProxy _proxy;
        private bool _isConnected;
        #endregion

        #region Public Methods
        public void Open(string host, int port)
        {
            Debug.WriteLine("[INFO] MtClient::Open");

            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException(nameof(host), "host is null or empty");

            if (port < 0 || port > 65536)
                throw new ArgumentOutOfRangeException(nameof(port), "port value is invalid");

            var urlService = $"net.tcp://{host}:{port}/{ServiceName}";

            if (_proxy != null)
                return;

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
        }

        public void Open(int port)
        {
            if (port < 0 || port > 65536)
                throw new ArgumentOutOfRangeException(nameof(port), "port value is invalid");

            var urlService = $"net.pipe://localhost/{ServiceName}_{port}";

            if (_proxy != null)
                return;

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
        }

        public void Close()
        {
            Debug.WriteLine("[INFO] MtClient::Close");

            if (_proxy != null)
            {
                _proxy.Faulted -= ProxyFaulted;
                _proxy.Dispose();
                _proxy = null;
            }

            _isConnected = false;
        }

        public void Connect()
        {
            Debug.WriteLine("[INFO] MtClient::Connect");

            try
            {
                if (_proxy != null && _isConnected)
                    return;

                _isConnected = _proxy.Connect();

                if (_isConnected == false)
                    throw new Exception("Connected failed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR] MtClient::Connect: {0}", ex.Message);

                Close();

                throw new CommunicationException("Connection failed to service");
            }
        }

        public void Disconnect()
        {
            Debug.WriteLine("[INFO] MtClient::Disconnect");

            try
            {
                _isConnected = false;

                _proxy?.Disconnect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR] MtClient::Disconnect: {0}", ex.Message);

                Close();
            }
        }

        public MtResponse SendCommand(int commandType, ArrayList commandParameters)
        {
            Debug.WriteLine("[INFO] MtClient::SendCommand: commandType = {0}", commandType);

            MtResponse result = null;

            try
            {
                if (_proxy != null && _isConnected)
                    result = _proxy.SendCommand(new MtCommand(commandType, commandParameters));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR] MtClient::SendCommand: {0}", ex.Message);

                Close();

                throw new CommunicationException("Service connection failed! " + ex.Message);
            }

            return result;
        }

        public IEnumerable<MtQuote> GetQuotes()
        {
            Debug.WriteLine("[INFO] MtClient::GetQuotes");

            IEnumerable<MtQuote> result = null;

            try
            {
                if (_proxy != null && _isConnected)
                    result = _proxy.GetQuotes();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR] MtClient::GetQuotes: {0}", ex.Message);

                Close();

                throw new CommunicationException("Service connection failed");
            }

            return result;;
        }

        #endregion

        #region IMtApiCallback Members

        public void OnQuoteUpdate(MtQuote quote)
        {
            if (quote == null) return;

            QuoteUpdated?.Invoke(quote);

            Debug.WriteLine("[INFO] MtClient::OnQuoteUpdate: " + quote);
        }

        public void OnQuoteAdded(MtQuote quote)
        {
            Debug.WriteLine("[INFO] MtClient::OnQuoteAdded");

            QuoteAdded?.Invoke(quote);
        }

        public void OnQuoteRemoved(MtQuote quote)
        {
            Debug.WriteLine("[INFO] MtClient::OnQuoteRemoved");

            QuoteRemoved?.Invoke(quote);
        }

        public void OnServerStopped()
        {
            Debug.WriteLine("[INFO] MtClient::OnServerStopped");

            Close();

            ServerDisconnected?.Invoke(this, EventArgs.Empty);
        }


        public void OnMtEvent(MtEvent mtEvent)
        {
            MtEventReceived?.Invoke(this, new MtEventArgs(mtEvent));
        }

        #endregion

        #region Properties
        public bool IsConnected => _proxy.State == CommunicationState.Opened && _isConnected;

        #endregion

        #region Private Methods

        private void ProxyFaulted(object sender, EventArgs e)
        {
            Debug.WriteLine("[INFO] MtClient::ProxyFaulted");

            Close();

            ServerFailed?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Debug.WriteLine("[INFO] MtClient::Dispose");

            Close();
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
