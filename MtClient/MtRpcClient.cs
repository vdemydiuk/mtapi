using System.Net.WebSockets;
using System.Text;

namespace MtClient
{
    public class MtRpcClient
    {
        public MtRpcClient(string host, int port)
        {
            host_ = host;
            port_ = port;

            receiveThread_ = new Thread(new ThreadStart(DoReceive));
            sendThread_ = new Thread(new ThreadStart(DoWrite));
        }

        public async Task Connect()
        {
            Log($"Connect: started to {host_}:{port_}");

            try
            {
                await ws_.ConnectAsync(new Uri($"ws://{host_}:{port_}/ws"), CancellationToken.None);
            }
            catch (Exception ex)
            {
                Log($"Connect failed: {ex.Message}");
                throw new Exception($"Failed connection to {host_}:{port_}");
            }

            receiveThread_.Start();
            sendThread_.Start();

            Log($"Connect: success.");
        }

        public async void Disconnect()
        {
            Log($"Disconnect: {host_}:{port_}");

            try
            {
                if (ws_.State == WebSocketState.Open)
                    await ws_.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                ws_.Dispose();
            }
            catch (Exception ex)
            {
                Log($"Disconnect: {ex.Message}");
            }

            sendWaiter_.Set();
            sendThread_.Join();
            receiveThread_.Join();

            Log($"Disconnect: success");
        }

        public void Send(MtMessage message)
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
                    Log($"DoWrite: sending message: {msgStr}");
                    byte[] bytes = Encoding.ASCII.GetBytes(msgStr);
                    await ws_.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception e)
                {
                    Log($"DoWrite: {e.Message}");
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
                        Log($"DoReceive: close signal {result.CloseStatusDescription}");
                        await ws_.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
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
                Log($"Exception in receive - {ex.Message}");
                ConnectionFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnReceive(string msg)
        {
            Log($"OnReceive: {msg}");

            if (string.IsNullOrEmpty(msg))
            {
                Log("OnReceive: Invalid message (null or empty)");
                return;
            }

            var pieces = msg.Split(";", 2);

            if (pieces.Length != 2
                || string.IsNullOrEmpty(pieces[0])
                || string.IsNullOrEmpty(pieces[1]))
            {
                Log("OnReceive: Invalid message format.");
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
                Log($"OnReceive: Parse MessageType failed. {e.Message}");
                return;
            }

            var message = MtMessageParser.Parse(msgType, (pieces[1]));
            if (message == null)
            {
                Log("OnReceive: Failed parse message payload");
                return;
            }
            
            MessageReceived?.Invoke(this, message);
        }

        private void Log(string msg)
        {
            Console.WriteLine($"[{Environment.CurrentManagedThreadId}] {msg}");
        }

        public event EventHandler<MtMessage>? MessageReceived;
        public event EventHandler<EventArgs>? ConnectionFailed;
        public event EventHandler<EventArgs>? Disconnected;

        private readonly ClientWebSocket ws_ = new();
        private readonly string host_;
        private readonly int port_;
        private readonly byte[] buf_ = new byte[10000];
        private readonly Queue<MtMessage> pendingMessages_ = [];

        private readonly Thread receiveThread_;
        private readonly Thread sendThread_;
        private readonly EventWaitHandle sendWaiter_ = new AutoResetEvent(false);
    }
}