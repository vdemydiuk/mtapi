using MtApi5.MtProtocol;
using Newtonsoft.Json.Linq;
using Xunit;
using static MtApi5.Tests.MtApi5ClientTestHelper;

namespace MtApi5.Tests
{
    // The single-bar timeseries access functions (iClose, iOpen, iBarShift, iHighest, ...) must send their
    // own command with the exact parameter names the expert reads, and convert the reply.
    public class MtApi5ClientTimeseriesTests
    {
        private const ENUM_TIMEFRAMES Tf = ENUM_TIMEFRAMES.PERIOD_H1;
        private const int TfInt = (int)ENUM_TIMEFRAMES.PERIOD_H1;

        // Not indicators, although their names start with 'i' (see MtApi5ClientIndicatorTests).
        public static readonly HashSet<string> Functions =
        [
            "iBars", "iBarShift", "iClose", "iHigh", "iHighest", "iLow", "iLowest",
            "iOpen", "iTime", "iTickVolume", "iRealVolume", "iVolume", "iSpread"
        ];

        private static object[] F(string name, string reply, Func<MtApi5Client, object> call, Mt5CommandType cmd, object payload, object expected) =>
            [name, reply, call, (int)cmd, payload, expected];

        private static object Shift(int shift) => new { Symbol = "EURUSD", Timeframe = TfInt, Shift = shift };

        public static IEnumerable<object[]> Calls()
        {
            yield return F("iBars", "500", c => c.iBars("EURUSD", Tf), Mt5CommandType.iBars,
                new { Symbol = "EURUSD", Timeframe = TfInt }, 500);
            yield return F("iBarShift", "7", c => c.iBarShift("EURUSD", Tf, Time, true), Mt5CommandType.iBarShift,
                new { Symbol = "EURUSD", Timeframe = TfInt, Time = MtTime, Exact = true }, 7);
            yield return F("iClose", "1.1", c => c.iClose("EURUSD", Tf, 1), Mt5CommandType.iClose, Shift(1), 1.1);
            yield return F("iHigh", "1.2", c => c.iHigh("EURUSD", Tf, 2), Mt5CommandType.iHigh, Shift(2), 1.2);
            yield return F("iHighest", "4", c => c.iHighest("EURUSD", Tf, ENUM_SERIESMODE.MODE_HIGH, 10, 1), Mt5CommandType.iHighest,
                new { Symbol = "EURUSD", Timeframe = TfInt, Type = (int)ENUM_SERIESMODE.MODE_HIGH, Count = 10, Start = 1 }, 4);
            yield return F("iLow", "1.05", c => c.iLow("EURUSD", Tf, 3), Mt5CommandType.iLow, Shift(3), 1.05);
            yield return F("iLowest", "9", c => c.iLowest("EURUSD", Tf, ENUM_SERIESMODE.MODE_LOW, 20, 2), Mt5CommandType.iLowest,
                new { Symbol = "EURUSD", Timeframe = TfInt, Type = (int)ENUM_SERIESMODE.MODE_LOW, Count = 20, Start = 2 }, 9);
            yield return F("iOpen", "1.15", c => c.iOpen("EURUSD", Tf, 0), Mt5CommandType.iOpen, Shift(0), 1.15);
            yield return F("iTime", MtTime.ToString(), c => c.iTime("EURUSD", Tf, 4), Mt5CommandType.iTime, Shift(4), Time);
            yield return F("iTickVolume", "1234", c => c.iTickVolume("EURUSD", Tf, 5), Mt5CommandType.iTickVolume, Shift(5), 1234L);
            yield return F("iRealVolume", "5000000000", c => c.iRealVolume("EURUSD", Tf, 6), Mt5CommandType.iRealVolume, Shift(6), 5000000000L);
            yield return F("iVolume", "321", c => c.iVolume("EURUSD", Tf, 7), Mt5CommandType.iVolume, Shift(7), 321L);
            yield return F("iSpread", "12", c => c.iSpread("EURUSD", Tf, 8), Mt5CommandType.iSpread, Shift(8), 12L);
        }

        [Theory]
        [MemberData(nameof(Calls))]
        public async Task Function_SendsExpectedPayload_AndConvertsReply(
            string name, string reply, Func<MtApi5Client, object> call, int command, object payload, object expected)
        {
            var (client, fake) = await Connect();
            fake.Reply(reply);

            var result = call(client);

            Assert.Equal(expected, result);
            var sent = fake.Last();
            Assert.True(command == sent.CommandType, $"{name}: command {sent.CommandType} != expected {command}");
            Assert.True(JToken.DeepEquals(JObject.FromObject(payload), JObject.Parse(sent.Payload)),
                $"{name}: payload {sent.Payload} != expected {JObject.FromObject(payload)}");
        }

        [Fact]
        public void Table_CoversEveryTimeseriesFunction()
        {
            Assert.Equal(Functions, Calls().Select(r => (string)r[0]).ToHashSet());
            foreach (var name in Functions)
                Assert.Contains(typeof(MtApi5Client).GetMethods(), m => m.Name == name);
        }

        [Fact]
        public async Task Defaults_MatchMql5()
        {
            var (client, fake) = await Connect();
            fake.Reply("0");

            client.iBarShift("EURUSD", Tf, Time);
            Assert.False((bool)fake.LastPayload()["Exact"]!);

            client.iHighest("EURUSD", Tf, ENUM_SERIESMODE.MODE_CLOSE);
            Assert.Equal(-1, (int)fake.LastPayload()["Count"]!);  // WHOLE_ARRAY
            Assert.Equal(0, (int)fake.LastPayload()["Start"]!);

            client.iLowest("EURUSD", Tf, ENUM_SERIESMODE.MODE_CLOSE);
            Assert.Equal(-1, (int)fake.LastPayload()["Count"]!);
            Assert.Equal(0, (int)fake.LastPayload()["Start"]!);
        }

        [Fact]
        public async Task NullSymbol_SendsEmptyString()
        {
            var (client, fake) = await Connect();
            fake.Reply("1.1");

            client.iClose(null!, Tf, 0);

            Assert.Equal("", (string?)fake.LastPayload()["Symbol"]);
        }

        [Theory]
        [InlineData(ENUM_SERIESMODE.MODE_OPEN, 0)]
        [InlineData(ENUM_SERIESMODE.MODE_LOW, 1)]
        [InlineData(ENUM_SERIESMODE.MODE_HIGH, 2)]
        [InlineData(ENUM_SERIESMODE.MODE_CLOSE, 3)]
        [InlineData(ENUM_SERIESMODE.MODE_VOLUME, 4)]
        [InlineData(ENUM_SERIESMODE.MODE_REAL_VOLUME, 7)]
        [InlineData(ENUM_SERIESMODE.MODE_SPREAD, 6)]
        public void SeriesMode_MatchesMql5(ENUM_SERIESMODE value, int expected) => Assert.Equal(expected, (int)value);
    }
}
