using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtApi5
{
    public class Mt5ConnectionEventArgs: EventArgs
    {
        public Mt5ConnectionState Status { get; private set; }
        public String ConnectionMessage { get; private set; }

        public Mt5ConnectionEventArgs(Mt5ConnectionState status, string message)
        {
            Status = status;
            ConnectionMessage = message;
        }
    }
}
