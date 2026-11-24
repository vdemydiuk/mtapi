using MtApi5.MtProtocol;
using Newtonsoft.Json.Linq;

namespace MtApi5.Tests
{
    // Builds a connected MtApi5Client over a FakeRpcClient for command-level tests.
    internal static class MtApi5ClientTestHelper
    {
        public const string QuoteJson =
            "{\"Instrument\":\"EURUSD\",\"ExpertHandle\":1,\"Tick\":{\"Bid\":1.1,\"Ask\":1.2,\"Time\":1700000000,\"Last\":1.15,\"Volume\":7,\"VolumeReal\":0,\"Flags\":0}}";

        // 2023-11-14 22:13:20 UTC
        public const long MtTime = 1700000000;
        public static readonly DateTime Time = new(2023, 11, 14, 22, 13, 20);

        public static async Task<(MtApi5Client Client, FakeRpcClient Fake)> Connect(int executor = 1)
        {
            var fake = new FakeRpcClient { Experts = [executor] };
            fake.Handler = (_, type, _) => type switch
            {
                (int)Mt5CommandType.GetQuote => FakeRpcClient.Ok(QuoteJson),
                (int)Mt5CommandType.IsTesting => FakeRpcClient.Ok("false"),
                _ => FakeRpcClient.Ok("null")
            };
            var client = new MtApi5Client((_, _, _) => fake);
            await client.Connect("localhost", 8222);
            return (client, fake);
        }

        public static void Reply(this FakeRpcClient fake, string valueJson) =>
            fake.Handler = (_, _, _) => FakeRpcClient.Ok(valueJson);

        public static FakeRpcClient.SentCommand Last(this FakeRpcClient fake)
        {
            lock (fake.Sent) return fake.Sent.Last();
        }

        public static JObject LastPayload(this FakeRpcClient fake) => JObject.Parse(fake.Last().Payload);
    }
}
