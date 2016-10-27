using System.Runtime.Serialization;

namespace MTApiService
{
    [DataContract]
    public class MtMqlTick
    {
        [DataMember]
        public long time { get; set; }          // Time of the last prices update
        
        [DataMember]
        public double bid { get; set; }           // Current Bid price

        [DataMember]
        public double ask { get; set; }           // Current Ask price

        [DataMember]
        public double last { get; set; }          // Price of the last deal (Last)

        [DataMember]
        public ulong volume { get; set; }        // Volume for the current Last price
    }
}
