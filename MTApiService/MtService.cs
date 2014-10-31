using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Diagnostics;
using System.Threading;

namespace MTApiService
{
    [ServiceContract(CallbackContract = typeof(IMtApiCallback), SessionMode = SessionMode.Required)]
    public interface IMtApi
    {
        [OperationContract]
        bool Connect();

        [OperationContract(IsOneWay = true)]
        void Disconnect();

        [OperationContract]
        MtResponse SendCommand(MtCommand command);

        [OperationContract]
        IEnumerable<MtQuote> GetQuotes();
    }
   
    public interface IMtApiCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnQuoteUpdate(MtQuote quote);

        [OperationContract(IsOneWay = true)]
        void OnServerStopped();

        [OperationContract(IsOneWay = true)]
        void OnQuoteAdded(MtQuote quote);

        [OperationContract(IsOneWay = true)]
        void OnQuoteRemoved(MtQuote quote);
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple,
                    AutomaticSessionShutdown = true,
                    IncludeExceptionDetailInFaults = true,
                    InstanceContextMode = InstanceContextMode.Single)]
    public sealed class MtService : IMtApi
    {
        public MtService(IMtApiServer serverCallback)
        {
            if (serverCallback == null)
                throw new ArgumentNullException("serverCallback");

            mServer = serverCallback;
        }

        #region IMtApi
        public bool Connect()
        {
            bool connected = false;

            IMtApiCallback callback = OperationContext.Current.GetCallbackChannel<IMtApiCallback>();

            if (callback != null)
            {
                try
                {
                    mClientsLocker.AcquireWriterLock(10000);

                    try
                    {
                        if (mClientCallbacks.Contains(callback) == false)
                            mClientCallbacks.Add(callback);

                        connected = true;
                    }
                    finally
                    {
                        mClientsLocker.ReleaseWriterLock();
                    }
                }
                catch (ApplicationException)
                {
                }
            }

            return connected;
        }

        public void Disconnect()
        {
            IMtApiCallback callback = OperationContext.Current.GetCallbackChannel<IMtApiCallback>();

            if (callback != null)
            {
                try
                {
                    mClientsLocker.AcquireWriterLock(10000);

                    try
                    {
                        mClientCallbacks.Remove(callback);
                    }
                    finally
                    {
                        mClientsLocker.ReleaseWriterLock();
                    }
                }
                catch (ApplicationException)
                {
                }
            }
        }

        public MtResponse SendCommand(MtCommand command)
        {
            return mServer.SendCommand(command);
        }

        public IEnumerable<MtQuote> GetQuotes()
        {
            return mServer.GetQuotes();
        }
        #endregion

        #region Public Methods
        public void OnStopServer()
        {
            try
            {
                mClientsLocker.AcquireReaderLock(2000);

                try
                {
                    foreach (var callback in mClientCallbacks)
                    {
                        try
                        {
                            callback.OnServerStopped();
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                finally
                {
                    mClientsLocker.ReleaseReaderLock();
                }
            }
            catch (ApplicationException)
            {
            }
        }

        public void QuoteUpdate(MtQuote quote)
        {
            try
            {
                mClientsLocker.AcquireReaderLock(200);

                List<IMtApiCallback> crashedClientsCallback = null;

                try
                {
                    foreach (var callback in mClientCallbacks)
                    {
                        try
                        {
                            callback.OnQuoteUpdate(quote);
                        }
                        catch (Exception)
                        {
                            if (crashedClientsCallback == null)
                                crashedClientsCallback = new List<IMtApiCallback>();

                            crashedClientsCallback.Add(callback);
                        }
                    }
                }
                finally
                {
                    mClientsLocker.ReleaseReaderLock();
                }

                removeCrashedClientCallbacks(crashedClientsCallback);
            }
            catch (ApplicationException)
            {
            }
        }

        public void OnQuoteAdded(MtQuote quote)
        {
            try
            {
                mClientsLocker.AcquireReaderLock(2000);

                List<IMtApiCallback> crashedClientsCallback = null;

                try
                {
                    foreach (var callback in mClientCallbacks)
                    {
                        try
                        {
                            callback.OnQuoteAdded(quote);
                        }
                        catch (Exception)
                        {
                            if (crashedClientsCallback == null)
                                crashedClientsCallback = new List<IMtApiCallback>();

                            crashedClientsCallback.Add(callback);
                        }
                    }
                }
                finally
                {
                    mClientsLocker.ReleaseReaderLock();
                }

                removeCrashedClientCallbacks(crashedClientsCallback);
            }
            catch (ApplicationException)
            {
            }
        }

        public void OnQuoteRemoved(MtQuote quote)
        {
            try
            {
                mClientsLocker.AcquireReaderLock(2000);

                List<IMtApiCallback> crashedClientsCallback = null;

                try
                {
                    foreach (var callback in mClientCallbacks)
                    {
                        try
                        {
                            callback.OnQuoteRemoved(quote);
                        }
                        catch (Exception)
                        {
                            if (crashedClientsCallback == null)
                                crashedClientsCallback = new List<IMtApiCallback>();

                            crashedClientsCallback.Add(callback);
                        }
                    }
                }
                finally
                {
                    mClientsLocker.ReleaseReaderLock();
                }

                removeCrashedClientCallbacks(crashedClientsCallback);
            }
            catch (ApplicationException)
            {
            }
        }
        #endregion

        #region Private Methods

        private void removeCrashedClientCallbacks(List<IMtApiCallback> crashedClientCallbacks)
        {
            if (crashedClientCallbacks != null)
            {
                try
                {
                    mClientsLocker.AcquireWriterLock(200);

                    try
                    {
                        foreach (var crashedCallback in crashedClientCallbacks)
                        {
                            mClientCallbacks.Remove(crashedCallback);
                        }
                    }
                    finally
                    {
                        mClientsLocker.ReleaseWriterLock();
                    }
                }
                catch (ApplicationException)
                {
                }
            }
        }

        #endregion

        #region Fields
        private readonly IMtApiServer mServer;
        private readonly List<IMtApiCallback> mClientCallbacks = new List<IMtApiCallback>();
        private readonly ReaderWriterLock mClientsLocker = new ReaderWriterLock();
        #endregion
    }
}
