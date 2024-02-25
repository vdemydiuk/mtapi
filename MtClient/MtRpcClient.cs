using System.Net.WebSockets;
using System.Text;

namespace MtClient
{
    public interface IRpcLogger
    {
        public void Debug(string message);
        public void Info(string message);
        public void Warn(string message);
        public void Error(string message);
    }

    public class MtRpcClient
    {
        public MtRpcClient(string host, int port, IRpcLogger? logger = null)
        {
            host_ = host;
            port_ = port;
            logger_ = logger ?? new StubLogger();

            receiveThread_ = new Thread(new ThreadStart(DoReceive));
            sendThread_ = new Thread(new ThreadStart(DoWrite));
        }

        public async Task Connect()
        {
            logger_.Debug($"MtRpcClient.Connect: started to {host_}:{port_}");

            try
            {
                await ws_.ConnectAsync(new Uri($"ws://{host_}:{port_}/ws"), CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger_.Error($"MtRpcClient.Connect failed: {ex.Message}");
                throw new Exception($"Failed connection to {host_}:{port_}");
            }

            receiveThread_.Start();
            sendThread_.Start();

            logger_.Debug($"MtRpcClient.Connect: success.");
        }

        public async void Disconnect()
        {
            logger_.Debug($"MtRpcClient.Disconnect: {host_}:{port_}");

            try
            {
                if (ws_.State == WebSocketState.Open)
                    await ws_.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                ws_.Dispose();
            }
            catch (Exception ex)
            {
                logger_.Warn($"MtRpcClient.Disconnect: {ex.Message}");
            }

            sendWaiter_.Set();
            sendThread_.Join();
            receiveThread_.Join();

            logger_.Debug($"MtRpcClient.Disconnect: success");
        }

        public string? SendCommand(int expertHandle, int commandType, string payload)
        {
            CommandTask commandTask = new();
            int commandId;
            lock (tasks_)
            {
                commandId = nextCommandId++;
                tasks_[commandId] = commandTask;
            }

            MtCommand command = new(expertHandle, commandType, commandId, payload);
            Send(command);

            var response = commandTask.WaitResponse(10000); // 10 sec
            lock (tasks_)
            {
                tasks_.Remove(commandId);
            }

            return response;
        }

        public void NotifyClientReady()
        {
            MtNotification notification = new(MtNotificationType.ClientReady);
            Send(notification);
        }

        private void Send(MtMessage message)
        {
            lock (pendingMessages_)
            {
                pendingMessages_.Enqueue(message);
            }
            sendWaiter_.Set();
        }

        private async void DoWrite()
        {
            while(ws_.State == WebSocketState.Open)
            {
                MtMessage? message = null;
                lock(pendingMessages_)
                {
                    if (pendingMessages_.Count > 0)
                        message = pendingMessages_.Dequeue();
                }

                if (message == null)
                {
                    sendWaiter_.WaitOne();
                    continue;
                }                   

                try
                {
                    string msgStr = message.Serialize();
                    logger_.Debug($"MtRpcClient.DoWrite: sending message: {msgStr}");
                    byte[] bytes = Encoding.ASCII.GetBytes(msgStr);
                    await ws_.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception e)
                {
                    logger_.Error($"MtRpcClient.DoWrite: {e.Message}");
                }
            }
        }

        private async void DoReceive()
        {
            try
            {
                byte[] recvBuffer = new byte[64 * 1024];
                while (ws_.State == WebSocketState.Open)
                {
                    var result = await ws_.ReceiveAsync(new ArraySegment<byte>(recvBuffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        logger_.Info($"MtRpcClient.DoReceive: close signal {result.CloseStatusDescription}");
                        Disconnected?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                    else
                    {
                        var msg = Encoding.ASCII.GetString(recvBuffer, 0, result.Count);
                        OnReceive(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                logger_.Warn($"MtRpcClient.DoReceive: Exception in receive - {ex.Message}");
                ConnectionFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnReceive(string msg)
        {
            logger_.Debug($"MtRpcClient.OnReceive: {msg}");

            if (string.IsNullOrEmpty(msg))
            {
                logger_.Warn("MtRpcClient.OnReceive: Invalid message (null or empty)");
                return;
            }

            var pieces = msg.Split(";", 2);

            if (pieces.Length != 2
                || string.IsNullOrEmpty(pieces[0])
                || string.IsNullOrEmpty(pieces[1]))
            {
                logger_.Warn("MtRpcClient.OnReceive: Invalid message format.");
                return;
            }                

            MessageType msgType;
            try
            {
                var msgTypeValue = int.Parse(pieces[0]);
                msgType = (MessageType)Enum.ToObject(typeof(MessageType), msgTypeValue);

            }
            catch (Exception e)
            {
                logger_.Error($"MtRpcClient.OnReceive: Parse MessageType failed. {e.Message}");
                return;
            }

            var message = MtMessageParser.Parse(msgType, (pieces[1]));
            if (message == null)
            {
                logger_.Error("MtRpcClient.OnReceive: Failed parse message payload");
                return;
            }

            switch (message.MsgType)
            {
                case MessageType.Event:
                    if (message is MtEvent e)
                        MtEventReceived?.Invoke(this, new(e.ExpertHandle, e.EventType, e.Payload));
                    break;
                case MessageType.Response:
                    if (message is MtResponse response)
                        ProcessResponse(response);
                    break;
                case MessageType.ExpertList:
                    if (message is MtExpertListMsg expertListMsg)
                        ExpertList?.Invoke(this, new(expertListMsg.Experts));
                    break;
                case MessageType.ExpertAdded:
                    if (message is MtExpertAddedMsg expertAddedMsg)
                        ExpertAdded?.Invoke(this, new(expertAddedMsg.ExpertHandle));
                    break;
                case MessageType.ExpertRemoved:
                    if (message is MtExpertRemovedMsg expertRemovedMsg)
                        ExpertRemoved?.Invoke(this, new(expertRemovedMsg.ExpertHandle));
                    break;
            }
        }

        private void ProcessResponse(MtResponse msg)
        {
            var handle = msg.ExpertHandle;
            var commandId = msg.CommandId;
            var payload = msg.Payload;

            logger_.Debug($"MtRpcClient.ProcessResponse: {handle}, {commandId}, [{payload}]");

            lock (tasks_)
            {
                if (tasks_.TryGetValue(commandId, out CommandTask? value))
                    value.SetResponse(payload);
            }
        }

        public event EventHandler<EventArgs>? ConnectionFailed;
        public event EventHandler<EventArgs>? Disconnected;
        public event EventHandler<MtExpertListEventArgs>? ExpertList;
        public event EventHandler<MtExpertEventArgs>? ExpertAdded;
        public event EventHandler<MtExpertEventArgs>? ExpertRemoved;
        public event EventHandler<MtEventArgs>? MtEventReceived;

        private readonly ClientWebSocket ws_ = new();
        private readonly string host_;
        private readonly int port_;
        private readonly byte[] buf_ = new byte[10000];
        private readonly Queue<MtMessage> pendingMessages_ = [];

        private readonly Thread receiveThread_;
        private readonly Thread sendThread_;
        private readonly EventWaitHandle sendWaiter_ = new AutoResetEvent(false);

        private int nextCommandId = 0;
        private readonly Dictionary<int, CommandTask> tasks_ = [];

        private readonly IRpcLogger logger_;
    }

    internal class CommandTask
    {
        private readonly EventWaitHandle responseWaiter_ = new AutoResetEvent(false);
        private string? response_;
        private readonly object locker_ = new();

        public string? WaitResponse(int time)
        {
            responseWaiter_.WaitOne(time);
            lock (locker_)
            {
                return response_;
            }
        }

        public void SetResponse(string result)
        {
            lock (locker_)
            {
                response_ = result;
            }
            responseWaiter_.Set();
        }
    }

    public class MtEventArgs(int expertHandle, int eventType, string payload) : EventArgs
    {
        public int ExpertHandle { get; } = expertHandle;
        public int EventType { get; } = eventType;
        public string Payload { get; } = payload;
    }

    public class MtExpertListEventArgs(HashSet<int> experts) : EventArgs
    {
        public HashSet<int> Experts { get; } = experts;
    }

    public class MtExpertEventArgs(int expert) : EventArgs
    {
        public int Expert { get;  }= expert;
    }

    internal class StubLogger : IRpcLogger
    {
        public void Debug(string message)
        {
        }

        public void Error(string message)
        {
        }

        public void Info(string message)
        {
        }

        public void Warn(string message)
        {
        }
    }
}