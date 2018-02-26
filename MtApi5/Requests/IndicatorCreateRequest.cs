using System.Collections.Generic;

namespace MtApi5.Requests
{
    internal class IndicatorCreateRequest: RequestBase
    {
        public override RequestType RequestType => RequestType.IndicatorCreate;

        public string Symbol { get; set; }
        public ENUM_TIMEFRAMES Period { get; set; }
        public ENUM_INDICATOR IndicatorType { get; set; }
        public List<MqlParam> Parameters { get; set; }
    }
}