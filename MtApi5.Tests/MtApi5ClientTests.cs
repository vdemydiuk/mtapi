using MtApi5.MtProtocol;
using MtClient;
using MtService;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MtApi5.Tests
{
    public class MtApi5ClientTests
    {
        private const string QuoteJson =
            "{\"Instrument\":\"EURUSD\",\"ExpertHandle\":1,\"Tick\":{\"Bid\":1.1,\"Ask\":1.2,\"Time\":1700000000,\"Last\":1.15,\"Volume\":7,\"VolumeReal\":0,\"Flags\":0}}";

        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        // Default fake: answers GetQuote with EURUSD and IsTesting with false.
        private static FakeRpcClient NewFake(params int[] experts)
        {
            var fake = new FakeRpcClient { Experts = experts.Length == 0 ? [1] : [.. experts] };
            fake.Handler = (_, type, _) => type switch
            {
                (int)Mt5CommandType.GetQuote => FakeRpcClient.Ok(QuoteJson),
                (int)Mt5CommandType.IsTesting => FakeRpcClient.Ok("false"),
                _ => FakeRpcClient.Ok("null")
            };
            return fake;
        }

        private static MtApi5Client NewClient(FakeRpcClient fake) => new((_, _, _) => fake);

        private static async Task<MtApi5Client> ConnectedClient(FakeRpcClient fake)
        {
            var client = NewClient(fake);
            await client.Connect("localhost", 8222);
            return client;
        }

        private static Task<T> Next<T>(Action<Action<T>> subscribe)
        {
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            subscribe(v => tcs.TrySetResult(v));
            return tcs.Task.WaitAsync(Timeout);
        }

        // ---- connection lifecycle ----

        [Fact]
        public async Task Connect_Success_RaisesStatesAndQuoteList_AndSelectsExecutor()
        {
            var fake = NewFake(5, 6);
            var client = NewClient(fake);
            var states = new List<Mt5ConnectionState>();
            client.ConnectionStateChanged += (_, e) => states.Add(e.Status);
            IList<Mt5Quote>? quotes = null;
            client.QuoteList += (_, e) => quotes = e.Quotes.ToList();

            await client.Connect("localhost", 8222);

            Assert.Equal([Mt5ConnectionState.Connecting, Mt5ConnectionState.Connected], states);
            Assert.NotNull(quotes);
            Assert.Equal(2, quotes!.Count);
            Assert.Equal("EURUSD", quotes[0].Instrument);
            Assert.Contains(client.ExecutorHandle, new[] { 5, 6 });
            Assert.Equal(1, fake.ConnectCalls);
        }

        [Fact]
        public async Task Connect_NullOrEmptyHost_Throws()
        {
            var client = NewClient(NewFake());
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.Connect("", 8222));
        }

        [Fact]
        public async Task Connect_WhenAlreadyConnected_DoesNothing()
        {
            var fake = NewFake();
            var created = 0;
            var client = new MtApi5Client((_, _, _) => { created++; return fake; });
            await client.Connect("localhost", 8222);
            await client.Connect("localhost", 8222);
            Assert.Equal(1, created);
        }

        [Fact]
        public async Task Connect_EmptyExpertList_FailsAndDisconnects()
        {
            var fake = NewFake();
            fake.Experts = [];
            var client = NewClient(fake);
            var states = new List<Mt5ConnectionState>();
            client.ConnectionStateChanged += (_, e) => states.Add(e.Status);

            await Assert.ThrowsAsync<Exception>(() => client.Connect("localhost", 8222));

            Assert.Equal(Mt5ConnectionState.Failed, states.Last());
            Assert.True(fake.DisconnectCalls >= 1);
        }

        [Fact]
        public async Task Connect_TransportError_FailsAndCanRetry()
        {
            var fake = NewFake();
            fake.ConnectException = new InvalidOperationException("refused");
            var client = NewClient(fake);
            var states = new List<Mt5ConnectionState>();
            client.ConnectionStateChanged += (_, e) => states.Add(e.Status);

            var ex = await Assert.ThrowsAsync<Exception>(() => client.Connect("localhost", 8222));
            Assert.Contains("refused", ex.Message);
            Assert.Equal(Mt5ConnectionState.Failed, states.Last());

            fake.ConnectException = null;
            await client.Connect("localhost", 8222);
            Assert.Equal(Mt5ConnectionState.Connected, states.Last());
        }

        [Fact]
        public async Task Disconnect_RaisesDisconnected_Once_AndClosesTransport()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            var states = new List<Mt5ConnectionState>();
            client.ConnectionStateChanged += (_, e) => states.Add(e.Status);

            client.Disconnect();
            client.Disconnect();

            Assert.Equal([Mt5ConnectionState.Disconnected], states);
            Assert.Equal(1, fake.DisconnectCalls);
            Assert.Equal(0, client.ExecutorHandle);
        }

        [Fact]
        public async Task TransportConnectionFailed_SetsFailedState()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            var next = Next<Mt5ConnectionEventArgs>(h => client.ConnectionStateChanged += (_, e) => h(e));

            fake.RaiseConnectionFailed();

            Assert.Equal(Mt5ConnectionState.Failed, (await next).Status);
        }

        [Fact]
        public async Task TransportDisconnected_SetsDisconnectedState()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            var next = Next<Mt5ConnectionEventArgs>(h => client.ConnectionStateChanged += (_, e) => h(e));

            fake.RaiseDisconnected();

            Assert.Equal(Mt5ConnectionState.Disconnected, (await next).Status);
        }

        // ---- commands ----

        [Fact]
        public void Command_WithoutConnection_Throws()
        {
            var client = NewClient(NewFake());
            var ex = Assert.Throws<Exception>(() => client.PositionsTotal());
            Assert.Equal("No connection", ex.Message);
        }

        [Fact]
        public void GetQuotes_WithoutConnection_Throws()
        {
            Assert.Throws<Exception>(() => NewClient(NewFake()).GetQuotes());
        }

        [Fact]
        public void CommandTimeout_DefaultAndValidation()
        {
            var client = NewClient(NewFake());
            Assert.Equal(30000, client.CommandTimeout);
            Assert.Throws<ArgumentException>(() => client.CommandTimeout = 0);
            Assert.Throws<ArgumentException>(() => client.CommandTimeout = -1);
            client.CommandTimeout = 1234;
            Assert.Equal(1234, client.CommandTimeout);
        }

        [Fact]
        public async Task PositionsTotal_SendsToExecutor_WithCommandTimeout_AndParsesValue()
        {
            var fake = NewFake(7);
            var client = await ConnectedClient(fake);
            client.CommandTimeout = 4321;
            fake.Handler = (_, _, _) => FakeRpcClient.Ok("3");

            Assert.Equal(3, client.PositionsTotal());

            var sent = fake.Sent.Last();
            Assert.Equal(7, sent.ExpertHandle);
            Assert.Equal((int)Mt5CommandType.PositionsTotal, sent.CommandType);
            Assert.Equal("", sent.Payload);
            Assert.Equal(4321, sent.Timeout);
        }

        [Fact]
        public async Task ExecutorHandle_CanBeChanged_AndIsUsedForCommands()
        {
            var fake = NewFake(1, 2);
            var client = await ConnectedClient(fake);
            fake.Handler = (_, _, _) => FakeRpcClient.Ok("0");
            client.ExecutorHandle = 2;

            client.PositionsTotal();

            Assert.Equal(2, fake.Sent.Last().ExpertHandle);
        }

        [Fact]
        public async Task AccountInfoDouble_SendsPropertyId()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            fake.Handler = (_, _, _) => FakeRpcClient.Ok("1234.5");

            var v = client.AccountInfoDouble(ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_BALANCE);

            Assert.Equal(1234.5, v);
            var sent = fake.Sent.Last();
            Assert.Equal((int)Mt5CommandType.AccountInfoDouble, sent.CommandType);
            Assert.Equal((int)ENUM_ACCOUNT_INFO_DOUBLE.ACCOUNT_BALANCE, (int)JObject.Parse(sent.Payload)["PropertyId"]!);
        }

        [Fact]
        public async Task CopyRates_SendsParameters_AndReturnsRates()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            fake.Handler = (_, _, _) => FakeRpcClient.Ok(
                "[{\"mt_time\":1700000000,\"open\":1,\"high\":2,\"low\":0.5,\"close\":1.5,\"tick_volume\":10,\"spread\":1,\"real_volume\":2}," +
                "{\"mt_time\":1700003600,\"open\":1.5,\"high\":2,\"low\":1,\"close\":1.8,\"tick_volume\":11,\"spread\":1,\"real_volume\":3}]");

            var count = client.CopyRates("EURUSD", ENUM_TIMEFRAMES.PERIOD_H1, 0, 2, out var rates);

            Assert.Equal(2, count);
            Assert.Equal(2, rates!.Length);
            Assert.Equal(1.8, rates[1].close);
            var p = JObject.Parse(fake.Sent.Last().Payload);
            Assert.Equal("EURUSD", (string?)p["Symbol"]);
            Assert.Equal((int)ENUM_TIMEFRAMES.PERIOD_H1, (int)p["Timeframe"]!);
            Assert.Equal(0, (int)p["StartPos"]!);
            Assert.Equal(2, (int)p["Count"]!);
        }

        [Fact]
        public async Task CopyRates_NullValue_ReturnsZeroAndEmptyArray()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            fake.Handler = (_, _, _) => FakeRpcClient.Ok("null");

            Assert.Equal(0, client.CopyRates("EURUSD", ENUM_TIMEFRAMES.PERIOD_M1, 0, 5, out var rates));
            Assert.Empty(rates!);
        }

        [Fact]
        public async Task OrderSend_SendsRequest_AndReturnsResult()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            fake.Handler = (_, _, _) => FakeRpcClient.Ok("{\"RetVal\":true,\"Result\":{\"Retcode\":10009,\"Deal\":5,\"Order\":6,\"Comment\":\"Done\"}}");
            var request = new MqlTradeRequest
            {
                Action = ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_DEAL,
                Symbol = "EURUSD",
                Volume = 0.1,
                Type = ENUM_ORDER_TYPE.ORDER_TYPE_BUY
            };

            var ok = client.OrderSend(request, out var result);

            Assert.True(ok);
            Assert.Equal(10009u, result!.Retcode);
            Assert.Equal("Done", result.Comment);
            var sent = fake.Sent.Last();
            Assert.Equal((int)Mt5CommandType.OrderSend, sent.CommandType);
            var tr = JObject.Parse(sent.Payload)["TradeRequest"]!;
            Assert.Equal("EURUSD", (string?)tr["Symbol"]);
            Assert.Equal(0.1, (double)tr["Volume"]!);
            Assert.Equal((int)ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_DEAL, (int)tr["Action"]!);
        }

        [Fact]
        public async Task OrderSend_RetValFalse_ReturnsFalse()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            fake.Handler = (_, _, _) => FakeRpcClient.Ok("{\"RetVal\":false,\"Result\":{\"Retcode\":10013}}");

            Assert.False(client.OrderSend(new MqlTradeRequest(), out var result));
            Assert.Equal(10013u, result!.Retcode);
        }

        [Fact]
        public async Task ErrorResponse_ThrowsExecutionException_WithCodeAndMessage()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            fake.Handler = (_, _, _) => "{\"ErrorCode\":4301,\"ErrorMessage\":\"Unknown symbol\"}";

            var ex = Assert.Throws<ExecutionException>(() => client.PositionsTotal());

            Assert.Equal(ErrorCode.ErrMarketUnknownSymbol, ex.ErrorCode);
            Assert.Equal("Unknown symbol", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task EmptyResponse_ThrowsExecutionException_Custom(string? response)
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            fake.Handler = (_, _, _) => response;

            var ex = Assert.Throws<ExecutionException>(() => client.PositionsTotal());

            Assert.Equal(ErrorCode.ErrCustom, ex.ErrorCode);
        }

        [Fact]
        public async Task DeliveryFailure_ThrowsExecutionException_Custom()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            fake.Handler = (_, _, _) => throw new MtRpcException(ResponseStatus.ExpertNotFound, "expert 9 not found");

            var ex = Assert.Throws<ExecutionException>(() => client.PositionsTotal());

            Assert.Equal(ErrorCode.ErrCustom, ex.ErrorCode);
            Assert.Equal("expert 9 not found", ex.Message);
        }

        // ---- experts and events ----

        [Fact]
        public async Task ExpertAdded_RaisesQuoteAdded()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            var next = Next<Mt5QuoteEventArgs>(h => client.QuoteAdded += (_, e) => h(e));

            fake.RaiseExpertAdded(2);

            var e = await next;
            Assert.Equal("EURUSD", e.Quote.Instrument);
            Assert.Equal(2, e.Quote.ExpertHandle);
            Assert.Equal(1.1, e.Quote.Bid);
            Assert.Equal(1.2, e.Quote.Ask);
        }

        [Fact]
        public async Task ExpertRemoved_RaisesQuoteRemoved_AndMovesExecutor()
        {
            var fake = NewFake(1, 2);
            var client = await ConnectedClient(fake);
            client.ExecutorHandle = 1;
            var next = Next<Mt5QuoteEventArgs>(h => client.QuoteRemoved += (_, e) => h(e));

            fake.RaiseExpertRemoved(1);

            var e = await next;
            Assert.Equal(1, e.Quote.ExpertHandle);
            Assert.Equal("EURUSD", e.Quote.Instrument);
            Assert.Equal(2, client.ExecutorHandle);
        }

        [Fact]
        public async Task TickEvent_RaisesQuoteUpdate_AndQuoteUpdated()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            var update = Next<Mt5QuoteEventArgs>(h => client.QuoteUpdate += (_, e) => h(e));
            var updated = Next<(string, double, double)>(h => client.QuoteUpdated += (_, s, b, a) => h((s, b, a)));

            fake.RaiseEvent(1, (int)Mt5EventTypes.OnTick, QuoteJson);

            var q = (await update).Quote;
            Assert.Equal("EURUSD", q.Instrument);
            Assert.Equal(1.1, q.Bid);
            Assert.Equal(1.2, q.Ask);
            Assert.Equal(1.15, q.Last);
            Assert.Equal(7ul, q.Volume);
            Assert.Equal(new DateTime(2023, 11, 14, 22, 13, 20), q.Time);
            Assert.Equal(("EURUSD", 1.1, 1.2), await updated);
        }

        [Fact]
        public async Task TradeTransactionEvent_IsForwarded()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            var next = Next<Mt5TradeTransactionEventArgs>(h => client.OnTradeTransaction += (_, e) => h(e));

            fake.RaiseEvent(3, (int)Mt5EventTypes.OnTradeTransaction,
                "{\"Trans\":{\"Deal\":9,\"Symbol\":\"EURUSD\",\"MtTimeExpiration\":1700000000},\"Result\":{\"Retcode\":10009}}");

            var e = await next;
            Assert.Equal(3, e.ExpertHandle);
            Assert.Equal(9ul, e.Trans!.Deal);
            Assert.Equal(new DateTime(2023, 11, 14, 22, 13, 20), e.Trans.TimeExpiration);
            Assert.Equal(10009u, e.Result!.Retcode);
            Assert.Null(e.Request);
        }

        [Fact]
        public async Task BookEvent_IsForwarded()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            var next = Next<Mt5BookEventArgs>(h => client.OnBookEvent += (_, e) => h(e));

            fake.RaiseEvent(4, (int)Mt5EventTypes.OnBookEvent, "{\"Symbol\":\"GBPUSD\"}");

            var e = await next;
            Assert.Equal(4, e.ExpertHandle);
            Assert.Equal("GBPUSD", e.Symbol);
        }

        [Fact]
        public async Task LastTimeBarEvent_IsForwarded()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            var next = Next<Mt5TimeBarArgs>(h => client.OnLastTimeBar += (_, e) => h(e));

            fake.RaiseEvent(1, (int)Mt5EventTypes.OnLastTimeBar,
                "{\"Instrument\":\"EURUSD\",\"Timeframe\":16385,\"Rates\":{\"mt_time\":1700000000,\"close\":1.5}}");

            var e = await next;
            Assert.Equal("EURUSD", e.Symbol);
            Assert.Equal(ENUM_TIMEFRAMES.PERIOD_H1, e.Timeframe);
            Assert.Equal(1.5, e.Rates.close);
        }

        [Fact]
        public async Task LockTicksEvent_IsForwarded()
        {
            var fake = NewFake();
            var client = await ConnectedClient(fake);
            var next = Next<Mt5LockTicksEventArgs>(h => client.OnLockTicks += (_, e) => h(e));

            fake.RaiseEvent(1, (int)Mt5EventTypes.OnLockTicks, "{\"Instrument\":\"USDJPY\"}");

            Assert.Equal("USDJPY", (await next).Symbol);
        }
    }
}
