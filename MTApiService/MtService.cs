using System;
using System.Collections.Generic;
using System.ServiceModel;
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

    [ServiceContract]
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

        [OperationContract(IsOneWay = true)]
        void OnMtEvent(MtEvent ntEvent);
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
            ExecuteCallbackAction(a => a.OnServerStopped());
        }

        public void QuoteUpdate(MtQuote quote)
        {
            ExecuteCallbackAction(a => a.OnQuoteUpdate(quote));
        }

        public void OnQuoteAdded(MtQuote quote)
        {
            ExecuteCallbackAction(a => a.OnQuoteAdded(quote));
        }

        public void OnQuoteRemoved(MtQuote quote)
        {
            ExecuteCallbackAction(a => a.OnQuoteRemoved(quote));
        }

        public void OnMtEvent(MtEvent mtEvent)
        {
            ExecuteCallbackAction(a => a.OnMtEvent(mtEvent));
        }
        #endregion

        #region Private Methods

        private void ExecuteCallbackAction(Action<IMtApiCallback> action)
        {
            try
            {
                mClientsLocker.AcquireReaderLock(2000);

                List<IMtApiCallback> crashedClientCallbacks = null;

                try
                {
                    foreach (var callback in mClientCallbacks)
                    {
                        try
                        {
                            action(callback);
                        }
                        catch (Exception)
                        {
                            if (crashedClientCallbacks == null)
                                crashedClientCallbacks = new List<IMtApiCallback>();

                            crashedClientCallbacks.Add(callback);
                        }
                    }

                    if (crashedClientCallbacks != null)
                    {
                        foreach (var crashedCallback in crashedClientCallbacks)
                        {
                            mClientCallbacks.Remove(crashedCallback);
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
                //TODO: add logging
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
