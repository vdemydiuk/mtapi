using System;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace MTApiService
{
    public class LogConfigurator
    {
        private const string LogFileNameExtension = "txt";

        public static void Setup(string profileName)
        {
            if (string.IsNullOrEmpty(profileName))
                throw new ArgumentNullException();

            var hierarchy = (Hierarchy) LogManager.GetRepository();

            var patternLayout = new PatternLayout
            {
                ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
            };
            patternLayout.ActivateOptions();

            string filename = $"{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}.{LogFileNameExtension}";

            var roller = new RollingFileAppender
            {
                AppendToFile = false,
                File = $@"{System.IO.Path.GetTempPath()}{profileName}\Logs\{filename}",
                Layout = patternLayout,
                PreserveLogFileNameExtension = true,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "1GB",
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = false
            };
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            var memory = new MemoryAppender();
            memory.ActivateOptions();
            hierarchy.Root.AddAppender(memory);

#if (DEBUG)
            hierarchy.Root.Level = Level.Debug;
#else
            hierarchy.Root.Level = Level.Info;
#endif
            hierarchy.Configured = true;
        }
    }
}
