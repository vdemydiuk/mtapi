using MtService;
using Org.SbeTool.Sbe.Dll;
using System;
using System.ComponentModel.Design;
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

        public string? SendCommand(int expertHandle, int commandType, string payload, int timeout = 10000)  // 10 sec
        {
            uint commandId = getNextCommandId();

            int requiredBufferSize = MessageHeader.Size + MtService.MtCommand.BlockLength + 4 + payload.Length; // 4 bytes for length prefix
            byte[] buffer = new byte[requiredBufferSize];
            DirectBuffer directBuffer = new(buffer);

            MessageHeader headerEncoder = new();
            MtService.MtCommand commandEncoder = new();

            // 1. Wrap and write SBE Header
            headerEncoder.Wrap(directBuffer, 0, MtService.MtCommand.SchemaVersion);
            int offset = MessageHeader.Size;

            // 2. Wrap and write message fields
            commandEncoder.WrapForEncodeAndApplyHeader(directBuffer, offset, headerEncoder);
            commandEncoder.Command_id = commandId;
            commandEncoder.Expert_handle = (uint)expertHandle;
            commandEncoder.Command_type = (ushort)commandType;

            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            commandEncoder.SetStringPayload(payloadBytes, 0, payloadBytes.Length);

            var response = Send<string>(buffer, commandId, timeout);
            return response;
        }

        public HashSet<int>? RequestExpertsList()
        {
            uint requestId = getNextCommandId();
            int requiredBufferSize = MessageHeader.Size + MtRequestExpertList.BlockLength;
            byte[] buffer = new byte[requiredBufferSize];
            DirectBuffer directBuffer = new(buffer);

            MessageHeader headerEncoder = new();
            MtRequestExpertList requestEncoder = new();

            // 1. Wrap and write SBE Header
            headerEncoder.Wrap(directBuffer, 0, MtRequestExpertList.SchemaVersion);
            int offset = MessageHeader.Size;

            // 2. Wrap and write message fields
            requestEncoder.WrapForEncodeAndApplyHeader(directBuffer, offset, headerEncoder);
            requestEncoder.Request_id = requestId;

            var response = Send<HashSet<int>>(buffer, requestId, 10000);
            return response;
        }

        private T? Send<T>(byte[] message, uint commandId, int timeout = 10000)
        {
            CommandTask<T> requestTask = new();
            lock (pendingMessages_)
            {
                pendingMessages_.Enqueue(message);
            }
            sendWaiter_.Set();

            var response = requestTask.WaitResponse(timeout);
            lock (tasks_)
            {
                tasks_.Remove(commandId);
            }

            return response;
        }

        private async void DoWrite()
        {
            while(ws_.State == WebSocketState.Open)
            {
                byte[]? message = null;
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
                    await ws_.SendAsync(message, WebSocketMessageType.Binary, true, CancellationToken.None);
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
                using (var ms = new MemoryStream())
                {
                    var messageBuffer = WebSocket.CreateClientBuffer(1024, 64);
                    while (ws_.State == WebSocketState.Open)
                    {
                        int recvCount = 0;
                        WebSocketReceiveResult result;
                        do
                        {
                            result = await ws_.ReceiveAsync(messageBuffer, CancellationToken.None);
                            var byteArray = messageBuffer.Array;
                            if (byteArray != null)
                            {
                                ms.Write(byteArray, messageBuffer.Offset, result.Count);
                                recvCount += result.Count;
                            }
                        }
                        while (!result.EndOfMessage);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            logger_.Info($"MtRpcClient.DoReceive: close signal {result.CloseStatusDescription}");
                            Disconnected?.Invoke(this, EventArgs.Empty);
                            break;
                        }
                        else if (result.MessageType == WebSocketMessageType.Binary)
                        {;
                            OnReceive(ms.ToArray());
                        }
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.Position = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                logger_.Warn($"MtRpcClient.DoReceive: Exception in receive - {ex.Message}");
                ConnectionFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnReceive(byte[] msg)
        {
            logger_.Debug($"MtRpcClient.OnReceive: {msg.Length} bytes");

            // Decoding the Header
            MessageHeader headerDecoder = new();
            var directBuffer = new DirectBuffer(msg);
            headerDecoder.Wrap(directBuffer, 0, MessageHeader.SbeSchemaVersion);

            ushort templateId = headerDecoder.TemplateId;
            ushort blockLength = headerDecoder.BlockLength;
            ushort schemaVersion = headerDecoder.Version;

            switch (templateId)
            {
                case MtService.MtEvent.TemplateId:
                    {
                        MtService.MtEvent eventMsg = new();
                        eventMsg.WrapForDecode(directBuffer, MessageHeader.Size, blockLength, schemaVersion);
                        ProcessEvent(eventMsg);
                    }
                    break;
                case MtService.MtResponse.TemplateId:
                    {
                        MtService.MtResponse responseMsg = new();
                        responseMsg.WrapForDecode(directBuffer, MessageHeader.Size, blockLength, schemaVersion);
                        ProcessResponse(responseMsg);
                    }
                    break;
                case MtExpertList.TemplateId:
                    {
                        MtExpertList expertListMsg = new();
                        expertListMsg.WrapForDecode(directBuffer, MessageHeader.Size, blockLength, schemaVersion);
                        ProcessExpertList(expertListMsg);
                    }
                    break;
                case MtExpertAdded.TemplateId:
                    {
                        MtExpertAdded expertAddedMsg = new();
                        expertAddedMsg.WrapForDecode(directBuffer, MessageHeader.Size, blockLength, schemaVersion);
                        ProcessExpertAdded(expertAddedMsg);
                    }
                    break;
                case MtExpertRemoved.TemplateId:
                    {
                        MtExpertRemoved expertRemovedMsg = new();
                        expertRemovedMsg.WrapForDecode(directBuffer, MessageHeader.Size, blockLength, schemaVersion);
                        ProcessExpertRemoved(expertRemovedMsg);
                    }
                    break;
            }
        }

        private void ProcessEvent(MtService.MtEvent msg)
        {
            var expertHandle = (int)msg.Expert_handle;
            var eventType = (int)msg.Event_type;
            var payload = msg.GetStringPayload();
            logger_.Debug($"MtRpcClient.ProcessEvent: {expertHandle}, {eventType}, [{payload}]");
            MtEventReceived?.Invoke(this, new(expertHandle, eventType, payload));
        }

        private void ProcessResponse(MtService.MtResponse msg)
        {
            var handle = msg.Expert_handle;
            var commandId = msg.Command_id;
            var payload = msg.GetStringPayload();

            logger_.Debug($"MtRpcClient.ProcessResponse: {handle}, {commandId}, [{payload}]");

            lock (tasks_)
            {
                if (tasks_.TryGetValue(commandId, out CommandTask<object>? value))
                    value.SetResponse(payload);
            }
        }

        private void ProcessExpertList(MtExpertList msg)
        {
            var requestId = msg.Request_id;

            // Read the Repeating Group (expert_list)
            var groupDecoder = msg.Expert_list;
            int totalExperts = groupDecoder.Count;

            HashSet<int> expertHandles = [];
            while (groupDecoder.HasNext)
            {
                groupDecoder.Next(); // Advance cursor to the next item in the group
                uint handle = groupDecoder.Expert_handle;
                expertHandles.Add((int)handle);
            }

            logger_.Debug($"MtRpcClient.ProcessExpertList: request ID - {requestId}, experts count - {totalExperts}, expert handles - [{string.Join(", ", expertHandles)}]");

            lock (tasks_)
            {
                if (tasks_.TryGetValue(requestId, out CommandTask<object>? value))
                    value.SetResponse(expertHandles);
            }
        }

        private void ProcessExpertAdded(MtExpertAdded msg)
        {
            var expertHandle = msg.Expert_handle;
            logger_.Debug($"MtRpcClient.ProcessExpertAdded: expert handle - {expertHandle}");
            ExpertAdded?.Invoke(this, new((int)expertHandle));
        }

        private void ProcessExpertRemoved(MtExpertRemoved msg)
        {
            var expertHandle = msg.Expert_handle;
            logger_.Debug($"MtRpcClient.ProcessExpertRemoved: expert handle - {expertHandle}");
            ExpertRemoved?.Invoke(this, new((int)expertHandle));
        }

        private uint getNextCommandId()
        {
            lock (tasks_)
            {
                return nextCommandId++;
            }
        }

        public event EventHandler<EventArgs>? ConnectionFailed;
        public event EventHandler<EventArgs>? Disconnected;
        public event EventHandler<MtExpertEventArgs>? ExpertAdded;
        public event EventHandler<MtExpertEventArgs>? ExpertRemoved;
        public event EventHandler<MtEventArgs>? MtEventReceived;

        private readonly ClientWebSocket ws_ = new();
        private readonly string host_;
        private readonly int port_;
        private readonly Queue<byte[]> pendingMessages_ = [];

        private readonly Thread receiveThread_;
        private readonly Thread sendThread_;
        private readonly EventWaitHandle sendWaiter_ = new AutoResetEvent(false);

        private uint nextCommandId = 0;
        private readonly Dictionary<uint, CommandTask<object>> tasks_ = [];

        private readonly IRpcLogger logger_;
    }

    internal class CommandTask<T>
    {
        private readonly EventWaitHandle responseWaiter_ = new AutoResetEvent(false);
        private T? response_;
        private readonly object locker_ = new();

        public T? WaitResponse(int time)
        {
            responseWaiter_.WaitOne(time);
            lock (locker_)
            {
                return response_;
            }
        }

        public void SetResponse(T result)
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