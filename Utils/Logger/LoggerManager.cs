﻿using Microsoft.Extensions.Logging;
using NLog;

namespace BuildingManager.Utils.Logger
{
    public class LoggerManager: ILoggerManager
    {
        private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();
        public LoggerManager() { }
        public void LogInfo(string message) => Logger.Info(message);
        public void LogWarn(string message) => Logger.Warn(message);
        public void LogDebug(string message) => Logger.Debug(message);
        public void LogError(string message) => Logger.Error(message);
    }
}
