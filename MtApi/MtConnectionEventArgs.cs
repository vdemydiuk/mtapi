using System;

namespace MtApi
{
    public class MtConnectionEventArgs: EventArgs
    {
        public MtConnectionState Status { get; private set; }
        public string ConnectionMessage { get; private set; }

        public MtConnectionEventArgs(MtConnectionState status, string message)
        {
            Status = status;
            ConnectionMessage = message;
        }
    }
}
