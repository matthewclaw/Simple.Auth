using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Interfaces
{
    public interface ICorrelationLogger: ILogger
    {
        void LogInformation(string message, params object?[] args);
        void LogError(string message, params object?[] args);
        void LogError(Exception e);
        void LogWarning(string message, params object?[] args);
    }
    public interface ICorrelationLoggerFactory: ILoggerFactory
    {
        ICorrelationLogger CreateLogger<T>();
    }
}
