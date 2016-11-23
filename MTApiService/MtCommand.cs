using System.Runtime.Serialization;
using System.Collections;

namespace MTApiService
{
    [DataContract]
    public class MtCommand
    {
        public MtCommand(int commandType, ArrayList parameters)
        {
            CommandType = commandType;
            Parameters = parameters;
        }

        [DataMember]
        public int CommandType { get; set; }

        [DataMember]
        public ArrayList Parameters { get; set; }

        [DataMember]
        public int ExpertHandle { get; set; }

        public override string ToString()
        {
            return $"CommandType = {CommandType}";
        }
    }
}
