using MtApi5.MtProtocol;
using Newtonsoft.Json;
using Xunit;

namespace MtApi5.Tests
{
    // JSON payloads exchanged with the MQL expert (see MtApi5Client.SendCommand and Process*Event).
    public class MtProtocolJsonTests
    {
        [Fact]
        public void Response_Success_WithValue()
        {
            var r = JsonConvert.DeserializeObject<Response<double>>("{\"ErrorCode\":0,\"Value\":1.5}");
            Assert.NotNull(r);
            Assert.Equal(0, r!.ErrorCode);
            Assert.Null(r.ErrorMessage);
            Assert.Equal(1.5, r.Value);
        }

        [Fact]
        public void Response_Error_WithMessage_NoValue()
        {
            var r = JsonConvert.DeserializeObject<Response<int?>>("{\"ErrorCode\":4106,\"ErrorMessage\":\"Unknown symbol\"}");
            Assert.NotNull(r);
            Assert.Equal(4106, r!.ErrorCode);
            Assert.Equal("Unknown symbol", r.ErrorMessage);
            Assert.Null(r.Value);
        }

        [Fact]
        public void Response_ArrayValue()
        {
            var r = JsonConvert.DeserializeObject<Response<List<int>>>("{\"ErrorCode\":0,\"Value\":[1,2,3]}");
            Assert.Equal(new[] { 1, 2, 3 }, r!.Value);
        }

        [Fact]
        public void Response_EmptyArrayValue()
        {
            var r = JsonConvert.DeserializeObject<Response<List<int>>>("{\"ErrorCode\":0,\"Value\":[]}");
            Assert.NotNull(r!.Value);
            Assert.Empty(r.Value!);
        }

        [Fact]
        public void FuncResult_RetValAndResult()
        {
            var r = JsonConvert.DeserializeObject<FuncResult<double>>("{\"RetVal\":true,\"Result\":2.25}");
            Assert.True(r!.RetVal);
            Assert.Equal(2.25, r.Result);
        }

        [Fact]
        public void FuncResult_MissingResult_IsDefault()
        {
            var r = JsonConvert.DeserializeObject<FuncResult<string>>("{\"RetVal\":false}");
            Assert.False(r!.RetVal);
            Assert.Null(r.Result);
        }

        [Fact]
        public void MtQuote_Deserializes()
        {
            const string json = "{\"Instrument\":\"EURUSD\",\"ExpertHandle\":7," +
                "\"Tick\":{\"Bid\":1.1,\"Ask\":1.2,\"Time\":1700000000,\"Last\":1.15,\"Volume\":18446744073709551615,\"VolumeReal\":3.5,\"Flags\":6}}";
            var q = JsonConvert.DeserializeObject<MtQuote>(json);
            Assert.Equal("EURUSD", q!.Instrument);
            Assert.Equal(7, q.ExpertHandle);
            Assert.NotNull(q.Tick);
            Assert.Equal(1.1, q.Tick!.Bid);
            Assert.Equal(1.2, q.Tick.Ask);
            Assert.Equal(1700000000L, q.Tick.Time);
            Assert.Equal(1.15, q.Tick.Last);
            Assert.Equal(ulong.MaxValue, q.Tick.Volume);
            Assert.Equal(3.5, q.Tick.VolumeReal);
            Assert.Equal(6u, q.Tick.Flags);
        }

        [Fact]
        public void MtQuote_WithoutTick_TickIsNull()
        {
            var q = JsonConvert.DeserializeObject<MtQuote>("{\"Instrument\":\"X\",\"ExpertHandle\":1}");
            Assert.Null(q!.Tick);
        }

        [Fact]
        public void OnBookEvent_Deserializes()
        {
            var e = JsonConvert.DeserializeObject<OnBookEvent>("{\"Symbol\":\"GBPUSD\"}");
            Assert.Equal("GBPUSD", e!.Symbol);
        }

        [Fact]
        public void OnLockTicksEvent_Deserializes()
        {
            var e = JsonConvert.DeserializeObject<OnLockTicksEvent>("{\"Instrument\":\"USDJPY\"}");
            Assert.Equal("USDJPY", e!.Instrument);
        }

        [Fact]
        public void OnLastTimeBarEvent_Deserializes()
        {
            const string json = "{\"Instrument\":\"EURUSD\",\"ExpertHandle\":3,\"Timeframe\":16385," +
                "\"Rates\":{\"mt_time\":1700000000,\"open\":1.0,\"high\":2.0,\"low\":0.5,\"close\":1.5,\"tick_volume\":100,\"spread\":2,\"real_volume\":50}}";
            var e = JsonConvert.DeserializeObject<OnLastTimeBarEvent>(json);
            Assert.Equal("EURUSD", e!.Instrument);
            Assert.Equal(3, e.ExpertHandle);
            Assert.Equal(ENUM_TIMEFRAMES.PERIOD_H1, e.Timeframe);
            var r = e.Rates!;
            Assert.Equal(1700000000L, r.mt_time);
            Assert.Equal(new DateTime(2023, 11, 14, 22, 13, 20), r.time);
            Assert.Equal(1.0, r.open);
            Assert.Equal(2.0, r.high);
            Assert.Equal(0.5, r.low);
            Assert.Equal(1.5, r.close);
            Assert.Equal(100, r.tick_volume);
            Assert.Equal(2, r.spread);
            Assert.Equal(50, r.real_volume);
        }

        [Fact]
        public void MqlRates_RoundTrip_PreservesMtTime()
        {
            var src = new MqlRates(new DateTime(2024, 5, 6, 7, 8, 9), 1, 2, 0.5, 1.5, 10, 3, 4);
            var back = JsonConvert.DeserializeObject<MqlRates>(JsonConvert.SerializeObject(src))!;
            Assert.Equal(src.mt_time, back.mt_time);
            Assert.Equal(src.time, back.time);
            Assert.Equal(src.open, back.open);
            Assert.Equal(src.spread, back.spread);
            Assert.Equal(src.real_volume, back.real_volume);
        }

        [Fact]
        public void MqlTradeRequest_RoundTrip()
        {
            var src = new MqlTradeRequest
            {
                Action = ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_PENDING,
                Magic = ulong.MaxValue,
                Order = 11,
                Symbol = "EURUSD",
                Volume = 0.1,
                Price = 1.1234,
                Stoplimit = 1.2,
                Sl = 1.0,
                Tp = 1.3,
                Deviation = 5,
                Type = ENUM_ORDER_TYPE.ORDER_TYPE_BUY_LIMIT,
                Type_filling = ENUM_ORDER_TYPE_FILLING.ORDER_FILLING_IOC,
                Type_time = ENUM_ORDER_TYPE_TIME.ORDER_TIME_SPECIFIED,
                Expiration = new DateTime(2025, 1, 2, 3, 4, 5),
                Comment = "héllo",
                Position = 22,
                PositionBy = 33
            };

            var json = JsonConvert.SerializeObject(src);
            Assert.DoesNotContain("\"Expiration\"", json); // [JsonIgnore]; MtExpiration is sent instead
            Assert.Contains("\"MtExpiration\"", json);

            var back = JsonConvert.DeserializeObject<MqlTradeRequest>(json)!;
            Assert.Equal(src.Action, back.Action);
            Assert.Equal(src.Magic, back.Magic);
            Assert.Equal(src.Order, back.Order);
            Assert.Equal(src.Symbol, back.Symbol);
            Assert.Equal(src.Volume, back.Volume);
            Assert.Equal(src.Price, back.Price);
            Assert.Equal(src.Stoplimit, back.Stoplimit);
            Assert.Equal(src.Sl, back.Sl);
            Assert.Equal(src.Tp, back.Tp);
            Assert.Equal(src.Deviation, back.Deviation);
            Assert.Equal(src.Type, back.Type);
            Assert.Equal(src.Type_filling, back.Type_filling);
            Assert.Equal(src.Type_time, back.Type_time);
            Assert.Equal(src.Comment, back.Comment);
            Assert.Equal(src.Position, back.Position);
            Assert.Equal(src.PositionBy, back.PositionBy);
        }

        [Fact]
        public void MqlTradeRequest_DefaultExpiration_SerializesAsZero()
        {
            var json = JsonConvert.SerializeObject(new MqlTradeRequest());
            Assert.Contains("\"MtExpiration\":0", json);
        }

        [Fact]
        public void MqlTradeResult_Deserializes_ThroughConstructor()
        {
            const string json = "{\"Retcode\":10009,\"Deal\":5,\"Order\":6,\"Volume\":0.1,\"Price\":1.1,\"Bid\":1.09,\"Ask\":1.11,\"Comment\":\"Done\",\"Request_id\":99}";
            var r = JsonConvert.DeserializeObject<MqlTradeResult>(json)!;
            Assert.Equal(10009u, r.Retcode);
            Assert.Equal(5ul, r.Deal);
            Assert.Equal(6ul, r.Order);
            Assert.Equal(0.1, r.Volume);
            Assert.Equal(1.1, r.Price);
            Assert.Equal(1.09, r.Bid);
            Assert.Equal(1.11, r.Ask);
            Assert.Equal("Done", r.Comment);
            Assert.Equal(99u, r.Request_id);
        }

        [Fact]
        public void MqlTradeTransaction_Deserializes()
        {
            const string json = "{\"Deal\":1,\"Order\":2,\"Symbol\":\"EURUSD\",\"Type\":2,\"Price\":1.5,\"PriceTrigger\":1.6," +
                "\"PriceSl\":1.4,\"PriceTp\":1.7,\"Volume\":0.3,\"Position\":8,\"PositionBy\":9,\"MtTimeExpiration\":1700000000}";
            var t = JsonConvert.DeserializeObject<MqlTradeTransaction>(json)!;
            Assert.Equal(1ul, t.Deal);
            Assert.Equal(2ul, t.Order);
            Assert.Equal("EURUSD", t.Symbol);
            Assert.Equal((ENUM_TRADE_TRANSACTION_TYPE)2, t.Type);
            Assert.Equal(1.5, t.Price);
            Assert.Equal(1.6, t.PriceTrigger);
            Assert.Equal(1.4, t.PriceSl);
            Assert.Equal(1.7, t.PriceTp);
            Assert.Equal(0.3, t.Volume);
            Assert.Equal(8ul, t.Position);
            Assert.Equal(9ul, t.PositionBy);
        }

        [Fact]
        public void MqlTradeRequest_RoundTrip_PreservesExpiration()
        {
            var src = new MqlTradeRequest { Expiration = new DateTime(2025, 1, 2, 3, 4, 5) };
            var back = JsonConvert.DeserializeObject<MqlTradeRequest>(JsonConvert.SerializeObject(src))!;
            Assert.Equal(src.MtExpiration, back.MtExpiration);
            Assert.Equal(src.Expiration, back.Expiration);
        }

        [Fact]
        public void MqlTradeTransaction_Deserializes_TimeExpiration()
        {
            var t = JsonConvert.DeserializeObject<MqlTradeTransaction>("{\"MtTimeExpiration\":1700000000}")!;
            Assert.Equal(1700000000L, t.MtTimeExpiration);
            Assert.Equal(new DateTime(2023, 11, 14, 22, 13, 20), t.TimeExpiration);
        }

        [Fact]
        public void OnTradeTransactionEvent_Deserializes_NestedObjects()
        {
            const string json = "{\"Trans\":{\"Deal\":1,\"Symbol\":\"EURUSD\"}," +
                "\"Request\":{\"Symbol\":\"EURUSD\",\"Volume\":0.2}," +
                "\"Result\":{\"Retcode\":10009,\"Comment\":\"ok\"}}";
            var e = JsonConvert.DeserializeObject<OnTradeTransactionEvent>(json)!;
            Assert.Equal(1ul, e.Trans!.Deal);
            Assert.Equal(0.2, e.Request!.Volume);
            Assert.Equal(10009u, e.Result!.Retcode);
            Assert.Equal("ok", e.Result.Comment);
        }

        [Fact]
        public void OnTradeTransactionEvent_MissingParts_AreNull()
        {
            var e = JsonConvert.DeserializeObject<OnTradeTransactionEvent>("{\"Trans\":{\"Deal\":1}}")!;
            Assert.NotNull(e.Trans);
            Assert.Null(e.Request);
            Assert.Null(e.Result);
        }

        [Fact]
        public void Mt5EventTypes_Values_MatchExpert()
        {
            Assert.Equal(1, (int)Mt5EventTypes.OnTradeTransaction);
            Assert.Equal(2, (int)Mt5EventTypes.OnBookEvent);
            Assert.Equal(3, (int)Mt5EventTypes.OnTick);
            Assert.Equal(4, (int)Mt5EventTypes.OnLastTimeBar);
            Assert.Equal(5, (int)Mt5EventTypes.OnLockTicks);
        }

        [Fact]
        public void CustomRequest_ParametersType_Values()
        {
            Assert.Equal(0, (int)MtApi5.MtProtocol.ICustomRequest.ParametersType.Int);
            Assert.Equal(1, (int)MtApi5.MtProtocol.ICustomRequest.ParametersType.Double);
            Assert.Equal(2, (int)MtApi5.MtProtocol.ICustomRequest.ParametersType.String);
            Assert.Equal(3, (int)MtApi5.MtProtocol.ICustomRequest.ParametersType.Boolean);
        }
    }
}
