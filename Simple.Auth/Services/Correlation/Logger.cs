using Microsoft.Extensions.Logging;
using Simple.Auth.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Services
{
    [ExcludeFromCodeCoverage]
    public class Logger : ICorrelationLogger
    {
        private readonly ILogger _internalLogger;
        private readonly ICorrelationService _correlationService;
        public Logger(ILoggerFactory loggerFactory, Type catergoryType, ICorrelationService correlationService)
        {
            _internalLogger = loggerFactory.CreateLogger(catergoryType.Name);
            _correlationService = correlationService;
        }
        public Logger(ILoggerFactory loggerFactory, string catergoryName, ICorrelationService correlationService)
        {
            _internalLogger = loggerFactory.CreateLogger(catergoryName);
            _correlationService = correlationService;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => _internalLogger?.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => _internalLogger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => _internalLogger.Log(logLevel, eventId, state, exception, formatter);

        public void LogDebug(string message, params object?[] args)
        {
            TryPrefixCorrelationId(message, LogLevel.Debug, args);
        }

        public void LogError(string message, params object?[] args)
        {
            TryPrefixCorrelationId(message, LogLevel.Error, args);
        }

        public void LogError(Exception e)
        {
            var logMessage = "Error Thrown: {error}. {stackTrace}";
            TryPrefixCorrelationId(logMessage, LogLevel.Error, e.Message, e.StackTrace);
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
