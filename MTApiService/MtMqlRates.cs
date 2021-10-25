using System.Runtime.Serialization;

namespace MTApiService
{
    [DataContract]
    public class MtMqlRates
    {
        [DataMember]
        public long Time { get; set; }         // Period start time
        [DataMember]
        public double Open { get; set; }         // Open price
        [DataMember]
        public double High { get; set; }         // The highest price of the period
        [DataMember]
        public double Low { get; set; }          // The lowest price of the period
        [DataMember]
        public double Close { get; set; }        // Close price
        [DataMember]
        public long Tick_volume { get; set; }  // Tick volume
        [DataMember]
        public int Spread { get; set; }       // Spread
        [DataMember]
        public long Real_volume { get; set; }  // Trade volume
    }
}
