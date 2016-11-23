using System;
using System.Runtime.Serialization;

namespace MTApiService
{
    [DataContract]
    public class MtEvent
    {
        [DataMember]
        public int EventType { get; internal set; }

        [DataMember]
        public string Payload { get; internal set; }

        [DataMember]
        public int ExpertHandle { get; internal set; }

        public override string ToString()
        {
            return $"EventType = {EventType}; Payload = {Payload}; ExpertHandle = {ExpertHandle}";
        }
    }
}
