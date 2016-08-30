using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Net;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.Net.Sockets;

namespace MTApiService
{
    internal class MtServer : IDisposable, IMtApiServer
    {
        #region Constants
        private const int WAIT_RESPONSE_TIME = 40000; // 40 sec
        private const int STOP_EXPERT_INTERVAL = 1000; // 1 sec 
        #endregion

        #region ctor
        public MtServer(int port)
        {
            Port = port;
            _service = new MtService(this);
        }
        #endregion

        #region Properties
        public int Port { get; private set; }
        #endregion

        #region Public Methods
        public void Start()
        {
            var hostsInitialized = InitHosts(Port);
            if (hostsInitialized == false)
            {
                Debug.WriteLine("Start: InitHosts failed. ");
            }
        }

        private bool InitHosts(int port)
        {
            lock (_hosts)
            {
                if (_hosts.Count > 0)
                    return false;

                //init local pipe host
                string localUrl = CreateConnectionAddress(null, port, true);
                ServiceHost localServiceHost = CreateServiceHost(localUrl, true);
                if (localServiceHost != null)
                {
                    _hosts.Add(localServiceHost);
                }
                
                //init network hosts
                IPHostEntry ips = Dns.GetHostEntry(Dns.GetHostName());
                if (ips != null)
                {
                    foreach (IPAddress ipAddress in ips.AddressList)
                    {
                        if (ipAddress != null)
                        {
                            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                            {
                                string ip = ipAddress.ToString();
                                string networkUrl = CreateConnectionAddress(ip, port, false);
                                ServiceHost serviceHost = CreateServiceHost(networkUrl, false);
                                if (serviceHost != null)
                                {
                                    _hosts.Add(serviceHost);
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        private ServiceHost CreateServiceHost(string serverUrlAdress, bool local)
        {
            ServiceHost serviceHost = null;            
            if (serverUrlAdress != null)
            {
                try
                {
                    serviceHost = new ServiceHost(_service);
                    var binding = CreateConnectionBinding(local);

                    serviceHost.AddServiceEndpoint(typeof(IMtApi), binding, serverUrlAdress);
                    serviceHost.Open();
                }
                catch(Exception e) 
                {
                    Debug.WriteLine("CreateServiceHost: Create ServiceHost failed. " + e.Message);
                }
            }

            return serviceHost;
        }

        public void AddExpert(MtExpert expert)
        {
            if (expert != null)
            {
                expert.Deinited += expert_Deinited;
                expert.QuoteChanged += expert_QuoteChanged;
                expert.OnMtEvent += Expert_OnMtEvent;

                lock (_experts)
                {
                    _experts.Add(expert);
                }

                _executorManager.AddCommandExecutor(expert);

                _service.OnQuoteAdded(expert.Quote);
            }
        }

        #endregion

        #region IMtApiServerCallback Members

        public MtResponse SendCommand(MtCommand command)
        {
            MtResponse response = null;

            if (command != null)
            {
                var task = new MtCommandTask(command);
                _executorManager.EnqueueCommandTask(task);

                //wait for execute command in MetaTrader
                response = task.WaitResult(WAIT_RESPONSE_TIME);
            }

            return response;
        }

        public IEnumerable<MtQuote> GetQuotes()
        {
            lock (_experts)
            {
                return (from s in _experts select s.Quote);
            }
        }

        #endregion

        #region Private Methods
        private void stopTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var expertsCount = 0;

            lock (_experts)
            {
                expertsCount = _experts.Count();
            }

            if (expertsCount == 0)
            {
                _service.OnStopServer();

                Stop();
            }

            var stopTimer = sender as System.Timers.Timer;
            if (stopTimer != null)
            {
                stopTimer.Stop();
                stopTimer.Elapsed -= stopTimer_Elapsed;                
            }
        }

        private string CreateConnectionAddress(string host, int port, bool local)
        {
            string connectionAddress = null;

            if (local == true)
            {
                //by Pipe
                connectionAddress = "net.pipe://localhost/MtApiService_" + port.ToString();
            }
            else
            {
                //by Socket
                if (host != null)
                {
                    connectionAddress = "net.tcp://" + host.ToString() + ":" + port.ToString() + "/MtApiService";
                }                
            }

            return connectionAddress;
        }

        private static Binding CreateConnectionBinding(bool local)
        {
            Binding connectionBinding = null;

            if (local == true)
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

            return connectionBinding;
        }

        private void Stop()
        {
            _executorManager.Stop();

            lock (_hosts)
            {
                if (_hosts.Count == 0)
                    return;

                try
                {
                    foreach (var host in _hosts)
                    {
                        host.Close();
                    }
                }
                catch (TimeoutException)
                {
                    foreach (var host in _hosts)
                    {
                        host.Abort();
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    _hosts.Clear();
                }
            }

            FireOnStopped();
        }

        private void expert_Deinited(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            var expert = (MtExpert)sender;
            var expertsCount = 0;

            lock (_experts)
            {
                _experts.Remove(expert);
                expertsCount = _experts.Count();
            }

            _executorManager.RemoveCommandExecutor(expert);

            expert.Deinited -= expert_Deinited;
            expert.QuoteChanged -= expert_QuoteChanged;

            _service.OnQuoteRemoved(expert.Quote);

            if (expertsCount == 0)
            {
                var stopTimer = new System.Timers.Timer();
                stopTimer.Elapsed += stopTimer_Elapsed;
                stopTimer.Interval = STOP_EXPERT_INTERVAL;
                stopTimer.Start();
            }
        }

        private void expert_QuoteChanged(MtExpert expert, MtQuote quote)
        {
            _service.QuoteUpdate(quote);
        }

        private void Expert_OnMtEvent(object sender, MtEventArgs e)
        {
            _service.OnMtEvent(e.Event);
        }

        private void FireOnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Events
        public event EventHandler Stopped;
        #endregion

        #region IDispose
        public void Dispose()
        {
            Stop();
        }

        #endregion

        #region Fields              
        private readonly MtService _service;
        private readonly List<ServiceHost> _hosts = new List<ServiceHost>();
        private readonly MtCommandExecutorManager _executorManager = new MtCommandExecutorManager();
        private readonly List<MtExpert> _experts = new List<MtExpert>();
        #endregion
    }
}
