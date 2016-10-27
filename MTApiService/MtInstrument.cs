using System.Runtime.Serialization;

namespace MTApiService
{
    [DataContract]
    public class MtQuote
    {
        [DataMember]
        public string Instrument { get; private set; }
        
        [DataMember]
        public double Bid { get; private set; }

        [DataMember]
        public double Ask { get; private set; }

        public MtQuote(string instrument, double bid, double ask)
        {
            Instrument = instrument;
            Bid = bid;
            Ask = ask;
        }

        public override string ToString()
        {
            return "Instrument = " + Instrument + ", Bid = " + Bid + ", Ask = " + Ask;
        }
    }
}
