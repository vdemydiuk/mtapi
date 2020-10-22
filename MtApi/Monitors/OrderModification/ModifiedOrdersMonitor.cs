using MtApi.Monitors.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtApi.Monitors
{
    public class ModifiedOrdersMonitor : MtMonitorBase
    {
        #region Fields
        private List<MtOrder> _lastOrders = null;

        #endregion

        #region Properties
        /// <summary>
        /// Define on which types of modification this monitor should raise <see cref="OrdersModified"/>
        /// </summary>
        public OrderModifiedTypes OrderModifiedTypes { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Will be raised when this monitor detects changes on open orders
        /// </summary>
        public event EventHandler<ModifiedOrdersEventArgs> OrdersModified;
        #endregion

        #region ctor
        public ModifiedOrdersMonitor(MtApiClient apiClient, IMonitorTrigger monitorTrigger, OrderModifiedTypes orderModifiedTypes = OrderModifiedTypes.All, bool syncTrigger = false)
            : base(apiClient, monitorTrigger, syncTrigger)
        {
            _lastOrders = GetOrders();
            OrderModifiedTypes = orderModifiedTypes;
        }
        #endregion

        /// <summary>
        /// Requests all current open orders
        /// </summary>
        /// <returns></returns>
        private List<MtOrder> GetOrders() => IsMtConnected ? ApiClient.GetOrders(OrderSelectSource.MODE_TRADES) : null;
        protected override void OnTriggerRaised()
        {
            if(_lastOrders == null)
            {
                _lastOrders = GetOrders();
                return;
            }
            List<MtOrder> currentOrders = GetOrders();
            OrderModifiedTypes omt = OrderModifiedTypes;
            var mtModifiedOrders = currentOrders
                .Select(co => new MtModifiedOrder(_lastOrders.FirstOrDefault(x => x.Ticket == co.Ticket), co))
                .ToList();
            List<MtModifiedOrder> modifiedOrders = new List<MtModifiedOrder>();
            modifiedOrders.AddRange(GetMtModifiedOrdersWithModType(mtModifiedOrders, omt, OrderModifiedTypes.TakeProfit)); //If the takeprofit were changed between both calls
            modifiedOrders.AddRange(GetMtModifiedOrdersWithModType(mtModifiedOrders, omt, OrderModifiedTypes.StopLoss)); //If the stoploss were changed between both calls
            modifiedOrders.AddRange(GetMtModifiedOrdersWithModType(mtModifiedOrders, omt, OrderModifiedTypes.Operation)); //If an order changed from limit / stop order to an open order
            if (modifiedOrders.Count > 0)
                OrdersModified?.Invoke(this, new ModifiedOrdersEventArgs(modifiedOrders));
            _lastOrders = currentOrders;
        }
        private static IEnumerable<MtModifiedOrder> GetMtModifiedOrdersWithModType(IEnumerable<MtModifiedOrder> orders, OrderModifiedTypes globalSearchFlag, OrderModifiedTypes modifiedType)
            => globalSearchFlag.HasFlag(modifiedType) ? orders.Where(o => o.ModifyType.HasFlag(modifiedType)) : new List<MtModifiedOrder>();
    }
}