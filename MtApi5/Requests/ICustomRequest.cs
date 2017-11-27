using System.Collections;

namespace MtApi5.Requests
{
    internal class ICustomRequest : RequestBase
    {
        public enum ParametersType
        {
            Int     = 0,
            Double  = 1,
            String  = 2,
            Boolean = 3
        }

        public string Symbol { get; set; }
        public int Timeframe { get; set; }
        public string Name { get; set; }
        public ArrayList Params { get; set; }
        public ParametersType ParamsType { get; set; }

        public override RequestType RequestType => RequestType.iCustom;
    }
}
