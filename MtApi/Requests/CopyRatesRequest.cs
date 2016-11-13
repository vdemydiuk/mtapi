namespace MtApi.Requests
{
    internal abstract class CopyRatesRequestBase : RequestBase
    {
        public enum CopyRatesTypeEnum
        {
            CopyRates_1 = 1,
            CopyRates_2 = 2,
            CopyRates_3 = 3,
        }

        public override RequestType RequestType => RequestType.CopyRates;

        public abstract CopyRatesTypeEnum CopyRatesType { get; }

        public string SymbolName { get; set; }
        public ENUM_TIMEFRAMES Timeframe { get; set; }
    }

    internal class CopyRates1Request : CopyRatesRequestBase
    {
        public override CopyRatesTypeEnum CopyRatesType => CopyRatesTypeEnum.CopyRates_1;

        public int StartPos { get; set; }
        public int Count { get; set; }
    }

    internal class CopyRates2Request : CopyRatesRequestBase
    {
        public override CopyRatesTypeEnum CopyRatesType => CopyRatesTypeEnum.CopyRates_2;

        public int StartTime { get; set; }
        public int Count { get; set; }
    }

    internal class CopyRates3Request : CopyRatesRequestBase
    {
        public override CopyRatesTypeEnum CopyRatesType => CopyRatesTypeEnum.CopyRates_3;

        public int StartTime { get; set; }
        public int StopTime { get; set; }
    }
}
