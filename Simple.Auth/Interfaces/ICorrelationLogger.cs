using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Interfaces
{
    public interface ICorrelationLogger
    {
        void LogInformation(string message, params object?[] args);
        void LogError(string message, params object?[] args);
        void LogError(Exception e);
        void LogWarning(string message, params object?[] args);
    }
    public interface ICorrelationLoggerFactory
    {
        ICorrelationLogger CreateLogger(string name);
        ICorrelationLogger CreateLogger<T>();
    }
}
