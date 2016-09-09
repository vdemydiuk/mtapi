using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MtApi.Monitors
{
    public abstract class TradeMonitor
    {
        #region Fields
        private readonly MtApiClient _apiClient;
        private List<MtOrder> _prevOrders;
        private readonly object _locker = new object();
        #endregion

        #region ctor
        public TradeMonitor(MtApiClient apiClient)
        {
            if (apiClient == null)
                throw new ArgumentNullException(nameof(apiClient));

            _apiClient = apiClient;
        }
        #endregion

        #region Public Methods
        //
        // Summary:
        //     Gets a value indicating whether the TradeMonitor should raise checking orders
        //
        // Returns:
        //     true if TradeMonitor should check orders
        //     otherwise, false.
        public abstract bool IsStarted { get; }

        //
        // Summary:
        //     Start checking orders.
        //
        public void Start()
        {
            _apiClient.ConnectionStateChanged += _apiClient_ConnectionStateChanged;
            if (IsMtConnected)
            {
                InitialCheck();
            }

            OnStart();
        }

        //
        // Summary:
        //     Stop checking orders.
        //
        public void Stop()
        {
            _apiClient.ConnectionStateChanged -= _apiClient_ConnectionStateChanged;
            OnStop();
        }
        #endregion

        #region Events
        //
        // Summary:
        //     Occurs when orders are opened or closed.
        public event EventHandler<AvailabilityOrdersEventArgs> AvailabilityOrdersChanged;
        #endregion

        #region Protected Methods

        protected abstract void OnStart();
        protected abstract void OnStop();

        protected abstract void OnMtConnected();
        protected abstract void OnMtDisconnected();

        public bool IsMtConnected
        {
            get
            {
                return _apiClient.ConnectionState == MtConnectionState.Connected;
            }
        }

        protected void Check()
        {
            try
            {
                CheckOrders();
            }
            catch (MtConnectionException)
            {
                //TODO: write error to log
            }
            catch (MtExecutionException)
            {
                //TODO: write error to log
            }
        }
        #endregion

        #region Private Methods
        private void CheckOrders()
        {
            var openedOrders = new List<MtOrder>();
            var closedOrders = new List<MtOrder>();

            // get current orders from MetaTrader
            var tradesOrders = _apiClient.GetOrders(OrderSelectSource.MODE_TRADES);

            List<MtOrder> prevOrders;
            lock(_locker)
            {
                prevOrders = _prevOrders;
            }

            if (_prevOrders != null) //skip checking on first load orders
            {
                //check open orders
                foreach (var order in tradesOrders)
                {
                    if (_prevOrders.Find(a => a.Ticket == order.Ticket) == null)
                    {
                        openedOrders.Add(order);
                    }
                }

                //check closed orders
                var closeOrdersTemp = new List<MtOrder>();
                foreach (var order in _prevOrders)
                {
                    if (tradesOrders.Find(a => a.Ticket == order.Ticket) == null)
                    {
                        closeOrdersTemp.Add(order);
                    }
                }

                if (closeOrdersTemp.Count > 0)
                {
                    //get closed orders from history with actual values
                    var historyOrders = _apiClient.GetOrders(OrderSelectSource.MODE_HISTORY);
                    foreach (var order in closeOrdersTemp)
                    {
                        var closedOrder = historyOrders.Find(a => a.Ticket == order.Ticket);
                        if (closedOrder != null)
                        {
                            closedOrders.Add(closedOrder);
                        }
                    }
                }
            }

            lock(_locker)
            {
                _prevOrders = tradesOrders;
            }

            if (openedOrders.Count > 0 || closedOrders.Count > 0)
            {
                AvailabilityOrdersChanged?.Invoke(this, new AvailabilityOrdersEventArgs(openedOrders, closedOrders));
            }
        }

        private void _apiClient_ConnectionStateChanged(object sender, MtConnectionEventArgs e)
        {
            if (e.Status == MtConnectionState.Connected)
            {
                InitialCheck();
                OnMtConnected();
            }
            else if (e.Status == MtConnectionState.Failed || e.Status == MtConnectionState.Disconnected)
            {
                OnMtDisconnected();
            }
        }

        private void InitialCheck()
        {
            lock (_locker)
            {
                _prevOrders = null;
            }

            Task.Factory.StartNew(() =>
            {
                Check();
            });
        }
        #endregion
    }
}