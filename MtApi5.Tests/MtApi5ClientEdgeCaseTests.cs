using MtApi5.MtProtocol;
using Xunit;
using static MtApi5.Tests.MtApi5ClientTestHelper;

namespace MtApi5.Tests
{
    // Connection edge cases, event robustness and logging of MtApi5Client.
    public class MtApi5ClientEdgeCaseTests
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        private sealed class RecordingLogger : IMtLogger
        {
            public List<(string Level, string Message)> Entries { get; } = [];
            private void Add(string l, object m) { lock (Entries) Entries.Add((l, m.ToString() ?? "")); }
            public void Debug(object message) => Add("Debug", message);
            public void Error(object message) => Add("Error", message);
            public void Fatal(object message) => Add("Fatal", message);
            public void Info(object message) => Add("Info", message);
            public void Warn(object message) => Add("Warn", message);
        }

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

        private static Task<T> Next<T>(Action<Action<T>> subscribe)
        {
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            subscribe(v => tcs.TrySetResult(v));
            return tcs.Task.WaitAsync(Timeout);
        }

        // ---- connect ----

        [Fact]
        public async Task Connect_PassesHostAndPortToFactory()
        {
            var fake = NewFake();
            (string, int)? seen = null;
            var client = new MtApi5Client((h, p, _) => { seen = (h, p); return fake; });

            await client.Connect("192.168.1.2", 8333);

            Assert.Equal(("192.168.1.2", 8333), seen);
        }

        [Fact]
        public async Task Connect_NullExpertList_Fails()
        {
            var fake = NewFake();
            fake.Experts = null;
            var client = new MtApi5Client((_, _, _) => fake);

            await Assert.ThrowsAsync<Exception>(() => client.Connect("localhost", 8222));
        }

        [Fact]
        public async Task Connect_InTesterMode_SendsBacktestingReady()
        {
            var fake = NewFake();
            fake.Handler = (_, type, _) => type switch
            {
                (int)Mt5CommandType.GetQuote => FakeRpcClient.Ok(QuoteJson),
                (int)Mt5CommandType.IsTesting => FakeRpcClient.Ok("true"),
                _ => FakeRpcClient.Ok("null")
            };
            var client = new MtApi5Client((_, _, _) => fake);

            await client.Connect("localhost", 8222);

            Assert.Contains(fake.Sent, s => s.CommandType == (int)Mt5CommandType.BacktestingReady);
        }

        [Fact]
        public async Task Connect_NotInTesterMode_DoesNotSendBacktestingReady()
        {
            var (_, fake) = await MtApi5ClientTestHelper.Connect();
            Assert.DoesNotContain(fake.Sent, s => s.CommandType == (int)Mt5CommandType.BacktestingReady);
        }

        [Fact]
        public async Task Connect_QuoteCommandFails_ConnectionFails()
        {
            var fake = NewFake();
            fake.Handler = (_, _, _) => "{\"ErrorCode\":4001,\"ErrorMessage\":\"boom\"}";
            var client = new MtApi5Client((_, _, _) => fake);
            var states = new List<Mt5ConnectionState>();
            client.ConnectionStateChanged += (_, e) => states.Add(e.Status);

            var ex = await Assert.ThrowsAsync<Exception>(() => client.Connect("localhost", 8222));

            Assert.Contains("boom", ex.Message);
            Assert.Equal(Mt5ConnectionState.Failed, states.Last());
            Assert.True(fake.DisconnectCalls >= 1);
        }

        [Fact]
        public async Task Connect_ExpertWithoutInstrument_IsSkippedInQuoteList()
        {
            var fake = NewFake(1, 2);
            fake.Handler = (handle, type, _) => type switch
            {
                (int)Mt5CommandType.GetQuote => handle == 1
                    ? FakeRpcClient.Ok(QuoteJson)
                    : FakeRpcClient.Ok("{\"Instrument\":\"\"}"),
                (int)Mt5CommandType.IsTesting => FakeRpcClient.Ok("false"),
                _ => FakeRpcClient.Ok("null")
            };
            var client = new MtApi5Client((_, _, _) => fake);
            Mt5QuotesEventArgs? args = null;
            client.QuoteList += (_, e) => args = e;

            await client.Connect("localhost", 8222);

            Assert.Single(args!.Quotes);
            Assert.Equal(1, client.GetQuotes().Single().ExpertHandle);
        }

        [Fact]
        public async Task ConcurrentConnect_CreatesOnlyOneClient()
        {
            var gate = new TaskCompletionSource();
            var fake = new SlowConnectFake(gate.Task) { Experts = [1] };
            fake.Handler = (_, type, _) => type == (int)Mt5CommandType.GetQuote ? FakeRpcClient.Ok(QuoteJson) : FakeRpcClient.Ok("false");
            var created = 0;
            var client = new MtApi5Client((_, _, _) => { Interlocked.Increment(ref created); return fake; });

            var first = client.Connect("localhost", 8222);
            await client.Connect("localhost", 8222);   // returns immediately: already connecting
            gate.SetResult();
            await first;

            Assert.Equal(1, created);
        }

        private sealed class SlowConnectFake(Task gate) : MtClient.IMtRpcClient
        {
            private readonly FakeRpcClient inner = new();
            public HashSet<int>? Experts { get => inner.Experts; set => inner.Experts = value; }
            public Func<int, int, string, string?> Handler { set => inner.Handler = value; }
            public async Task Connect() => await gate;
            public void Disconnect() => inner.Disconnect();
            public string? SendCommand(int e, int t, string p, int timeout = 10000) => inner.SendCommand(e, t, p, timeout);
            public HashSet<int>? RequestExpertsList() => inner.RequestExpertsList();
            public event EventHandler<EventArgs>? ConnectionFailed { add { } remove { } }
            public event EventHandler<EventArgs>? Disconnected { add { } remove { } }
            public event EventHandler<MtClient.MtExpertEventArgs>? ExpertAdded { add { } remove { } }
            public event EventHandler<MtClient.MtExpertEventArgs>? ExpertRemoved { add { } remove { } }
            public event EventHandler<MtClient.MtEventArgs>? MtEventReceived { add { } remove { } }
        }

        [Fact]
        public async Task Reconnect_AfterDisconnect_CreatesNewClient()
        {
            var fakes = new Queue<FakeRpcClient>([NewFake(), NewFake()]);
            var client = new MtApi5Client((_, _, _) => fakes.Dequeue());
            var states = new List<Mt5ConnectionState>();
            client.ConnectionStateChanged += (_, e) => states.Add(e.Status);

            await client.Connect("localhost", 8222);
            client.Disconnect();
            await client.Connect("localhost", 8222);

            Assert.Equal([Mt5ConnectionState.Connecting, Mt5ConnectionState.Connected, Mt5ConnectionState.Disconnected,
                Mt5ConnectionState.Connecting, Mt5ConnectionState.Connected], states);
            Assert.Empty(fakes);
        }

        [Fact]
        public async Task AfterDisconnect_CommandsAndGetQuotes_Throw()
        {
            var (client, _) = await MtApi5ClientTestHelper.Connect();
            client.Disconnect();

            Assert.Throws<Exception>(() => client.PositionsTotal());
            Assert.Throws<Exception>(() => client.GetQuotes());
        }

        [Fact]
        public async Task BeginConnect_ConnectsInBackground_ToLocalhost()
        {
            var fake = NewFake();
            (string, int)? seen = null;
            var client = new MtApi5Client((h, p, _) => { seen = (h, p); return fake; });
            var connected = Next<Mt5ConnectionEventArgs>(h => client.ConnectionStateChanged += (_, e) =>
            {
                if (e.Status == Mt5ConnectionState.Connected) h(e);
            });

            client.BeginConnect(8444);

            await connected;
            Assert.Equal(("localhost", 8444), seen);
        }

        [Fact]
        public async Task BeginConnect_Failure_DoesNotThrowToCaller()
        {
            var fake = NewFake();
            fake.ConnectException = new InvalidOperationException("refused");
            var client = new MtApi5Client((_, _, _) => fake);
            var failed = Next<Mt5ConnectionEventArgs>(h => client.ConnectionStateChanged += (_, e) =>
            {
                if (e.Status == Mt5ConnectionState.Failed) h(e);
            });

            client.BeginConnect("localhost", 8222);

            Assert.Equal(Mt5ConnectionState.Failed, (await failed).Status);
        }

        [Fact]
        public async Task BeginDisconnect_DisconnectsInBackground()
        {
            var (client, fake) = await MtApi5ClientTestHelper.Connect();
            var disconnected = Next<Mt5ConnectionEventArgs>(h => client.ConnectionStateChanged += (_, e) => h(e));

            client.BeginDisconnect();

            Assert.Equal(Mt5ConnectionState.Disconnected, (await disconnected).Status);
            Assert.Equal(1, fake.DisconnectCalls);
        }

        // ---- experts ----

        [Fact]
        public async Task ExpertAdded_Duplicate_DoesNotRaiseQuoteAddedAgain()
        {
            var (client, fake) = await MtApi5ClientTestHelper.Connect(1);
            var added = new List<int>();
            var second = Next<int>(h => client.QuoteAdded += (_, e) =>
            {
                lock (added) added.Add(e.Quote.ExpertHandle);
                if (e.Quote.ExpertHandle == 2) h(2);
            });

            fake.RaiseExpertAdded(1);   // already known
            fake.RaiseExpertAdded(2);
            await second;
            await Task.Delay(100);

            lock (added) Assert.Equal([2], added);
        }

        [Fact]
        public async Task ExpertRemoved_Unknown_DoesNothing()
        {
            var (client, fake) = await MtApi5ClientTestHelper.Connect(1);
            var removed = new List<int>();
            client.QuoteRemoved += (_, e) => { lock (removed) removed.Add(e.Quote.ExpertHandle); };

            fake.RaiseExpertRemoved(99);
            await Task.Delay(100);

            lock (removed) Assert.Empty(removed);
            Assert.Equal(1, client.ExecutorHandle);
        }

        [Fact]
        public async Task LastExpertRemoved_ResetsExecutor_AndNewExpertBecomesExecutor()
        {
            var (client, fake) = await MtApi5ClientTestHelper.Connect(1);
            var removed = Next<Mt5QuoteEventArgs>(h => client.QuoteRemoved += (_, e) => h(e));
            fake.RaiseExpertRemoved(1);
            await removed;
            Assert.Equal(0, client.ExecutorHandle);

            var added = Next<Mt5QuoteEventArgs>(h => client.QuoteAdded += (_, e) => h(e));
            fake.RaiseExpertAdded(2);
            await added;

            Assert.Equal(2, client.ExecutorHandle);
        }

        [Fact]
        public async Task ExpertAdded_WithoutQuote_DoesNotRaiseQuoteAdded_ButIsKnown()
        {
            var (client, fake) = await MtApi5ClientTestHelper.Connect(1);
            fake.Handler = (_, type, _) => type == (int)Mt5CommandType.GetQuote
                ? FakeRpcClient.Ok("{\"Instrument\":\"\"}") : FakeRpcClient.Ok("null");
            var raised = false;
            client.QuoteAdded += (_, _) => raised = true;

            fake.RaiseExpertAdded(2);
            await Task.Delay(100);

            Assert.False(raised);
            Assert.Contains(fake.Sent, s => s.ExpertHandle == 2 && s.CommandType == (int)Mt5CommandType.GetQuote);
        }

        // ---- event robustness ----

        [Fact]
        public async Task ThrowingSubscriber_DoesNotStopLaterEvents()
        {
            var (client, fake) = await MtApi5ClientTestHelper.Connect();
            var calls = 0;
            var second = Next<Mt5QuoteEventArgs>(h => client.QuoteUpdate += (_, e) =>
            {
                if (Interlocked.Increment(ref calls) == 1)
                    throw new InvalidOperationException("subscriber bug");
                h(e);
            });

            fake.RaiseEvent(1, (int)Mt5EventTypes.OnTick, QuoteJson);
            await Task.Delay(50);
            fake.RaiseEvent(1, (int)Mt5EventTypes.OnTick, QuoteJson);

            Assert.Equal("EURUSD", (await second).Quote.Instrument);
        }

        [Theory]
        [InlineData("not json")]
        [InlineData("")]
        [InlineData("null")]
        [InlineData("{\"Instrument\":\"\"}")]
        public async Task MalformedOrIncompleteTickPayload_IsIgnored_AndLaterEventsStillWork(string badPayload)
        {
            var (client, fake) = await MtApi5ClientTestHelper.Connect();
            var updates = new List<string>();
            var good = Next<string>(h => client.QuoteUpdate += (_, e) =>
            {
                lock (updates) updates.Add(e.Quote.Instrument);
                h(e.Quote.Instrument);
            });

            fake.RaiseEvent(1, (int)Mt5EventTypes.OnTick, badPayload);
            await Task.Delay(50);
            fake.RaiseEvent(1, (int)Mt5EventTypes.OnTick, QuoteJson);

            Assert.Equal("EURUSD", await good);
            lock (updates) Assert.Single(updates);
        }

        [Fact]
        public async Task UnknownEventType_DoesNotBreakClient()
        {
            var (client, fake) = await MtApi5ClientTestHelper.Connect();
            var next = Next<Mt5LockTicksEventArgs>(h => client.OnLockTicks += (_, e) => h(e));

            fake.RaiseEvent(1, 9999, "{}");
            await Task.Delay(50);
            fake.RaiseEvent(1, (int)Mt5EventTypes.OnLockTicks, "{\"Instrument\":\"EURUSD\"}");

            Assert.Equal("EURUSD", (await next).Symbol);
        }

        [Theory]
        [InlineData(2, "{}")]
        [InlineData(5, "{}")]
        [InlineData(4, "{\"Instrument\":\"EURUSD\"}")]   // no rates
        public async Task IncompleteEventPayloads_AreIgnored(int type, string payload)
        {
            var (client, fake) = await MtApi5ClientTestHelper.Connect();
            var raised = 0;
            client.OnBookEvent += (_, _) => Interlocked.Increment(ref raised);
            client.OnLockTicks += (_, _) => Interlocked.Increment(ref raised);
            client.OnLastTimeBar += (_, _) => Interlocked.Increment(ref raised);

            fake.RaiseEvent(1, type, payload);
            await Task.Delay(100);

            Assert.Equal(0, raised);
        }

        [Fact]
        public async Task TradeTransactionEvent_NullPayload_IsIgnored()
        {
            var (client, fake) = await MtApi5ClientTestHelper.Connect();
            var raised = 0;
            client.OnTradeTransaction += (_, _) => Interlocked.Increment(ref raised);

            fake.RaiseEvent(1, (int)Mt5EventTypes.OnTradeTransaction, "null");
            await Task.Delay(100);

            Assert.Equal(0, raised);
        }

        // ---- logging ----

        [Fact]
        public async Task Logger_ReceivesConnectionAndCommandMessages()
        {
            var log = new RecordingLogger();
            var fake = NewFake();
            var client = new MtApi5Client((_, _, _) => fake, log);

            await client.Connect("localhost", 8222);
            fake.Handler = (_, _, _) => "{\"ErrorCode\":4301,\"ErrorMessage\":\"Unknown symbol\"}";
            Assert.Throws<ExecutionException>(() => client.PositionsTotal());
            client.Disconnect();

            lock (log.Entries)
            {
                Assert.Contains(log.Entries, e => e.Level == "Info" && e.Message.Contains("Connected to localhost:8222"));
                Assert.Contains(log.Entries, e => e.Level == "Warn" && e.Message.Contains("4301"));
                Assert.Contains(log.Entries, e => e.Level == "Info" && e.Message.Contains("Disconnect"));
            }
        }

        [Fact]
        public void Logger_NoConnection_WarnsOnCommand()
        {
            var log = new RecordingLogger();
            var client = new MtApi5Client((_, _, _) => NewFake(), log);

            Assert.Throws<Exception>(() => client.PositionsTotal());

            lock (log.Entries)
                Assert.Contains(log.Entries, e => e.Level == "Warn" && e.Message.Contains("No connection"));
        }

        [Fact]
        public async Task Logger_ConnectFailure_LogsError()
        {
            var log = new RecordingLogger();
            var fake = NewFake();
            fake.ConnectException = new InvalidOperationException("refused");
            var client = new MtApi5Client((_, _, _) => fake, log);

            await Assert.ThrowsAsync<Exception>(() => client.Connect("localhost", 8222));

            lock (log.Entries)
                Assert.Contains(log.Entries, e => e.Level == "Error" && e.Message.Contains("refused"));
        }
    }
}
