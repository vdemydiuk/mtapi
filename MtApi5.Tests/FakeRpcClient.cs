using MtClient;

namespace MtApi5.Tests
{
    // In-memory replacement for MtRpcClient. Commands are answered by a handler that returns
    // the response JSON, which lets tests control what "MetaTrader" replies.
    internal sealed class FakeRpcClient : IMtRpcClient
    {
        public record SentCommand(int ExpertHandle, int CommandType, string Payload, int Timeout);

        public List<SentCommand> Sent { get; } = [];
        public HashSet<int>? Experts { get; set; } = [1];
        public Exception? ConnectException { get; set; }
        public int ConnectCalls { get; private set; }
        public int DisconnectCalls { get; private set; }

        // (expertHandle, commandType, payload) -> response json
        public Func<int, int, string, string?> Handler { get; set; } = (_, _, _) => Ok("null");

        public static string Ok(string valueJson) => "{\"ErrorCode\":0,\"Value\":" + valueJson + "}";

        public Task Connect()
        {
            ConnectCalls++;
            return ConnectException != null ? Task.FromException(ConnectException) : Task.CompletedTask;
        }

        public void Disconnect() => DisconnectCalls++;

        public string? SendCommand(int expertHandle, int commandType, string payload, int timeout = 10000)
        {
            lock (Sent) Sent.Add(new SentCommand(expertHandle, commandType, payload, timeout));
            return Handler(expertHandle, commandType, payload);
        }

        public HashSet<int>? RequestExpertsList() => Experts;

        public event EventHandler<EventArgs>? ConnectionFailed;
        public event EventHandler<EventArgs>? Disconnected;
        public event EventHandler<MtExpertEventArgs>? ExpertAdded;
        public event EventHandler<MtExpertEventArgs>? ExpertRemoved;
        public event EventHandler<MtEventArgs>? MtEventReceived;

        public void RaiseConnectionFailed() => ConnectionFailed?.Invoke(this, EventArgs.Empty);
        public void RaiseDisconnected() => Disconnected?.Invoke(this, EventArgs.Empty);
        public void RaiseExpertAdded(int handle) => ExpertAdded?.Invoke(this, new MtExpertEventArgs(handle));
        public void RaiseExpertRemoved(int handle) => ExpertRemoved?.Invoke(this, new MtExpertEventArgs(handle));
        public void RaiseEvent(int handle, int eventType, string payload) =>
            MtEventReceived?.Invoke(this, new MtEventArgs(handle, eventType, payload));
    }
}
