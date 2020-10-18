using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtApi.Monitors
{
    public class MtModifiedOrder
    {
        /// <summary>
        /// The order in its old state (before the changes)
        /// </summary>
        public MtOrder OldOrder { get; }
        /// <summary>
        /// The order in its new state (after the changes)
        /// </summary>
        public MtOrder NewOrder { get; }
        /// <summary>
        /// The changes found by this instance
        /// </summary>
        public OrderModifiedTypes ModifyType { get; private set; }
        /// <summary>
        /// Initializes an instance and compare the order in its old and new state
        /// </summary>
        /// <param name="oldOrder">The order in its old state (before the changes)</param>
        /// <param name="newOrder">The order in its new state (after the changes)</param>
        public MtModifiedOrder(MtOrder oldOrder, MtOrder newOrder)
        {
            if (oldOrder != null && newOrder != null && oldOrder.Ticket != newOrder.Ticket)
                throw new ArgumentException(nameof(oldOrder) + " and " + nameof(newOrder) + " need to have the same ticket id");
            OldOrder = oldOrder;
            NewOrder = newOrder;
            ModifyType = OrderModifiedTypes.None;
            Compare();
        }
        private void Compare()
        {
            if(NewOrder != null && OldOrder != null)
            {
                if (OldOrder.StopLoss != NewOrder.StopLoss)
                    ModifyType |= OrderModifiedTypes.StopLoss;
                if (OldOrder.TakeProfit != NewOrder.TakeProfit)
                    ModifyType |= OrderModifiedTypes.TakeProfit;
                if (OldOrder.Operation != NewOrder.Operation)
                    ModifyType |= OrderModifiedTypes.Operation;
            }
        }
    }
}
