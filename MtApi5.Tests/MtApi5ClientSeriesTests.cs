using System.Reflection;
using MtApi5.MtProtocol;
using Newtonsoft.Json.Linq;
using Xunit;
using static MtApi5.Tests.MtApi5ClientTestHelper;

namespace MtApi5.Tests
{
    // Every CopyXxx series has three overloads (by position / by start time + count / by time range)
    // that must use commands CopyXxx, CopyXxx1, CopyXxx2. The overloads are found by reflection so
    // one test checks the whole family and its naming convention.
    public class MtApi5ClientSeriesTests
    {
        public static IEnumerable<object[]> Series()
        {
            yield return new object[] { "CopyOpen", "[1.5,1.6,1.7]" };
            yield return new object[] { "CopyClose", "[1.5,1.6,1.7]" };
            yield return new object[] { "CopyTickVolume", "[10,20,30]" };
            yield return new object[] { "CopyRealVolume", "[10,20,30]" };
            yield return new object[] { "CopySpread", "[1,2,3]" };
            yield return new object[] { "CopyTime", "[1700000000,1700003600,1700007200]" };
            yield return new object[] { "CopyRates",
                "[{\"mt_time\":1700000000},{\"mt_time\":1700003600},{\"mt_time\":1700007200}]" };
        }

        private static MethodInfo Overload(string name, Type third, Type fourth) =>
            typeof(MtApi5Client).GetMethods().Single(m => m.Name == name
                && m.GetParameters().Length == 5
                && m.GetParameters()[2].ParameterType == third
                && m.GetParameters()[3].ParameterType == fourth);

        private static (int Result, Array Output) Call(MtApi5Client client, MethodInfo m, object third, object fourth)
        {
            var args = new object?[] { "EURUSD", ENUM_TIMEFRAMES.PERIOD_H1, third, fourth, null };
            var result = (int)m.Invoke(client, args)!;
            return (result, (Array)args[4]!);
        }

        [Theory]
        [MemberData(nameof(Series))]
        public async Task Overloads_UseCommandFamily_AndParameters(string method, string reply)
        {
            var (client, fake) = await Connect();
            fake.Reply(reply);

            var byPos = Call(client, Overload(method, typeof(int), typeof(int)), 2, 3);
            var c1 = fake.Last(); var p1 = fake.LastPayload();
            var byCount = Call(client, Overload(method, typeof(DateTime), typeof(int)), Time, 3);
            var c2 = fake.Last(); var p2 = fake.LastPayload();
            var byRange = Call(client, Overload(method, typeof(DateTime), typeof(DateTime)), Time, Time.AddDays(1));
            var c3 = fake.Last(); var p3 = fake.LastPayload();

            Assert.Equal(Enum.Parse<Mt5CommandType>(method), (Mt5CommandType)c1.CommandType);
            Assert.Equal(Enum.Parse<Mt5CommandType>(method + "1"), (Mt5CommandType)c2.CommandType);
            Assert.Equal(Enum.Parse<Mt5CommandType>(method + "2"), (Mt5CommandType)c3.CommandType);

            Assert.Equal("EURUSD", (string?)p1["Symbol"]);
            Assert.Equal((int)ENUM_TIMEFRAMES.PERIOD_H1, (int)p1["Timeframe"]!);
            Assert.Equal(2, (int)p1["StartPos"]!);
            Assert.Equal(3, (int)p1["Count"]!);
            Assert.Equal(MtTime, (long)p2["StartTime"]!);
            Assert.Equal(3, (int)p2["Count"]!);
            Assert.Equal(MtTime, (long)p3["StartTime"]!);
            Assert.Equal(MtTime + 86400, (long)p3["StopTime"]!);

            foreach (var r in new[] { byPos, byCount, byRange })
            {
                Assert.Equal(3, r.Result);
                Assert.Equal(3, r.Output.Length);
            }
        }

        [Theory]
        [MemberData(nameof(Series))]
        public async Task Overloads_NullReply_ReturnZeroAndEmptyArray(string method, string _)
        {
            var (client, fake) = await Connect();
            fake.Reply("null");

            foreach (var (m, a, b) in new (MethodInfo, object, object)[]
            {
                (Overload(method, typeof(int), typeof(int)), 0, 1),
                (Overload(method, typeof(DateTime), typeof(int)), Time, 1),
                (Overload(method, typeof(DateTime), typeof(DateTime)), Time, Time.AddHours(1)),
            })
            {
                var r = Call(client, m, a, b);
                Assert.Equal(0, r.Result);
                Assert.Empty(r.Output);
            }
        }

        [Fact]
        public async Task CopyTime_AllOverloads_ConvertValues()
        {
            var (client, fake) = await Connect();
            fake.Reply("[1700000000,1700003600]");

            client.CopyTime("EURUSD", ENUM_TIMEFRAMES.PERIOD_H1, Time, 2, out var a);
            client.CopyTime("EURUSD", ENUM_TIMEFRAMES.PERIOD_H1, Time, Time.AddHours(1), out var b);

            Assert.Equal([Time, Time.AddHours(1)], a);
            Assert.Equal(a, b);
        }

        [Fact]
        public async Task CopyRates_AllOverloads_MapValues()
        {
            var (client, fake) = await Connect();
            fake.Reply("[{\"mt_time\":1700000000,\"open\":1,\"close\":2,\"tick_volume\":5}]");

            client.CopyRates("EURUSD", ENUM_TIMEFRAMES.PERIOD_H1, Time, 1, out var a);
            client.CopyRates("EURUSD", ENUM_TIMEFRAMES.PERIOD_H1, Time, Time.AddHours(1), out var b);

            foreach (var rates in new[] { a!, b! })
            {
                Assert.Single(rates);
                Assert.Equal(Time, rates[0].time);
                Assert.Equal(1, rates[0].open);
                Assert.Equal(2, rates[0].close);
                Assert.Equal(5, rates[0].tick_volume);
            }
        }

        [Fact]
        public async Task CopyTickVolume_ReturnsLongValues()
        {
            var (client, fake) = await Connect();
            fake.Reply("[9223372036854775807,1]");

            client.CopyTickVolume("EURUSD", ENUM_TIMEFRAMES.PERIOD_H1, 0, 2, out var v);

            Assert.Equal([long.MaxValue, 1L], v);
        }

        // ---- CopyBuffer ----

        [Fact]
        public async Task CopyBuffer_Overloads_UseCommandFamily()
        {
            var (client, fake) = await Connect();
            fake.Reply("[0.5,0.6]");

            Assert.Equal(2, client.CopyBuffer(11, 1, 0, 2, out var a));
            var c1 = fake.Last(); var p1 = fake.LastPayload();
            Assert.Equal(2, client.CopyBuffer(11, 1, Time, 2, out var b));
            var c2 = fake.Last(); var p2 = fake.LastPayload();
            Assert.Equal(2, client.CopyBuffer(11, 1, Time, Time.AddHours(1), out var c));
            var c3 = fake.Last(); var p3 = fake.LastPayload();

            Assert.Equal((int)Mt5CommandType.CopyBuffer, c1.CommandType);
            Assert.Equal((int)Mt5CommandType.CopyBuffer1, c2.CommandType);
            Assert.Equal((int)Mt5CommandType.CopyBuffer2, c3.CommandType);
            Assert.Equal((11, 1, 0, 2), ((int)p1["IndicatorHandle"]!, (int)p1["BufferNum"]!, (int)p1["StartPos"]!, (int)p1["Count"]!));
            Assert.Equal((MtTime, 2), ((long)p2["StartTime"]!, (int)p2["Count"]!));
            Assert.Equal((MtTime, MtTime + 3600), ((long)p3["StartTime"]!, (long)p3["StopTime"]!));
            Assert.Equal([0.5, 0.6], a);
            Assert.Equal(a, b);
            Assert.Equal(a, c);
        }

        [Fact]
        public async Task CopyBuffer_NullReply_ReturnsZeroAndEmpty()
        {
            var (client, fake) = await Connect();
            fake.Reply("null");

            Assert.Equal(0, client.CopyBuffer(1, 0, 0, 5, out var buf));
            Assert.Empty(buf);
        }
    }
}
