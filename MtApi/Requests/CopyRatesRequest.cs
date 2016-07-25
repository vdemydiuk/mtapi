using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtApi.Requests
{
    public abstract class CopyRatesRequestBase : RequestBase
    {
        public enum CopyRatesTypeEnum
        {
            CopyRates_1 = 1,
            CopyRates_2 = 2,
            CopyRates_3 = 3,
        }

        public override RequestType RequestType
        {
            get { return RequestType.CopyRates; }
        }

        public abstract CopyRatesTypeEnum CopyRatesType { get; }

        public string SymbolName { get; set; }
        public ENUM_TIMEFRAMES Timeframe { get; set; }
    }

    public class CopyRates1Request : CopyRatesRequestBase
    {
        public override CopyRatesTypeEnum CopyRatesType
        {
            get { return CopyRatesTypeEnum.CopyRates_1; }
        }

        public int StartPos { get; set; }
        public int Count { get; set; }
    }

    public class CopyRates2Request : CopyRatesRequestBase
    {
        public override CopyRatesTypeEnum CopyRatesType
        {
            get { return CopyRatesTypeEnum.CopyRates_2; }
        }

        public int StartTime { get; set; }
        public int Count { get; set; }
    }

    public class CopyRates3Request : CopyRatesRequestBase
    {
        public override CopyRatesTypeEnum CopyRatesType
        {
            get { return CopyRatesTypeEnum.CopyRates_3; }
        }

        public int StartTime { get; set; }
        public int StopTime { get; set; }
    }
}
