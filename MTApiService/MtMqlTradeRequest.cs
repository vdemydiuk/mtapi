using System;
using System.Runtime.Serialization;

namespace MTApiService
{
    [DataContract]
    public class MtMqlTradeRequest
    {
        [DataMember]
        public int Action { get; set; }
        [DataMember]
        public ulong Magic { get; set; }
        [DataMember]
        public ulong Order { get; set; }
        [DataMember]
        public string Symbol { get; set; }
        [DataMember]
        public double Volume { get; set; }
        [DataMember]
        public double Price { get; set; }
        [DataMember]
        public double Stoplimit { get; set; }
        [DataMember]
        public double Sl { get; set; }
        [DataMember]
        public double Tp { get; set; }
        [DataMember]
        public ulong Deviation { get; set; }
        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public int Type_filling { get; set; }
        [DataMember]
        public int Type_time { get; set; }
        [DataMember]
        public DateTime Expiration { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public ulong Position { get; set; }
        [DataMember]
        public ulong PositionBy { get; set; }
    }
}
