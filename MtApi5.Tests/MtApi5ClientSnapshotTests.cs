using MtApi5.MtProtocol;
using Xunit;
using static MtApi5.Tests.MtApi5ClientTestHelper;

namespace MtApi5.Tests
{
    // GetPositions/GetOrders return all open positions or pending orders with all their properties
    // in one request (#345). Replies below are built the way the expert writes them (MtApi5.mq5).
    public class MtApi5ClientSnapshotTests
    {
        private const string PositionJson =
            "{\"Ticket\":9223372036854775807,\"Identifier\":9223372036854775806,\"Symbol\":\"EURUSD\"," +
            "\"Type\":1,\"Reason\":3,\"Magic\":123456789012,\"Volume\":0.15,\"PriceOpen\":1.0855," +
            "\"StopLoss\":1.09,\"TakeProfit\":1.07,\"PriceCurrent\":1.0849,\"Swap\":-0.42,\"Profit\":9.0," +
            "\"MtTime\":1700000000,\"TimeMsc\":1700000000123,\"MtTimeUpdate\":1700003600,\"TimeUpdateMsc\":1700003600456," +
            "\"Comment\":\"say \\\"hi\\\" \\\\ tab\\tnew\\nline\",\"ExternalId\":\"ext-1\"}";

        private const string OrderJson =
            "{\"Ticket\":18446744073709551615,\"Symbol\":\"GBPUSD\",\"Type\":7,\"State\":1,\"TypeTime\":2," +
            "\"TypeFilling\":1,\"Reason\":3,\"Magic\":42,\"PositionId\":9223372036854775807,\"PositionById\":5," +
            "\"VolumeInitial\":0.3,\"VolumeCurrent\":0.2,\"PriceOpen\":1.25,\"StopLoss\":1.2,\"TakeProfit\":1.3," +
            "\"PriceCurrent\":1.26,\"PriceStopLimit\":1.255,\"MtTimeSetup\":1700000000,\"TimeSetupMsc\":1700000000789," +
            "\"MtTimeExpiration\":1700086400,\"Comment\":\"q\\\"c\",\"ExternalId\":\"\"}";

        [Fact]
        public async Task GetPositions_SendsCommandWithoutPayload_AndMapsAllProperties()
        {
            var (client, fake) = await Connect();
            fake.Reply($"[{PositionJson}]");

            var positions = client.GetPositions();

            var sent = fake.Last();
            Assert.Equal((int)Mt5CommandType.GetPositions, sent.CommandType);
            Assert.Equal(390, sent.CommandType);
            Assert.Equal("", sent.Payload);

            Assert.NotNull(positions);
            var p = Assert.Single(positions);
            Assert.Equal((ulong)long.MaxValue, p.Ticket);
            Assert.Equal(long.MaxValue - 1, p.Identifier);
            Assert.Equal("EURUSD", p.Symbol);
            Assert.Equal(ENUM_POSITION_TYPE.POSITION_TYPE_SELL, p.Type);
            Assert.Equal(ENUM_POSITION_REASON.POSITION_REASON_EXPERT, p.Reason);
            Assert.Equal(123456789012L, p.Magic);
            Assert.Equal(0.15, p.Volume);
            Assert.Equal(1.0855, p.PriceOpen);
            Assert.Equal(1.09, p.StopLoss);
            Assert.Equal(1.07, p.TakeProfit);
            Assert.Equal(1.0849, p.PriceCurrent);
            Assert.Equal(-0.42, p.Swap);
            Assert.Equal(9.0, p.Profit);
            Assert.Equal(MtTime, p.MtTime);
            Assert.Equal(Time, p.Time);
            Assert.Equal(1700000000123L, p.TimeMsc);
            Assert.Equal(1700003600L, p.MtTimeUpdate);
            Assert.Equal(Time.AddHours(1), p.TimeUpdate);
            Assert.Equal(1700003600456L, p.TimeUpdateMsc);
            Assert.Equal("say \"hi\" \\ tab\tnew\nline", p.Comment);
            Assert.Equal("ext-1", p.ExternalId);
        }

        [Fact]
        public async Task GetOrders_SendsCommandWithoutPayload_AndMapsAllProperties()
        {
            var (client, fake) = await Connect();
            fake.Reply($"[{OrderJson}]");

            var orders = client.GetOrders();

            var sent = fake.Last();
            Assert.Equal((int)Mt5CommandType.GetOrders, sent.CommandType);
            Assert.Equal(391, sent.CommandType);
            Assert.Equal("", sent.Payload);

            Assert.NotNull(orders);
            var o = Assert.Single(orders);
            Assert.Equal(ulong.MaxValue, o.Ticket);
            Assert.Equal("GBPUSD", o.Symbol);
            Assert.Equal(ENUM_ORDER_TYPE.ORDER_TYPE_SELL_STOP_LIMIT, o.Type);
            Assert.Equal(ENUM_ORDER_STATE.ORDER_STATE_PLACED, o.State);
            Assert.Equal(ENUM_ORDER_TYPE_TIME.ORDER_TIME_SPECIFIED, o.TypeTime);
            Assert.Equal(ENUM_ORDER_TYPE_FILLING.ORDER_FILLING_IOC, o.TypeFilling);
            Assert.Equal(ENUM_ORDER_REASON.ORDER_REASON_EXPERT, o.Reason);
            Assert.Equal(42L, o.Magic);
            Assert.Equal(long.MaxValue, o.PositionId);
            Assert.Equal(5L, o.PositionById);
            Assert.Equal(0.3, o.VolumeInitial);
            Assert.Equal(0.2, o.VolumeCurrent);
            Assert.Equal(1.25, o.PriceOpen);
            Assert.Equal(1.2, o.StopLoss);
            Assert.Equal(1.3, o.TakeProfit);
            Assert.Equal(1.26, o.PriceCurrent);
            Assert.Equal(1.255, o.PriceStopLimit);
            Assert.Equal(MtTime, o.MtTimeSetup);
            Assert.Equal(Time, o.TimeSetup);
            Assert.Equal(1700000000789L, o.TimeSetupMsc);
            Assert.Equal(1700086400L, o.MtTimeExpiration);
            Assert.Equal(Time.AddDays(1), o.TimeExpiration);
            Assert.Equal("q\"c", o.Comment);
            Assert.Equal("", o.ExternalId);
        }

        [Fact]
        public async Task GetPositions_KeepsOrderOfMultipleItems()
        {
            var (client, fake) = await Connect();
            fake.Reply("[{\"Ticket\":1,\"Symbol\":\"EURUSD\",\"Type\":0},{\"Ticket\":2,\"Symbol\":\"USDJPY\",\"Type\":1}]");

            var positions = client.GetPositions();

            Assert.NotNull(positions);
            Assert.Equal([1UL, 2UL], positions.Select(p => p.Ticket));
            Assert.Equal(["EURUSD", "USDJPY"], positions.Select(p => p.Symbol));
            Assert.Equal([ENUM_POSITION_TYPE.POSITION_TYPE_BUY, ENUM_POSITION_TYPE.POSITION_TYPE_SELL], positions.Select(p => p.Type));
        }

        [Fact]
        public async Task GetPositionsAndOrders_EmptyReply_ReturnsEmptyLists()
        {
            var (client, fake) = await Connect();
            fake.Reply("[]");

            var positions = client.GetPositions();
            var orders = client.GetOrders();

            Assert.NotNull(positions);
            Assert.Empty(positions);
            Assert.NotNull(orders);
            Assert.Empty(orders);
        }

        [Fact]
        public async Task GetPositions_MissingProperties_UseDefaults()
        {
            var (client, fake) = await Connect();
            fake.Reply("[{\"Ticket\":3}]");

            var p = Assert.Single(client.GetPositions()!);

            Assert.Equal(3UL, p.Ticket);
            Assert.Equal(string.Empty, p.Symbol);
            Assert.Equal(string.Empty, p.Comment);
            Assert.Equal(string.Empty, p.ExternalId);
            Assert.Equal(new DateTime(1970, 1, 1), p.Time);
        }
    }
}
