using Microsoft.Extensions.Logging;
using Simple.Auth.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InternalLogger = Microsoft.Extensions.Logging.ILogger;
using InternalLoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Simple.Auth.Services
{
    public class Logger : ILogger
    {
        private readonly InternalLogger _internalLogger;
        private readonly ICorrelationService _correlationService;
        public Logger(InternalLoggerFactory loggerFactory, Type catergoryType, ICorrelationService correlationService)
        {
            _internalLogger = loggerFactory.CreateLogger(catergoryType.Name);
            _correlationService = correlationService;
        }
        public Logger(InternalLoggerFactory loggerFactory, string catergoryName, ICorrelationService correlationService)
        {
            _internalLogger = loggerFactory.CreateLogger(catergoryName);
            _correlationService = correlationService;
        }

        public void LogError(string message, params object?[] args)
        {
            TryPrefixCorrelationId(message, LogLevel.Error, args);
        }

        public void LogInformation(string message, params object?[] args)
        {
           TryPrefixCorrelationId(message, LogLevel.Information, args);
        }

        public void LogWarning(string message, params object?[] args)
        {
            TryPrefixCorrelationId(message, LogLevel.Warning, args);
        }

        private void TryPrefixCorrelationId(string message, LogLevel level, params object?[] args)
        {
            var correlationId = _correlationService.GetCorrelationId();
            if (string.IsNullOrEmpty(correlationId))
            {
                _internalLogger.Log(level, message, args);
                return;
            }
            string correlationPrefix = "[Correlation: {correlationId}] ";
            var newMessage = correlationPrefix + message;
            if (args == null)
            {
                args = new object[1];
                args[0] = correlationId;
                _internalLogger.Log(level, newMessage, args);
                return;
            }
                object[] combined = new object[args.Length + 1];
                combined[0] = correlationId;
                Array.Copy(args, 0, combined, 1, args.Length);
                _internalLogger.Log(level, newMessage, combined);
        }
    }
}
