using System.Runtime.Serialization;

namespace MTApiService
{
    [DataContract]
    public class MtMqlBookInfo
    {
        [DataMember]
        public int type { get; set; }

        [DataMember]
        public double price { get; set; }

        [DataMember]
        public long volume { get; set; }  
    }
}
