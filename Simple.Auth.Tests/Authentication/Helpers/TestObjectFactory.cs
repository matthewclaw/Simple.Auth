using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Simple.Auth.Interfaces;
using Simple.Auth.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Tests.Authentication.Helpers
{
    [ExcludeFromCodeCoverage]
    public static class TestObjectFactory
    {
        public static ICorrelationLoggerFactory GetLoggerFactory()
        {
            var mockBaseLoggerFactory = Substitute.For<ILoggerFactory>();
            var mockServiceProvider = Substitute.For<IServiceProvider>();
            var mockCorrelationService = Substitute.For<ICorrelationService>();
            var loggerFactory = CorrelationLoggerFactory.GetInstance(mockBaseLoggerFactory, mockServiceProvider, mockCorrelationService);
            return loggerFactory;
        }

        public static ClaimsPrincipal GetClaimsPrincipal(string scheme = Constants.Schemes.DEFAULT)
        => GetClaimsPrincipal(null, scheme);

        public static ClaimsPrincipal GetClaimsPrincipal(Dictionary<string, string>? claims, string scheme = Constants.Schemes.DEFAULT)
        {
            var castClaims = claims?.Select(c => new Claim(c.Key, c.Value)).ToList() ?? new List<Claim>();
            var principle = new ClaimsPrincipal(new ClaimsIdentity(castClaims, scheme));
            return principle;
        }
    }
}
