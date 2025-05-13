using log4net;
using System;
using System.IO;
using System.Reflection;

namespace ScreenshotHook.Presentation.Utilities
{
    public static class LogHelper
    {
        // 使用当前类型获取 Logger
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Configure()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());

            var configFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config"));

            log4net.Config.XmlConfigurator.ConfigureAndWatch(logRepository, configFile);
        }

        public static void Info(object message)
        {
            if (log.IsInfoEnabled)
                log.Info(message);
        }

        public static void Warn(object message)
        {
            if (log.IsWarnEnabled)
                log.Warn(message);
        }

        public static void Error(object message, Exception ex = null)
        {
            if (log.IsErrorEnabled)
            {
                if (ex != null)
                {
                    log.Error(message, ex);
                }
                else
                {
                    log.Error(message);
                }
            }
        }

        public static void Error(Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.Error(ex.Message, ex);
            }
        }
    }
}