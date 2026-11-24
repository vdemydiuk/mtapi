using MtApi5.MtProtocol;
using Newtonsoft.Json.Linq;
using Xunit;
using static MtApi5.Tests.MtApi5ClientTestHelper;

namespace MtApi5.Tests
{
    // Every technical-indicator function must send its own command with the exact parameter names the
    // expert reads, and return the indicator handle from the reply.
    public class MtApi5ClientIndicatorTests
    {
        private const ENUM_TIMEFRAMES Tf = ENUM_TIMEFRAMES.PERIOD_H1;
        private const int TfInt = (int)ENUM_TIMEFRAMES.PERIOD_H1;
        private const ENUM_APPLIED_PRICE Ap = ENUM_APPLIED_PRICE.PRICE_HIGH;
        private const int ApInt = (int)ENUM_APPLIED_PRICE.PRICE_HIGH;
        private const ENUM_APPLIED_VOLUME Av = ENUM_APPLIED_VOLUME.VOLUME_REAL;
        private const int AvInt = (int)ENUM_APPLIED_VOLUME.VOLUME_REAL;
        private const ENUM_MA_METHOD Mm = ENUM_MA_METHOD.MODE_EMA;
        private const int MmInt = (int)ENUM_MA_METHOD.MODE_EMA;

        private static object[] I(string name, Func<MtApi5Client, int> call, Mt5CommandType cmd, object payload) =>
            [name, call, (int)cmd, payload];

        public static IEnumerable<object[]> Indicators()
        {
            yield return I("iAC", c => c.iAC("EURUSD", Tf), Mt5CommandType.iAC, new { Symbol = "EURUSD", Period = TfInt });
            yield return I("iAD", c => c.iAD("EURUSD", Tf, Av), Mt5CommandType.iAD, new { Symbol = "EURUSD", Period = TfInt, AppliedVolume = AvInt });
            yield return I("iADX", c => c.iADX("EURUSD", Tf, 14), Mt5CommandType.iADX, new { Symbol = "EURUSD", Period = TfInt, AdxPeriod = 14 });
            yield return I("iADXWilder", c => c.iADXWilder("EURUSD", Tf, 14), Mt5CommandType.iADXWilder, new { Symbol = "EURUSD", Period = TfInt, AdxPeriod = 14 });
            yield return I("iAlligator", c => c.iAlligator("EURUSD", Tf, 13, 8, 8, 5, 5, 3, Mm, Ap), Mt5CommandType.iAlligator,
                new { Symbol = "EURUSD", Period = TfInt, JawPeriod = 13, JawShift = 8, TeethPeriod = 8, TeethShift = 5, LipsPeriod = 5, LipsShift = 3, MaMethod = MmInt, AppliedPrice = ApInt });
            yield return I("iAMA", c => c.iAMA("EURUSD", Tf, 9, 2, 30, 1, Ap), Mt5CommandType.iAMA,
                new { Symbol = "EURUSD", Period = TfInt, AmaPeriod = 9, FastMaPeriod = 2, SlowMaPeriod = 30, AmaShift = 1, AppliedPrice = ApInt });
            yield return I("iAO", c => c.iAO("EURUSD", Tf), Mt5CommandType.iAO, new { Symbol = "EURUSD", Period = TfInt });
            yield return I("iATR", c => c.iATR("EURUSD", Tf, 14), Mt5CommandType.iATR, new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 14 });
            yield return I("iBearsPower", c => c.iBearsPower("EURUSD", Tf, 13), Mt5CommandType.iBearsPower, new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 13 });
            yield return I("iBands", c => c.iBands("EURUSD", Tf, 20, 1, 2.0, Ap), Mt5CommandType.iBands,
                new { Symbol = "EURUSD", Period = TfInt, BandsPeriod = 20, BandsShift = 1, Deviation = 2.0, AppliedPrice = ApInt });
            yield return I("iBullsPower", c => c.iBullsPower("EURUSD", Tf, 13), Mt5CommandType.iBullsPower, new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 13 });
            yield return I("iCCI", c => c.iCCI("EURUSD", Tf, 14, Ap), Mt5CommandType.iCCI, new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 14, AppliedPrice = ApInt });
            yield return I("iChaikin", c => c.iChaikin("EURUSD", Tf, 3, 10, Mm, Av), Mt5CommandType.iChaikin,
                new { Symbol = "EURUSD", Period = TfInt, FastMaPeriod = 3, SlowMaPeriod = 10, MaMethod = MmInt, AppliedVolume = AvInt });
            yield return I("iDEMA", c => c.iDEMA("EURUSD", Tf, 14, 2, Ap), Mt5CommandType.iDEMA, new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 14, MaShift = 2, AppliedPrice = ApInt });
            yield return I("iDeMarker", c => c.iDeMarker("EURUSD", Tf, 14), Mt5CommandType.iDeMarker, new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 14 });
            yield return I("iEnvelopes", c => c.iEnvelopes("EURUSD", Tf, 14, 2, Mm, Ap, 0.1), Mt5CommandType.iEnvelopes,
                new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 14, MaShift = 2, MaMethod = MmInt, AppliedPrice = ApInt, Deviation = 0.1 });
            yield return I("iForce", c => c.iForce("EURUSD", Tf, 13, Mm, Av), Mt5CommandType.iForce,
                new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 13, MaMethod = MmInt, AppliedVolume = AvInt });
            yield return I("iFractals", c => c.iFractals("EURUSD", Tf), Mt5CommandType.iFractals, new { Symbol = "EURUSD", Period = TfInt });
            yield return I("iFrAMA", c => c.iFrAMA("EURUSD", Tf, 14, 2, Ap), Mt5CommandType.iFrAMA, new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 14, MaShift = 2, AppliedPrice = ApInt });
            yield return I("iGator", c => c.iGator("EURUSD", Tf, 13, 8, 8, 5, 5, 3, Mm, Ap), Mt5CommandType.iGator,
                new { Symbol = "EURUSD", Period = TfInt, JawPeriod = 13, JawShift = 8, TeethPeriod = 8, TeethShift = 5, LipsPeriod = 5, LipsShift = 3, MaMethod = MmInt, AppliedPrice = ApInt });
            yield return I("iIchimoku", c => c.iIchimoku("EURUSD", Tf, 9, 26, 52), Mt5CommandType.iIchimoku, new { Symbol = "EURUSD", Period = TfInt, TenkanSen = 9, KijunSen = 26, SenkouSpanB = 52 });
            yield return I("iBWMFI", c => c.iBWMFI("EURUSD", Tf, Av), Mt5CommandType.iBWMFI, new { Symbol = "EURUSD", Period = TfInt, AppliedVolume = AvInt });
            yield return I("iMomentum", c => c.iMomentum("EURUSD", Tf, 14, Ap), Mt5CommandType.iMomentum, new { Symbol = "EURUSD", Period = TfInt, MomPeriod = 14, AppliedPrice = ApInt });
            yield return I("iMFI", c => c.iMFI("EURUSD", Tf, 14, Av), Mt5CommandType.iMFI, new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 14, AppliedVolume = AvInt });
            yield return I("iMA", c => c.iMA("EURUSD", Tf, 14, 2, Mm, Ap), Mt5CommandType.iMA, new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 14, MaShift = 2, MaMethod = MmInt, AppliedPrice = ApInt });
            yield return I("iOsMA", c => c.iOsMA("EURUSD", Tf, 12, 26, 9, Ap), Mt5CommandType.iOsMA, new { Symbol = "EURUSD", Period = TfInt, FastEmaPeriod = 12, SlowEmaPeriod = 26, SignalPeriod = 9, AppliedPrice = ApInt });
            yield return I("iMACD", c => c.iMACD("EURUSD", Tf, 12, 26, 9, Ap), Mt5CommandType.iMACD, new { Symbol = "EURUSD", Period = TfInt, FastEmaPeriod = 12, SlowEmaPeriod = 26, SignalPeriod = 9, AppliedPrice = ApInt });
            yield return I("iOBV", c => c.iOBV("EURUSD", Tf, Av), Mt5CommandType.iOBV, new { Symbol = "EURUSD", Period = TfInt, AppliedVolume = AvInt });
            yield return I("iSAR", c => c.iSAR("EURUSD", Tf, 0.02, 0.2), Mt5CommandType.iSAR, new { Symbol = "EURUSD", Period = TfInt, Step = 0.02, Maximum = 0.2 });
            yield return I("iRSI", c => c.iRSI("EURUSD", Tf, 14, Ap), Mt5CommandType.iRSI, new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 14, AppliedPrice = ApInt });
            yield return I("iRVI", c => c.iRVI("EURUSD", Tf, 10), Mt5CommandType.iRVI, new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 10 });
            yield return I("iStdDev", c => c.iStdDev("EURUSD", Tf, 20, 1, Mm, Ap), Mt5CommandType.iStdDev, new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 20, MaShift = 1, MaMethod = MmInt, AppliedPrice = ApInt });
            yield return I("iStochastic", c => c.iStochastic("EURUSD", Tf, 5, 3, 3, Mm, ENUM_STO_PRICE.STO_CLOSECLOSE), Mt5CommandType.iStochastic,
                new { Symbol = "EURUSD", Period = TfInt, Kperiod = 5, Dperiod = 3, Slowing = 3, MaMethod = MmInt, PriceField = (int)ENUM_STO_PRICE.STO_CLOSECLOSE });
            yield return I("iTEMA", c => c.iTEMA("EURUSD", Tf, 14, 2, Ap), Mt5CommandType.iTEMA, new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 14, MaShift = 2, AppliedPrice = ApInt });
            yield return I("iTriX", c => c.iTriX("EURUSD", Tf, 14, Ap), Mt5CommandType.iTriX, new { Symbol = "EURUSD", Period = TfInt, MaPeriod = 14, AppliedPrice = ApInt });
            yield return I("iWPR", c => c.iWPR("EURUSD", Tf, 14), Mt5CommandType.iWPR, new { Symbol = "EURUSD", Period = TfInt, CalcPeriod = 14 });
            yield return I("iVIDyA", c => c.iVIDyA("EURUSD", Tf, 9, 12, 2, Ap), Mt5CommandType.iVIDyA, new { Symbol = "EURUSD", Period = TfInt, CmoPeriod = 9, EmaPeriod = 12, MaShift = 2, AppliedPrice = ApInt });
            yield return I("iVolumes", c => c.iVolumes("EURUSD", Tf, Av), Mt5CommandType.iVolumes, new { Symbol = "EURUSD", Period = TfInt, AppliedVolume = AvInt });
        }

        [Theory]
        [MemberData(nameof(Indicators))]
        public async Task Indicator_SendsExpectedPayload_AndReturnsHandle(
            string name, Func<MtApi5Client, int> call, int command, object payload)
        {
            var (client, fake) = await Connect();
            fake.Reply("321");

            var handle = call(client);

            Assert.Equal(321, handle);
            var sent = fake.Last();
            Assert.True(command == sent.CommandType, $"{name}: command {sent.CommandType} != expected {command}");
            Assert.True(JToken.DeepEquals(JObject.FromObject(payload), JObject.Parse(sent.Payload)),
                $"{name}: payload {sent.Payload} != expected {JObject.FromObject(payload)}");
        }

        [Fact]
        public void Indicators_TableCoversEveryIndicatorFunction()
        {
            // Guards against adding a new iXxx function without a test row.
            var covered = Indicators().Select(r => (string)r[0]).ToHashSet();
            var all = typeof(MtApi5Client).GetMethods()
                .Where(m => m.DeclaringType == typeof(MtApi5Client) && m.Name.Length > 1
                    && m.Name[0] == 'i' && char.IsUpper(m.Name[1]) && m.Name != "iCustom")
                .Select(m => m.Name).Distinct().ToList();
            Assert.Empty(all.Except(covered));
        }

        [Fact]
        public async Task Indicator_ErrorReply_Throws()
        {
            var (client, fake) = await Connect();
            fake.Handler = (_, _, _) => "{\"ErrorCode\":4802,\"ErrorMessage\":\"Indicator cannot be created\"}";

            var ex = Assert.Throws<ExecutionException>(() => client.iRSI("EURUSD", Tf, 14, Ap));

            Assert.Equal(ErrorCode.ErrIndicatorCannotCreate, ex.ErrorCode);
        }
    }
}
