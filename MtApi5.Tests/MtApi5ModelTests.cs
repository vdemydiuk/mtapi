using MtApi5.MtProtocol;
using Newtonsoft.Json;
using Xunit;

namespace MtApi5.Tests
{
    // Data classes that are (de)serialized to/from the expert or exposed to library users.
    public class MtApi5ModelTests
    {
        [Fact]
        public void MqlParam_RoundTrip()
        {
            var src = new MqlParam { DataType = ENUM_DATATYPE.TYPE_STRING, IntegerValue = long.MaxValue, DoubleValue = 1.5, StringValue = "héllo" };
            var back = JsonConvert.DeserializeObject<MqlParam>(JsonConvert.SerializeObject(src))!;
            Assert.Equal(src.DataType, back.DataType);
            Assert.Equal(src.IntegerValue, back.IntegerValue);
            Assert.Equal(src.DoubleValue, back.DoubleValue);
            Assert.Equal(src.StringValue, back.StringValue);
        }

        [Fact]
        public void MqlParam_Defaults()
        {
            var p = new MqlParam();
            Assert.Equal(0, p.IntegerValue);
            Assert.Equal(0.0, p.DoubleValue);
            Assert.Equal("", p.StringValue);
        }

        [Fact]
        public void MqlParam_SerializedPropertyNames_AreWhatTheExpertReads()
        {
            var json = JsonConvert.SerializeObject(new MqlParam());
            Assert.Contains("\"DataType\"", json);
            Assert.Contains("\"IntegerValue\"", json);
            Assert.Contains("\"DoubleValue\"", json);
            Assert.Contains("\"StringValue\"", json);
        }

        [Fact]
        public void MqlBookInfo_Deserializes_AndConstructorSetsFields()
        {
            var b = JsonConvert.DeserializeObject<MqlBookInfo>("{\"type\":2,\"price\":1.1,\"volume\":10,\"volume_real\":10.5}")!;
            Assert.Equal(ENUM_BOOK_TYPE.BOOK_TYPE_BUY, b.type);
            Assert.Equal(1.1, b.price);
            Assert.Equal(10, b.volume);
            Assert.Equal(10.5, b.volume_real);

            var c = new MqlBookInfo(ENUM_BOOK_TYPE.BOOK_TYPE_SELL, 2.5, 7);
            Assert.Equal("type = BOOK_TYPE_SELL; price = 2.5; volume = 7", c.ToString());
        }

        [Fact]
        public void MqlTradeCheckResult_Deserializes_AndConstructorSetsFields()
        {
            const string json = "{\"Retcode\":10009,\"Balance\":1,\"Equity\":2,\"Profit\":3,\"Margin\":4,\"Margin_free\":5,\"Margin_level\":6,\"Comment\":\"ok\"}";
            var r = JsonConvert.DeserializeObject<MqlTradeCheckResult>(json)!;
            Assert.Equal(10009u, r.Retcode);
            Assert.Equal([1.0, 2.0, 3.0, 4.0, 5.0, 6.0], new[] { r.Balance, r.Equity, r.Profit, r.Margin, r.Margin_free, r.Margin_level });
            Assert.Equal("ok", r.Comment);

            var c = new MqlTradeCheckResult(1, 2, 3, 4, 5, 6, 7, "c");
            Assert.Equal("Retcode=1; Comment=c; Balance=2; Equity=3; Profit=4; Margin=5; Margin_free=6; Margin_level=7", c.ToString());
        }

        [Fact]
        public void MqlTradeResult_CopyConstructor_CopiesAllFields()
        {
            var src = new MqlTradeResult(10009, 1, 2, 0.1, 1.1, 1.09, 1.11, "Done", 99);
            var copy = new MqlTradeResult(src);
            Assert.Equal(src.ToString(), copy.ToString());
            Assert.Equal(99u, copy.Request_id);
        }

        [Fact]
        public void MqlRates_Constructors_ConvertTime()
        {
            var r = new MqlRates(MtApi5ClientTestHelper.Time, 1, 2, 0.5, 1.5, 10, 3, 4);
            Assert.Equal(MtApi5ClientTestHelper.MtTime, r.mt_time);
            Assert.Equal(MtApi5ClientTestHelper.Time, r.time);
            Assert.Equal((1.0, 2.0, 0.5, 1.5, 10L, 3, 4L), (r.open, r.high, r.low, r.close, r.tick_volume, r.spread, r.real_volume));

            var internalCtor = new MqlRates(MtApi5ClientTestHelper.MtTime, 1, 2, 0.5, 1.5, 10, 3, 4);
            Assert.Equal(r.time, internalCtor.time);
        }

        [Fact]
        public void MqlTick_PublicConstructors_ConvertTime_AndFlags()
        {
            var t = new MqlTick(MtApi5ClientTestHelper.Time, 1.1, 1.2, 1.15, 7, ENUM_TICK_FLAGS.TICK_FLAG_BID);
            Assert.Equal(MtApi5ClientTestHelper.MtTime, t.MtTime);
            Assert.Equal(MtApi5ClientTestHelper.Time, t.time);
            Assert.Equal(ENUM_TICK_FLAGS.TICK_FLAG_BID, t.flags);
            Assert.Equal(ENUM_TICK_FLAGS.TICK_FLAG_NONE, new MqlTick(MtApi5ClientTestHelper.Time, 1, 2, 3, 4).flags);
        }

        [Fact]
        public void MqlTick_FromMtTick_CopiesAllFields_AndNullGivesDefaults()
        {
            var t = new MqlTick(new MtTick { Bid = 1, Ask = 2, Time = 1700000000, Last = 3, Volume = 4, VolumeReal = 5.5, Flags = 6 });
            Assert.Equal((1.0, 2.0, 3.0, 4ul, 5.5), (t.bid, t.ask, t.last, t.volume, t.volume_real));
            Assert.Equal(ENUM_TICK_FLAGS.TICK_FLAG_BID | ENUM_TICK_FLAGS.TICK_FLAG_ASK, t.flags);
            Assert.Equal(MtApi5ClientTestHelper.Time, t.time);

            var empty = new MqlTick((MtTick?)null);
            Assert.Equal(0, empty.MtTime);
            Assert.Equal(0.0, empty.bid);
        }

        [Fact]
        public void MqlTradeRequest_ToString_ListsFields()
        {
            var s = new MqlTradeRequest { Symbol = "EURUSD", Volume = 0.1, Comment = "c" }.ToString();
            Assert.Contains("Symbol=EURUSD", s);
            Assert.Contains("Volume=0.1", s);
            Assert.Contains("Comment=c", s);
        }

        [Fact]
        public void CopyTicksFlag_Values_MatchMql5()
        {
            Assert.Equal(1, (int)CopyTicksFlag.Info);
            Assert.Equal(2, (int)CopyTicksFlag.Trade);
            Assert.Equal(-1, (int)CopyTicksFlag.All);
        }

        [Fact]
        public void Mt5Quote_Defaults()
        {
            var q = new Mt5Quote();
            Assert.Equal("", q.Instrument);
            Assert.Equal(0, q.ExpertHandle);
        }

        [Fact]
        public void Mt5ConnectionState_HasExpectedStates()
        {
            Assert.True(Enum.IsDefined(Mt5ConnectionState.Disconnected));
            Assert.True(Enum.IsDefined(Mt5ConnectionState.Connecting));
            Assert.True(Enum.IsDefined(Mt5ConnectionState.Connected));
            Assert.True(Enum.IsDefined(Mt5ConnectionState.Failed));
        }

        [Fact]
        public void EventArgs_ExposeConstructorValues()
        {
            var quotes = new List<Mt5Quote> { new() { Instrument = "A" } };
            Assert.Same(quotes, new Mt5QuotesEventArgs(quotes).Quotes);
            var q = new Mt5Quote();
            Assert.Same(q, new Mt5QuoteEventArgs(q).Quote);
            var conn = new Mt5ConnectionEventArgs(Mt5ConnectionState.Failed, "msg");
            Assert.Equal((Mt5ConnectionState.Failed, "msg"), (conn.Status, conn.ConnectionMessage));
            var bar = new Mt5TimeBarArgs(3, "EURUSD", ENUM_TIMEFRAMES.PERIOD_M5, new MqlRates());
            Assert.Equal((3, "EURUSD", ENUM_TIMEFRAMES.PERIOD_M5), (bar.ExpertHandle, bar.Symbol, bar.Timeframe));
            Assert.Equal("EURUSD", new Mt5LockTicksEventArgs("EURUSD").Symbol);
        }

        [Fact]
        public void ExecutionException_ExposesErrorCodeAndMessage()
        {
            var ex = new ExecutionException(ErrorCode.ErrTradeDisabled, "disabled");
            Assert.Equal(ErrorCode.ErrTradeDisabled, ex.ErrorCode);
            Assert.Equal("disabled", ex.Message);
        }
    }
}
