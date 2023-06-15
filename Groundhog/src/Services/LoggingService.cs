using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;

namespace Groundhog.Services
{
    public class LoggingService
    {
        private readonly ILogger _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }

        public Task LogAsync(LogMessage logMessage)
        {
            // 根据日志级别输出不同颜色的日志
            var logColor = logMessage.Severity switch
            {
                LogSeverity.Critical => ConsoleColor.Red,
                LogSeverity.Error => ConsoleColor.DarkRed,
                LogSeverity.Warning => ConsoleColor.Yellow,
                LogSeverity.Info => ConsoleColor.White,
                LogSeverity.Verbose => ConsoleColor.Gray,
                LogSeverity.Debug => ConsoleColor.DarkGray,
                _ => ConsoleColor.White,
            };

            // 设置控制台颜色并输出日志
            Console.ForegroundColor = logColor;
            _logger.LogInformation(logMessage.ToString());
            Console.ResetColor();

            return Task.CompletedTask;
        }

        // 創建Discord.LogMessage實例，一般資訊LogSeverity.Info
        public Task LogInfoAsync(string unit,string message)
        {
            var logMessage = new LogMessage(LogSeverity.Info, '[' + unit + ']', message);
            return LogAsync(logMessage);
        }

        // 創建Discord.LogMessage實例，錯誤資訊LogSeverity.Error
        public Task LogErrorAsync(string unit, string message)
        {
            var logMessage = new LogMessage(LogSeverity.Error, '[' + unit + ']', message);
            return LogAsync(logMessage);
        }
    }
}
