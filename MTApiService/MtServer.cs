using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.Net;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.Runtime.InteropServices;

namespace MTApiService
{
    class MtServer : IDisposable, IMtApiServer
    {
        #region Constants
        private const int WAIT_RESPONSE_TIME = 40000; // 40 sec
        private const int STOP_EXPERT_INTERVAL = 1000; // 1 sec 
        #endregion

        #region ctor
        public MtServer(MtConnectionProfile profile)
        {
            Profile = profile;
            mService = new MtService(this);

            mExecutorManager = new MtCommandExecutorManager();
            mExecutorManager.CommandExecuted += mExecutorManager_CommandExecuted;
        }
        #endregion

        #region Properties
        public MtConnectionProfile Profile { get; private set; }        
        #endregion

        #region Public Methods
        public void Start()
        {
            lock (mHostLocker)
            {
                if (mHost != null)
                    return;
                            
                ServiceHost host = new ServiceHost(mService);

                string mServerUrlAdress = CreateConnectionAddress(Profile);
                Binding mBinding = CreateConnectionBinding(Profile);

                host.AddServiceEndpoint(typeof(IMtApi), mBinding, mServerUrlAdress);
                host.Open();

                mHost = host;
            }

            lock (mExpertsLocker)
            {
                mExperts = new List<MtExpert>();    
            }
        }

        public void AddExpert(MtExpert expert)
        {
            if (expert != null)
            {
                expert.Deinited += new EventHandler(expert_Deinited);
                expert.QuoteChanged += new MtExpert.MtQuoteHandler(expert_QuoteChanged);

                lock (mExpertsLocker)
                {
                    mExperts.Add(expert);
                }

                mExecutorManager.AddCommandExecutor(expert);

                mService.OnQuoteAdded(expert.Quote);
            }
        }

        #endregion

        #region IMtApiServerCallback Members

        public MtResponse SendCommand(MtCommand command)
        {
            MtResponse response = null;

            if (command != null)
            {
                EventWaitHandle responseWaiter = new AutoResetEvent(false);

                lock (mResponseLocker)
                {
                    mResponseWaiters[command] = responseWaiter;
                }

                mExecutorManager.EnqueueCommand(command);

                //wait for execute command in MetaTrader
                responseWaiter.WaitOne(WAIT_RESPONSE_TIME);

                lock (mResponseLocker)
                {
                    if (mResponseWaiters.ContainsKey(command) == true)
                    {
                        mResponseWaiters.Remove(command);
                    }

                    if (mResponses.ContainsKey(command) == true)
                    {
                        response = mResponses[command];
                        mResponses.Remove(command);
                    }
                }
            }

            return response;
        }

        public IEnumerable<MtQuote> GetQuotes()
        {
            lock (mExpertsLocker)
            {
                return (from s in mExperts select s.Quote);
            }
        }

        #endregion

        #region Private Methods
        private void stopTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int expertsCount = 0;

            lock (mExpertsLocker)
            {
                expertsCount = mExperts.Count();
            }

            if (expertsCount == 0)
            {
                mService.OnStopServer();

                stop();
            }

            var stopTimer = sender as System.Timers.Timer;
            stopTimer.Stop();
            stopTimer.Elapsed -= stopTimer_Elapsed;
        }

        private string CreateConnectionAddress(MtConnectionProfile profile)
        {
            string connectionAddress = null;

            if (profile != null)
            {
                if (string.IsNullOrEmpty(profile.Host))
                {
                    //by Pipe
                    connectionAddress = "net.pipe://localhost/MtApiService_" + profile.Port.ToString();
                }
                else
                {
                    //by Socket
                    connectionAddress = "net.tcp://" + profile.Host.ToString() + ":" + profile.Port.ToString() + "/MtApiService";
                }
            }

            return connectionAddress;
        }

        private static Binding CreateConnectionBinding(MtConnectionProfile profile)
        {
            Binding connectionBinding = null;

            if (profile != null)
            {
                if (string.IsNullOrEmpty(profile.Host))
                {
                    //by Pipe
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

                    connectionBinding = bind;
                }
                else
                {
                    //by Socket
                    var bind = new NetTcpBinding(SecurityMode.None);
                    bind.MaxReceivedMessageSize = 2147483647;
                    bind.MaxBufferSize = 2147483647;

                    // Commented next statement since it is not required
                    bind.MaxBufferPoolSize = 2147483647;
                    bind.ReaderQuotas.MaxArrayLength = 2147483647;
                    bind.ReaderQuotas.MaxBytesPerRead = 2147483647;
                    bind.ReaderQuotas.MaxDepth = 2147483647;
                    bind.ReaderQuotas.MaxStringContentLength = 2147483647;
                    bind.ReaderQuotas.MaxNameTableCharCount = 2147483647;

                    connectionBinding = bind;
                }
            }

            return connectionBinding;
        }

        private void stop()
        {
            mExecutorManager.Stop();

            lock (mHostLocker)
            {
                if (mHost == null)
                    return;

                try
                {
                    mHost.Close();
                }
                catch (TimeoutException)
                {
                    mHost.Abort();
                }
                catch (Exception)
                {
                }
                finally
                {
                    mHost = null;
                }
            }

            if (Stopped != null)
            {
                Stopped(this, EventArgs.Empty);
            }
        }

        private void expert_Deinited(object sender, EventArgs e)
        {
            MtExpert expert = (MtExpert)sender;
            int expertsCount = 0;

            lock (mExpertsLocker)
            {
                mExperts.Remove(expert);

                expertsCount = mExperts.Count();
            }

            mExecutorManager.RemoveCommandExecutor(expert);

            if (expert != null)
            {
                expert.Deinited -= expert_Deinited;
                expert.QuoteChanged -= expert_QuoteChanged;

                mService.OnQuoteRemoved(expert.Quote);
            }

            if (expertsCount == 0)
            {
                var stopTimer = new System.Timers.Timer();
                stopTimer.Elapsed += stopTimer_Elapsed;
                stopTimer.Interval = STOP_EXPERT_INTERVAL;
                stopTimer.Start();
            }
        }

        void mExecutorManager_CommandExecuted(object sender, MtCommandExecuteEventArgs e)
        {
            EventWaitHandle responseWaiter = null;

            lock (mResponseLocker)
            {
                if (mResponseWaiters.ContainsKey(e.Command) == true)
                {
                    responseWaiter = mResponseWaiters[e.Command];
                    mResponses[e.Command] = e.Response;
                }
            }

            if (responseWaiter != null)
            {
                responseWaiter.Set();
            }
        }

        private void expert_QuoteChanged(MtExpert expert, MtQuote quote)
        {
            mService.QuoteUpdate(quote);
        }

        #endregion

        #region Events
        public event EventHandler Stopped;
        #endregion

        #region IDispose
        public void Dispose()
        {
            stop();
        }

        #endregion

        #region Fields              
        private readonly MtService mService;
        private ServiceHost mHost;        

        private readonly MtCommandExecutorManager mExecutorManager;
        private List<MtExpert> mExperts;

        private readonly Dictionary<MtCommand, EventWaitHandle> mResponseWaiters = new Dictionary<MtCommand, EventWaitHandle>();
        private readonly Dictionary<MtCommand, MtResponse> mResponses = new Dictionary<MtCommand, MtResponse>();

        private readonly object mResponseLocker = new object();
        private readonly object mHostLocker = new object();
        private readonly object mExpertsLocker = new object();
        private readonly object mCommandExecutorsLocker = new object();
        #endregion
    }
}
