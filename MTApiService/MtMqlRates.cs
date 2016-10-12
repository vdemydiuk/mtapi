using System.Runtime.Serialization;

namespace MTApiService
{
    [DataContract]
    public class MtMqlRates
    {
        [DataMember]
        public long time { get; set; }         // Period start time
        [DataMember]
        public double open { get; set; }         // Open price
        [DataMember]
        public double high { get; set; }         // The highest price of the period
        [DataMember]
        public double low { get; set; }          // The lowest price of the period
        [DataMember]
        public double close { get; set; }        // Close price
        [DataMember]
        public long tick_volume { get; set; }  // Tick volume
        [DataMember]
        public int spread { get; set; }       // Spread
        [DataMember]
        public long real_volume { get; set; }  // Trade volume
    }
}
