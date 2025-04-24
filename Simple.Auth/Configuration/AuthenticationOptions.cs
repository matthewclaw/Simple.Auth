using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Configuration
{
    public class AuthenticationOptions
    {
        public readonly IConfiguration Configuration;
        public readonly TokenAccessOptions TokenAccessOptions;
        public readonly TokenServiceOptions TokenServiceOptions;
        public readonly Type UserAuthenticatorType;
        public readonly List<Action<AuthenticationBuilder>> SchemeAdditions;
        public readonly bool CacheDisabled;
        public AuthenticationOptions(IConfiguration configuration, TokenAccessOptions tokenAccessOptions, 
            TokenServiceOptions tokenServiceOptions, Type userAuthenticatorType,
            List<Action<AuthenticationBuilder>> schemeAdditions, bool cacheDisabled)
        {
            Configuration = configuration;
            TokenAccessOptions = tokenAccessOptions;
            TokenServiceOptions = tokenServiceOptions;
            UserAuthenticatorType = userAuthenticatorType;
            SchemeAdditions = schemeAdditions;
            CacheDisabled = cacheDisabled;
        }
    }
    public class TokenAccessOptions
    {
        public readonly Type? TokenAccessorType;
        public readonly CookieAccessorOptions? CookieAccessorOptions;
        public readonly Func<IServiceProvider, HttpTokenAccessor>? ImplementationFactory;
        public TokenAccessOptions(Type? tokenAccessorType, CookieAccessorOptions? cookieAccessorOptions, Func<IServiceProvider, HttpTokenAccessor>? implementationFactory)
        {
            TokenAccessorType = tokenAccessorType;
            CookieAccessorOptions = cookieAccessorOptions;
            ImplementationFactory = implementationFactory;
        }
    }
    public class TokenServiceOptions
    {
        public readonly Type? ServiceType;
        public readonly Func<IServiceProvider, ITokenService>? ImplementationFactory;
        public TokenServiceOptions(Type? serviceType, Func<IServiceProvider, ITokenService>? implementationFactory)
        {
            ServiceType = serviceType;
            ImplementationFactory = implementationFactory;
        }
    }
}
