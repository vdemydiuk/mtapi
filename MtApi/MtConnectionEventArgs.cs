namespace MtApi
{
    public class MtConnectionEventArgs(MtConnectionState status, string message) : EventArgs
    {
        public MtConnectionState Status { get; private set; } = status;
        public string ConnectionMessage { get; private set; } = message;
    }
}
