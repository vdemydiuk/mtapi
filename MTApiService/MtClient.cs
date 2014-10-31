using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.ServiceModel;
using System.Collections.Generic;

namespace MTApiService
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class MtClient: IMtApiCallback, IDisposable
    {
        private static string SERVICE_NAME = "MtApiService";

//        public delegate void MtInstrumentsChangedHandler(string addedInstrument, string removedInstrument);
        public delegate void MtQuoteHandler(MtQuote quote);

        #region Public Methods
        public void Open(string host, int port)
        {
            Debug.WriteLine("[INFO] MtClient::Open");

            if (string.IsNullOrEmpty(host) == true)
                throw new ArgumentNullException("host", "host is null or epmty");

            if (port < 0 || port > 65536)
                throw new ArgumentOutOfRangeException("port", "port value is invalid");

            string urlService = string.Format("net.tcp://{0}:{1}/{2}", host, port, SERVICE_NAME);

            lock (mClientLocker)
            {
                if (mProxy != null)
                    return;

                var bind = new NetTcpBinding();
                bind.MaxReceivedMessageSize = 2147483647;
                bind.MaxBufferSize = 2147483647;
                // Commented next statement since it is not required
                bind.MaxBufferPoolSize = 2147483647;
                bind.ReaderQuotas.MaxArrayLength = 2147483647;
                bind.ReaderQuotas.MaxBytesPerRead = 2147483647;
                bind.ReaderQuotas.MaxDepth = 2147483647;
                bind.ReaderQuotas.MaxStringContentLength = 2147483647;
                bind.ReaderQuotas.MaxNameTableCharCount = 2147483647;

                mProxy = new MtApiProxy(new InstanceContext(this), bind, new EndpointAddress(urlService));
                mProxy.Faulted += mProxy_Faulted;
            }
        }

        public void Open(int port)
        {
            if (port < 0 || port > 65536)
                throw new ArgumentOutOfRangeException("port", "port value is invalid");

            string urlService = "net.pipe://localhost/" + SERVICE_NAME + "_" + port.ToString();

            lock (mClientLocker)
            {
                if (mProxy != null)
                    return;

                var bind = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                bind.MaxReceivedMessageSize = 2147483647;
                bind.MaxBufferSize = 2147483647;
                // Commented next statement since it is not required
                bind.MaxBufferPoolSize = 2147483647;
                bind.ReaderQuotas.MaxArrayLength = 2147483647;
                bind.ReaderQuotas.MaxBytesPerRead = 2147483647;
                bind.ReaderQuotas.MaxDepth = 2147483647;
                bind.ReaderQuotas.MaxStringContentLength = 2147483647;
                bind.ReaderQuotas.MaxNameTableCharCount = 2147483647;

                mProxy = new MtApiProxy(new InstanceContext(this), bind, new EndpointAddress(urlService));
                mProxy.Faulted += mProxy_Faulted;
            }
        }

        public void Close()
        {
            Debug.WriteLine("[INFO] MtClient::Close");

            lock (mClientLocker)
            {
                if (mProxy != null)
                {
                    mProxy.Faulted -= mProxy_Faulted;
                    mProxy.Dispose();
                    mProxy = null;
                }

                mIsConnected = false;
            }
        }

        public void Connect()
        {
            Debug.WriteLine("[INFO] MtClient::Connect");

            try
            {
                lock (mClientLocker)
                {
                    if (mProxy != null && mIsConnected == true)
                        return;

                    mIsConnected = mProxy.Connect();

                    if (mIsConnected == false)
                        throw new Exception("Connected failed");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR] MtClient::Connect: {0}", ex.Message);

                Close();

                throw new CommunicationException(string.Format("Connection failed to service"));
            }
        }

        public void Disconnect()
        {
            Debug.WriteLine("[INFO] MtClient::Disconnect");

            try
            {
                lock (mClientLocker)
                {
                    mIsConnected = false;

                    if (mProxy != null)
                        mProxy.Disconnect();
                }
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
                lock (mClientLocker)
                {
                    if (mProxy != null && mIsConnected == true)
                        result = mProxy.SendCommand(new MtCommand(commandType, commandParameters));
                }
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
                lock (mClientLocker)
                {
                    if (mProxy != null && mIsConnected == true)
                        result = mProxy.GetQuotes();
                }
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
            if (quote != null)
            {                
                if (QuoteUpdated != null)
                {
                    QuoteUpdated(quote);
                }

                Debug.WriteLine("[INFO] MtClient::OnQuoteUpdate: " + quote);
            }
        }

        public void OnQuoteAdded(MtQuote quote)
        {
            Debug.WriteLine("[INFO] MtClient::OnQuoteAdded");

            if (QuoteAdded != null)
            {
                QuoteAdded(quote);
            }
        }

        public void OnQuoteRemoved(MtQuote quote)
        {
            Debug.WriteLine("[INFO] MtClient::OnQuoteRemoved");

            if (QuoteRemoved != null)
            {
                QuoteRemoved(quote);
            }
        }

        public void OnServerStopped()
        {
            Debug.WriteLine("[INFO] MtClient::OnServerStopped");

            Close();

            if (ServerDisconnected != null)
            {
                ServerDisconnected(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Properties
        public bool IsConnected 
        {
            get
            {
                lock (mClientLocker)
                {
                    return mProxy.State == CommunicationState.Opened && mIsConnected == true;
                }
            }
        }

        #endregion

        #region Private Methods

        void mProxy_Faulted(object sender, EventArgs e)
        {
            Debug.WriteLine("[INFO] MtClient::mProxy_Faulted");

            Close();

            if (ServerFailed != null)
            {
                ServerFailed(this, EventArgs.Empty);
            }
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
        #endregion

        #region Fields
        private readonly object mClientLocker = new object();
        private MtApiProxy mProxy = null;
        private bool mIsConnected = false;
        #endregion
    }
}
