using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtApi.Monitors
{
    public class ModifiedOrdersEventArgs : EventArgs
    {
        /// <summary>
        /// Returns a list of all modified orders
        /// </summary>
        public List<MtModifiedOrder> ModifiedOrders { get; }
        public ModifiedOrdersEventArgs(List<MtModifiedOrder> modifiedOrders)
        {
            ModifiedOrders = modifiedOrders;
        }
    }
}
