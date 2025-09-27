using MtApi;

namespace TestApiClientUI
{
    class MtLogger : IMtLogger
    {
        public void Debug(object message)
        {
            Write("DEBUG", message);
        }

        public void Error(object message)
        {
            Write("ERROR", message);
        }

        public void Fatal(object message)
        {
            Write("FATAL", message);
        }

        public void Info(object message)
        {
            Write("INFO", message);
        }

        public void Warn(object message)
        {
            Write("WARN", message);
        }
        private void Write(string level, object message)
        {
            Console.WriteLine($"[{Environment.CurrentManagedThreadId}] [{level}] {message}");
        }
    }
}
