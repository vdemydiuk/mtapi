using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtApi
{
    public class MtConnectionEventArgs: EventArgs
    {
        public MtConnectionState Status { get; private set; }
        public String ConnectionMessage { get; private set; }

        public MtConnectionEventArgs(MtConnectionState status, string message)
        {
            Status = status;
            ConnectionMessage = message;
        }
    }
}
