using Microsoft.Extensions.Logging;

namespace Simple.Auth.Interfaces
{
    public interface ICorrelationLogger : ILogger
    {
        void LogInformation(string message, params object?[] args);

        void LogError(string message, params object?[] args);

        void LogDebug(string message, params object?[] args);

        void LogError(Exception e);

        void LogWarning(string message, params object?[] args);
    }

    public interface ICorrelationLoggerFactory : ILoggerFactory
    {
        ICorrelationLogger CreateLogger<T>();
    }
}