using Xunit;

namespace MtApi5.Tests
{
    // MtRpcClient messages reach the user's IMtLogger through RpcClientLogger; levels must be preserved.
    public class RpcClientLoggerTests
    {
        private sealed class RecordingLogger : IMtLogger
        {
            public List<(string Level, object Message)> Entries { get; } = [];
            public void Debug(object message) => Entries.Add(("Debug", message));
            public void Error(object message) => Entries.Add(("Error", message));
            public void Fatal(object message) => Entries.Add(("Fatal", message));
            public void Info(object message) => Entries.Add(("Info", message));
            public void Warn(object message) => Entries.Add(("Warn", message));
        }

        [Fact]
        public void Levels_AreForwardedToMatchingLoggerMethods()
        {
            var rec = new RecordingLogger();
            var log = new RpcClientLogger(rec);

            log.Debug("d");
            log.Info("i");
            log.Warn("w");
            log.Error("e");

            Assert.Equal([("Debug", (object)"d"), ("Info", "i"), ("Warn", "w"), ("Error", "e")], rec.Entries);
        }
    }
}
