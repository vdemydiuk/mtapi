using System;
using System.Diagnostics;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace MTApiService
{
    public class MtLog
    {
        #region ctor
        internal MtLog(Type type)
        {
            _log = LogManager.GetLogger(type);
        }
        #endregion

        #region Public

        public void Debug(object message)
        {
            _log.Debug(message);
        }

        public void Error(object message)
        {
            _log.Error(message);
        }

        public void Fatal(object message)
        {
            _log.Fatal(message);
        }

        public void Info(object message)
        {
            _log.Info(message);
        }

        public void Warn(object message)
        {
            _log.Warn(message);
        }
        #endregion

        #region Private

        private readonly ILog _log;

        #endregion
    }

    public class LogConfigurator
    {
        private const string LogFileNameExtension = "log";

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

            string filename = $"{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}-{Process.GetCurrentProcess().Id}.{LogFileNameExtension}";

            var roller = new RollingFileAppender
            {
                AppendToFile = true,
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

#if (DEBUG)
            hierarchy.Root.Level = Level.Debug;
#else
            hierarchy.Root.Level = Level.Info;
#endif
            hierarchy.Configured = true;
        }

        public static MtLog GetLogger(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return new MtLog(type);
        }
    }
}
