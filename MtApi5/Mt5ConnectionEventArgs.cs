using System;

namespace MtApi5
{
    public class Mt5ConnectionEventArgs: EventArgs
    {
        public Mt5ConnectionState Status { get; }
        public string ConnectionMessage { get; }

        public Mt5ConnectionEventArgs(Mt5ConnectionState status, string message)
        {
            Status = status;
            ConnectionMessage = message;
        }
    }
}
