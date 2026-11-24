using MtApi5.MtProtocol;
using Xunit;
using static MtApi5.Tests.MtApi5ClientTestHelper;

namespace MtApi5.Tests
{
    // Command-level tests: each MtApi5Client method must send the right command id and JSON
    // parameters to the expert, and convert the reply (times, out-parameters, nulls) correctly.
    public class MtApi5ClientCommandTests
    {
        // ---- trading helpers ----

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task BuySell_OptionalSymbolAndComment_AreSentOnlyWhenSet(bool buy)
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":true,\"Result\":{\"Retcode\":10009}}");

            var ok = buy ? client.Buy(out var r1, 0.5) : client.Sell(out r1, 0.5);

            Assert.True(ok);
            Assert.Equal(10009u, r1!.Retcode);
            Assert.Equal(buy ? Mt5CommandType.Buy : Mt5CommandType.Sell, (Mt5CommandType)fake.Last().CommandType);
            var p = fake.LastPayload();
            Assert.Equal(0.5, (double)p["Volume"]!);
            Assert.Equal(0.0, (double)p["Price"]!);
            Assert.Null(p["Symbol"]);
            Assert.Null(p["Comment"]);

            _ = buy ? client.Buy(out _, 1, "GBPUSD", 1.3, 1.2, 1.4, "c") : client.Sell(out _, 1, "GBPUSD", 1.3, 1.2, 1.4, "c");
            p = fake.LastPayload();
            Assert.Equal("GBPUSD", (string?)p["Symbol"]);
            Assert.Equal("c", (string?)p["Comment"]);
            Assert.Equal(1.3, (double)p["Price"]!);
            Assert.Equal(1.2, (double)p["Sl"]!);
            Assert.Equal(1.4, (double)p["Tp"]!);
        }

        [Fact]
        public async Task OrderSendAsync_UsesOwnCommand()
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":true,\"Result\":{\"Retcode\":10008}}");

            Assert.True(client.OrderSendAsync(new MqlTradeRequest { Symbol = "EURUSD" }, out var result));

            Assert.Equal((int)Mt5CommandType.OrderSendAsync, fake.Last().CommandType);
            Assert.Equal(10008u, result!.Retcode);
            Assert.Equal("EURUSD", (string?)fake.LastPayload()["TradeRequest"]!["Symbol"]);
        }

        [Fact]
        public async Task OrderSend_NullValueResponse_ReturnsFalseAndNullResult()
        {
            var (client, fake) = await Connect();
            fake.Reply("null");

            Assert.False(client.OrderSend(new MqlTradeRequest(), out var result));
            Assert.Null(result);
        }

        // ---- symbol info ----

        [Fact]
        public async Task SymbolInfoTick_ConvertsTick()
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":true,\"Result\":{\"Bid\":1.1,\"Ask\":1.2,\"Time\":1700000000,\"Last\":1.15,\"Volume\":7,\"VolumeReal\":7.5,\"Flags\":6}}");

            Assert.True(client.SymbolInfoTick("EURUSD", out var tick));

            Assert.Equal("EURUSD", (string?)fake.LastPayload()["Symbol"]);
            Assert.Equal(1.1, tick!.bid);
            Assert.Equal(1.2, tick.ask);
            Assert.Equal(1.15, tick.last);
            Assert.Equal(7ul, tick.volume);
            Assert.Equal(7.5, tick.volume_real);
            Assert.Equal(ENUM_TICK_FLAGS.TICK_FLAG_BID | ENUM_TICK_FLAGS.TICK_FLAG_ASK, tick.flags);
            Assert.Equal(MtTime, tick.MtTime);
            Assert.Equal(Time, tick.time);
        }

        [Fact]
        public async Task SymbolInfoTick_Failure_ReturnsNullTick()
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":false}");

            Assert.False(client.SymbolInfoTick("XXX", out var tick));
            Assert.Null(tick);
            Assert.Null(client.SymbolInfoTick("XXX"));
        }

        [Fact]
        public async Task SymbolInfoTick_SingleArgOverload_ReturnsTick()
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":true,\"Result\":{\"Bid\":2}}");

            Assert.Equal(2, client.SymbolInfoTick("EURUSD")!.bid);
        }

        [Fact]
        public async Task CopyTicks_SendsParameters_AndMapsTicks()
        {
            var (client, fake) = await Connect();
            fake.Reply("[{\"Bid\":1,\"Ask\":2,\"Time\":1700000000,\"Last\":1.5,\"Volume\":3,\"VolumeReal\":3,\"Flags\":2},{\"Bid\":1.1}]");

            var ticks = client.CopyTicks("EURUSD", CopyTicksFlag.Info, 1000, 2);

            Assert.Equal(2, ticks!.Count);
            Assert.Equal(1, ticks[0].bid);
            Assert.Equal(Time, ticks[0].time);
            Assert.Equal(1.1, ticks[1].bid);
            var p = fake.LastPayload();
            Assert.Equal((int)Mt5CommandType.CopyTicks, fake.Last().CommandType);
            Assert.Equal("EURUSD", (string?)p["Symbol"]);
            Assert.Equal((int)CopyTicksFlag.Info, (int)p["Flags"]!);
            Assert.Equal(1000, (long)p["From"]!);
            Assert.Equal(2, (int)p["Count"]!);
        }

        [Fact]
        public async Task CopyTicks_NullResponse_ReturnsNull()
        {
            var (client, fake) = await Connect();
            fake.Reply("null");
            Assert.Null(client.CopyTicks("EURUSD"));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SymbolInfoSession_ConvertsSecondsToTimes(bool quote)
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":true,\"Result\":{\"From\":3600,\"To\":7200}}");

            var ok = quote
                ? client.SymbolInfoSessionQuote("EURUSD", ENUM_DAY_OF_WEEK.MONDAY, 1, out var from, out var to)
                : client.SymbolInfoSessionTrade("EURUSD", ENUM_DAY_OF_WEEK.MONDAY, 1, out from, out to);

            Assert.True(ok);
            Assert.Equal(new DateTime(1970, 1, 1, 1, 0, 0), from);
            Assert.Equal(new DateTime(1970, 1, 1, 2, 0, 0), to);
            var p = fake.LastPayload();
            Assert.Equal("EURUSD", (string?)p["Symbol"]);
            Assert.Equal(1, (int)p["DayOfWeek"]!);
            Assert.Equal(1, (int)p["SessionIndex"]!);
            Assert.Equal(quote ? Mt5CommandType.SymbolInfoSessionQuote : Mt5CommandType.SymbolInfoSessionTrade,
                (Mt5CommandType)fake.Last().CommandType);
        }

        [Fact]
        public async Task SymbolInfoSession_MissingKeys_ReturnsFalseAndMinValue()
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":true,\"Result\":{\"From\":3600}}");

            Assert.False(client.SymbolInfoSessionQuote("EURUSD", ENUM_DAY_OF_WEEK.MONDAY, 0, out var from, out var to));
            Assert.Equal(DateTime.MinValue, from);
            Assert.Equal(DateTime.MinValue, to);
        }

        // ---- history ----

        [Fact]
        public async Task Bars_BothOverloads()
        {
            var (client, fake) = await Connect();
            fake.Reply("500");

            Assert.Equal(500, client.Bars("EURUSD", ENUM_TIMEFRAMES.PERIOD_M5));
            Assert.Equal((int)Mt5CommandType.Bars, fake.Last().CommandType);
            Assert.Equal((int)ENUM_TIMEFRAMES.PERIOD_M5, (int)fake.LastPayload()["Timeframe"]!);

            Assert.Equal(500, client.Bars("EURUSD", ENUM_TIMEFRAMES.PERIOD_M5, Time, Time.AddHours(1)));
            Assert.Equal((int)Mt5CommandType.Bars2, fake.Last().CommandType);
            Assert.Equal(MtTime, (long)fake.LastPayload()["StartTime"]!);
            Assert.Equal(MtTime + 3600, (long)fake.LastPayload()["StopTime"]!);
        }

        [Fact]
        public async Task CopyTime_ConvertsToDateTimes()
        {
            var (client, fake) = await Connect();
            fake.Reply("[1700000000,1700003600]");

            var count = client.CopyTime("EURUSD", ENUM_TIMEFRAMES.PERIOD_H1, 0, 2, out var times);

            Assert.Equal(2, count);
            Assert.Equal([Time, Time.AddHours(1)], times);
            Assert.Equal((int)Mt5CommandType.CopyTime, fake.Last().CommandType);
        }

        [Fact]
        public async Task CopyTime_NullResponse_ReturnsZeroAndEmpty()
        {
            var (client, fake) = await Connect();
            fake.Reply("null");

            Assert.Equal(0, client.CopyTime("EURUSD", ENUM_TIMEFRAMES.PERIOD_H1, 0, 2, out var times));
            Assert.Empty(times!);
        }

        // The three overloads of every CopyXxx must pick position / time+count / time range commands.
        public static IEnumerable<object[]> CopySeries()
        {
            yield return new object[] { "High", (int)Mt5CommandType.CopyHigh, (int)Mt5CommandType.CopyHigh1, (int)Mt5CommandType.CopyHigh2 };
            yield return new object[] { "Low", (int)Mt5CommandType.CopyLow, (int)Mt5CommandType.CopyLow1, (int)Mt5CommandType.CopyLow2 };
        }

        [Theory]
        [MemberData(nameof(CopySeries))]
        public async Task CopyHighLow_Overloads_UseExpectedCommandsAndParameters(
            string series, int byPos, int byTimeCount, int byRange)
        {
            var (client, fake) = await Connect();
            fake.Reply("[1.5,1.6,1.7]");
            double[] a, b, c;
            int n1, n2, n3;
            var tf = ENUM_TIMEFRAMES.PERIOD_H1;
            if (series == "High")
            {
                n1 = client.CopyHigh("EURUSD", tf, 2, 3, out a);
                var cmd1 = fake.Last(); var p1 = fake.LastPayload();
                n2 = client.CopyHigh("EURUSD", tf, Time, 3, out b);
                var cmd2 = fake.Last(); var p2 = fake.LastPayload();
                n3 = client.CopyHigh("EURUSD", tf, Time, Time.AddDays(1), out c);
                Verify(cmd1, p1, cmd2, p2, fake.Last(), fake.LastPayload());
            }
            else
            {
                n1 = client.CopyLow("EURUSD", tf, 2, 3, out a);
                var cmd1 = fake.Last(); var p1 = fake.LastPayload();
                n2 = client.CopyLow("EURUSD", tf, Time, 3, out b);
                var cmd2 = fake.Last(); var p2 = fake.LastPayload();
                n3 = client.CopyLow("EURUSD", tf, Time, Time.AddDays(1), out c);
                Verify(cmd1, p1, cmd2, p2, fake.Last(), fake.LastPayload());
            }

            Assert.Equal([3, 3, 3], new[] { n1, n2, n3 });
            Assert.Equal([1.5, 1.6, 1.7], a);
            Assert.Equal(a, b);
            Assert.Equal(a, c);

            void Verify(FakeRpcClient.SentCommand c1, Newtonsoft.Json.Linq.JObject p1,
                        FakeRpcClient.SentCommand c2, Newtonsoft.Json.Linq.JObject p2,
                        FakeRpcClient.SentCommand c3, Newtonsoft.Json.Linq.JObject p3)
            {
                Assert.Equal(byPos, c1.CommandType);
                Assert.Equal(2, (int)p1["StartPos"]!);
                Assert.Equal(3, (int)p1["Count"]!);
                Assert.Equal(byTimeCount, c2.CommandType);
                Assert.Equal(MtTime, (long)p2["StartTime"]!);
                Assert.Equal(3, (int)p2["Count"]!);
                Assert.Equal(byRange, c3.CommandType);
                Assert.Equal(MtTime, (long)p3["StartTime"]!);
                Assert.Equal(MtTime + 86400, (long)p3["StopTime"]!);
            }
        }

        [Fact]
        public async Task CopyHigh_NullResponse_ReturnsZeroAndEmpty()
        {
            var (client, fake) = await Connect();
            fake.Reply("null");
            Assert.Equal(0, client.CopyHigh("EURUSD", ENUM_TIMEFRAMES.PERIOD_H1, 0, 5, out var arr));
            Assert.Empty(arr);
        }

        // ---- chart coordinate conversion ----

        [Fact]
        public async Task ChartTimePriceToXY_ReturnsCoordinates()
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":true,\"Result\":{\"X\":120,\"Y\":340}}");

            Assert.True(client.ChartTimePriceToXY(77, 0, Time, 1.25, out var x, out var y));

            Assert.Equal((120, 340), (x, y));
            var p = fake.LastPayload();
            Assert.Equal(77, (long)p["ChartId"]!);
            Assert.Equal(MtTime, (long)p["Time"]!);
            Assert.Equal(1.25, (double)p["Price"]!);
        }

        [Fact]
        public async Task ChartTimePriceToXY_NullTime_SendsZero_AndFailureGivesZeroes()
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":false}");

            Assert.False(client.ChartTimePriceToXY(1, 0, null, 1.0, out var x, out var y));

            Assert.Equal((0, 0), (x, y));
            Assert.Equal(0, (long)fake.LastPayload()["Time"]!);
        }

        [Fact]
        public async Task ChartXYToTimePrice_ConvertsResult()
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":true,\"Result\":{\"SubWindow\":1,\"Time\":1700000000,\"Price\":1.5}}");

            Assert.True(client.ChartXYToTimePrice(5, 10, 20, out var sub, out var time, out var price));

            Assert.Equal(1, sub);
            Assert.Equal(Time, time);
            Assert.Equal(1.5, price);
            var p = fake.LastPayload();
            Assert.Equal((5, 10, 20), ((long)p["ChartId"]!, (int)p["X"]!, (int)p["Y"]!));
        }

        [Fact]
        public async Task ChartXYToTimePrice_Failure_ReturnsDefaults()
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":false}");

            Assert.False(client.ChartXYToTimePrice(5, 1, 2, out var sub, out var time, out var price));

            Assert.Equal(0, sub);
            Assert.Null(time);
            Assert.True(double.IsNaN(price));
        }

        // ---- objects ----

        [Fact]
        public async Task ObjectCreate_SendsAdditionalCoordinatesAsTimesAndPrices()
        {
            var (client, fake) = await Connect();
            fake.Reply("true");

            var ok = client.ObjectCreate(0, "line", ENUM_OBJECT.OBJ_TREND, 0, Time, 1.0,
                [Tuple.Create(Time.AddHours(1), 2.0), Tuple.Create(Time.AddHours(2), 3.0)]);

            Assert.True(ok);
            var p = fake.LastPayload();
            Assert.Equal("line", (string?)p["Name"]);
            Assert.Equal((int)ENUM_OBJECT.OBJ_TREND, (int)p["Type"]!);
            Assert.Equal([MtTime + 3600, MtTime + 7200], p["Times"]!.Select(t => (long)t));
            Assert.Equal([2.0, 3.0], p["Prices"]!.Select(t => (double)t));
        }

        [Fact]
        public async Task ObjectCreate_WithoutAdditionalCoordinates_SendsEmptyLists()
        {
            var (client, fake) = await Connect();
            fake.Reply("true");

            client.ObjectCreate(0, "o", ENUM_OBJECT.OBJ_VLINE, 0, Time, 1.0);

            var p = fake.LastPayload();
            Assert.Empty(p["Times"]!);
            Assert.Empty(p["Prices"]!);
        }

        [Fact]
        public async Task ObjectCreate_MoreThan29AdditionalCoordinates_ThrowsWithoutSending()
        {
            var (client, fake) = await Connect();
            var before = fake.Sent.Count;
            var coords = Enumerable.Range(0, 30).Select(i => Tuple.Create(Time, (double)i)).ToList();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                client.ObjectCreate(0, "o", ENUM_OBJECT.OBJ_TREND, 0, Time, 1.0, coords));

            Assert.Equal(before, fake.Sent.Count);
        }

        [Fact]
        public async Task ObjectCreate_Exactly29AdditionalCoordinates_IsAllowed()
        {
            var (client, fake) = await Connect();
            fake.Reply("true");
            var coords = Enumerable.Range(0, 29).Select(i => Tuple.Create(Time, (double)i)).ToList();

            Assert.True(client.ObjectCreate(0, "o", ENUM_OBJECT.OBJ_TREND, 0, Time, 1.0, coords));
            Assert.Equal(29, fake.LastPayload()["Times"]!.Count());
        }

        // ---- indicators ----

        [Fact]
        public async Task iMA_SendsAllParameters_AndReturnsHandle()
        {
            var (client, fake) = await Connect();
            fake.Reply("42");

            var handle = client.iMA("EURUSD", ENUM_TIMEFRAMES.PERIOD_H1, 14, 2, ENUM_MA_METHOD.MODE_EMA, ENUM_APPLIED_PRICE.PRICE_HIGH);

            Assert.Equal(42, handle);
            Assert.Equal((int)Mt5CommandType.iMA, fake.Last().CommandType);
            var p = fake.LastPayload();
            Assert.Equal("EURUSD", (string?)p["Symbol"]);
            Assert.Equal((int)ENUM_TIMEFRAMES.PERIOD_H1, (int)p["Period"]!);
            Assert.Equal(14, (int)p["MaPeriod"]!);
            Assert.Equal(2, (int)p["MaShift"]!);
            Assert.Equal((int)ENUM_MA_METHOD.MODE_EMA, (int)p["MaMethod"]!);
            Assert.Equal((int)ENUM_APPLIED_PRICE.PRICE_HIGH, (int)p["AppliedPrice"]!);
        }

        [Fact]
        public async Task iCustom_Overloads_SendParamsAndParamsType()
        {
            var (client, fake) = await Connect();
            fake.Reply("7");

            client.iCustom("EURUSD", ENUM_TIMEFRAMES.PERIOD_M1, "Ind", new[] { 1.5, 2.5 });
            AssertCustom(fake, 1, "[1.5,2.5]");
            client.iCustom("EURUSD", ENUM_TIMEFRAMES.PERIOD_M1, "Ind", new[] { 1, 2 });
            AssertCustom(fake, 0, "[1,2]");
            client.iCustom("EURUSD", ENUM_TIMEFRAMES.PERIOD_M1, "Ind", new[] { "a", "b" });
            AssertCustom(fake, 2, "[\"a\",\"b\"]");
            client.iCustom("EURUSD", ENUM_TIMEFRAMES.PERIOD_M1, "Ind", new[] { true, false });
            AssertCustom(fake, 3, "[true,false]");

            static void AssertCustom(FakeRpcClient f, int paramsType, string paramsJson)
            {
                Assert.Equal((int)Mt5CommandType.iCustom, f.Last().CommandType);
                var p = f.LastPayload();
                Assert.Equal("Ind", (string?)p["Name"]);
                Assert.Equal(paramsType, (int)p["ParamsType"]!);
                Assert.Equal(paramsJson, p["Params"]!.ToString(Newtonsoft.Json.Formatting.None));
            }
        }

        [Fact]
        public async Task IndicatorCreate_OmitsNullSymbolAndParameters()
        {
            var (client, fake) = await Connect();
            fake.Reply("9");

            Assert.Equal(9, client.IndicatorCreate(null, ENUM_TIMEFRAMES.PERIOD_H4, ENUM_INDICATOR.IND_RSI));

            var p = fake.LastPayload();
            Assert.Null(p["Symbol"]);
            Assert.Null(p["Parameters"]);
            Assert.Equal((int)ENUM_TIMEFRAMES.PERIOD_H4, (int)p["Period"]!);
            Assert.Equal((int)ENUM_INDICATOR.IND_RSI, (int)p["IndicatorType"]!);
        }

        [Fact]
        public async Task IndicatorCreate_SendsSymbolAndParameters_WhenGiven()
        {
            var (client, fake) = await Connect();
            fake.Reply("9");

            client.IndicatorCreate("EURUSD", ENUM_TIMEFRAMES.PERIOD_H4, ENUM_INDICATOR.IND_MA, [new MqlParam { }]);

            var p = fake.LastPayload();
            Assert.Equal("EURUSD", (string?)p["Symbol"]);
            Assert.Single(p["Parameters"]!);
        }
    }
}
