using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Simple.Auth.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Services
{
    public class CorrelationLoggerFactory : ICorrelationLoggerFactory
    {
        private readonly ILoggerFactory _internalFactory;
        private readonly IServiceProvider _serviceProvider;
        public CorrelationLoggerFactory(ILoggerFactory internalFactory, IServiceProvider serviceProvider)
        {
            _internalFactory = internalFactory;
            _serviceProvider = serviceProvider;
        }

        public void AddProvider(ILoggerProvider provider)
        {
            _internalFactory.AddProvider(provider);
        }

        public ILogger CreateLogger(string categoryName)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var correlationService = scope.ServiceProvider.GetRequiredService<ICorrelationService>();
                return new Logger(_internalFactory, categoryName, correlationService);
            }
        }

        public ICorrelationLogger CreateLogger<T>()
        {
            return (ICorrelationLogger)CreateLogger(typeof(T).Name);
        }

        public void Dispose()
        {
            _internalFactory?.Dispose();
        }

    }
}
