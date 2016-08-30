using System;
using System.Runtime.Serialization;

namespace MTApiService
{
    [DataContract]
    public class MtEvent
    {
        [DataMember]
        public int EventType { get; private set; }

        [DataMember]
        public string Payload { get; private set; }

        public MtEvent(int eventType, string payload)
        {
            EventType = eventType;
            Payload = payload;
        }

        public override string ToString()
        {
            return "MtEvent = " + EventType + ", Payload = " + Payload;
        }
    }

    public class MtEventArgs: EventArgs
    {
        public MtEventArgs(MtEvent e)
        {
            Event = e;
        }

        public MtEvent Event { get; private set; }
    }
}
