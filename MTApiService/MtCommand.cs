using System.Runtime.Serialization;
using System.Collections;

namespace MTApiService
{
    [DataContract]
    public class MtCommand
    {
        [DataMember]
        public int CommandType { get; set; }

        [DataMember]
        public ArrayList Parameters { get; set; }

        [DataMember]
        public int ExpertHandle { get; set; }

        public override string ToString()
        {
            return $"CommandType = {CommandType}; ExpertHandle = {ExpertHandle}";
        }
    }
}
