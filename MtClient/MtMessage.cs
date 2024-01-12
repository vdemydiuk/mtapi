using System.ComponentModel.Design;
using System.Data;

namespace MtClient
{
    public enum MessageType
    {
        Command = 0,
        Response = 1,
        Event = 2,
        ExpertList = 3,
        ExpertAdded = 4,
        ExpertRemoved = 5,
        ClientReady = 6
    }

    public abstract class MtMessage
    {
        public abstract MessageType MsgType { get; }

        public string Serialize()
        {
            return $"{(int)MsgType};{GetMessageBody()}";
        }

        protected abstract string GetMessageBody();
    }

    public class MtCommand(int expertHandle, int commandType, int commandId, string payload) : MtMessage
    {
        public override MessageType MsgType => MessageType.Command;

        public int ExpertHandle { private set; get; } = expertHandle;
        public int CommandType { private set; get; } = commandType;
        public int CommandId { private set; get; } = commandId;
        public string Payload { private set; get; } = payload;

        protected override string GetMessageBody()
        {
            return $"{ExpertHandle};{CommandId};{CommandType};{Payload}";
        }
    }

    public class MtClientReady : MtMessage
    {
        public override MessageType MsgType => MessageType.ClientReady;

        protected override string GetMessageBody()
        {
            return string.Empty;
        }
    }

    public class MtEvent(int expertHandle, int eventType, string payload) : MtMessage
    {
        public override MessageType MsgType => MessageType.Event;

        public int ExpertHandle { private set; get; } = expertHandle;
        public int EventType { private set; get; } = eventType;
        public string Payload { private set; get; } = payload;

        protected override string GetMessageBody()
        {
            throw new NotImplementedException();
        }

        public static MtMessage? Parse(string payload)
        {
            var pieces = payload.Split(";", 3);
            if (pieces.Length == 3
                && int.TryParse(pieces[0], out int expertHandle)
                && int.TryParse(pieces[1], out int eventType))
                    return new MtEvent(expertHandle, eventType, pieces[2]);
            return null;
        }
    }

    public class MtExpertAddedMsg(int expertHandle) : MtMessage
    {
        public override MessageType MsgType => MessageType.ExpertAdded;

        public int ExpertHandle { private set; get; } = expertHandle;

        protected override string GetMessageBody()
        {
            throw new NotImplementedException();
        }

        public static MtMessage? Parse(string payload)
        {
            if (int.TryParse(payload, out int expertHandle) == false)
                return null;

            return new MtExpertAddedMsg(expertHandle);
        }
    }

    public class MtExpertRemovedMsg(int expertHandle) : MtMessage
    {
        public override MessageType MsgType => MessageType.ExpertRemoved;

        public int ExpertHandle { private set; get; } = expertHandle;

        protected override string GetMessageBody()
        {
            throw new NotImplementedException();
        }

        public static MtMessage? Parse(string payload)
        {
            if (int.TryParse(payload, out int expertHandle) == false)
                return null;

            return new MtExpertRemovedMsg(expertHandle);
        }
    }

    public class MtExpertListMsg(HashSet<int> experts) : MtMessage
    {
        public override MessageType MsgType => MessageType.ExpertList;

        public HashSet<int> Experts { private set; get; } = experts;

        protected override string GetMessageBody()
        {
            throw new NotImplementedException();
        }

        public static MtMessage? Parse(string payload)
        {
            var pieces = payload.Split(",");
            HashSet<int> handles = [];
            foreach (var p in pieces)
            {
                if (int.TryParse(p, out int expertHandle) == false)
                    return null;
                handles.Add(expertHandle);
            }

            return new MtExpertListMsg(handles);
        }
    }

    public class MtResponse(int expertHandle, int commandId, string payload) : MtMessage
    {
        public override MessageType MsgType => MessageType.Response;

        public int ExpertHandle { private set; get; } = expertHandle;
        public int CommandId { private set; get; } = commandId;
        public string Payload { private set; get; } = payload;

        protected override string GetMessageBody()
        {
            throw new NotImplementedException();
        }

        public static MtMessage? Parse(string payload)
        {
            var pieces = payload.Split(";", 3);
            if (pieces.Length != 3
                || string.IsNullOrEmpty(pieces[0])
                || string.IsNullOrEmpty(pieces[1])
                || string.IsNullOrEmpty(pieces[2]))
                return null;

            if (int.TryParse(pieces[0], out int expertHandle) == false)
                return null;

            if (int.TryParse(pieces[1], out int commandId) == false)
                return null;

            return new MtResponse(expertHandle, commandId, pieces[2]);
        }
    }

    public static class MtMessageParser
    {
        static MtMessageParser()
        {
            msgHandlers_[MessageType.Event] = MtEvent.Parse;
            msgHandlers_[MessageType.Response] = MtResponse.Parse;
            msgHandlers_[MessageType.ExpertList] = MtExpertListMsg.Parse;
            msgHandlers_[MessageType.ExpertAdded] = MtExpertAddedMsg.Parse;
            msgHandlers_[MessageType.ExpertRemoved] = MtExpertRemovedMsg.Parse;
        }

        public static MtMessage? Parse(MessageType msgType, string payload)
        {
            return msgHandlers_[msgType](payload);
        }

        private static readonly Dictionary<MessageType, Func<string, MtMessage?>> msgHandlers_ = new();
    }
}
