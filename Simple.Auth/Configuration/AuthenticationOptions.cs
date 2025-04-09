using Microsoft.Extensions.Configuration;
using Simple.Auth.Interfaces.Authentication;
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
        public AuthenticationOptions(IConfiguration configuration, TokenAccessOptions tokenAccessOptions, TokenServiceOptions tokenServiceOptions)
        {
            Configuration = configuration;
            TokenAccessOptions = tokenAccessOptions;
            TokenServiceOptions = tokenServiceOptions;
        }
    }
    public class TokenAccessOptions
    {
        public readonly Type? TokenAccessorType;
        public readonly CookieAccessorOptions? CookieAccessorOptions;
        public readonly Func<IServiceProvider, ITokenAccessor>? ImplementationFactory;
        public TokenAccessOptions(Type? tokenAccessorType, CookieAccessorOptions? cookieAccessorOptions, Func<IServiceProvider, ITokenAccessor>? implementationFactory)
        {
            TokenAccessorType = tokenAccessorType;
            CookieAccessorOptions = cookieAccessorOptions;
            ImplementationFactory = implementationFactory;
        }
    }
    public class TokenServiceOptions
    {
        public readonly Type? ServiceType;
        public readonly Func<IServiceProvider,ITokenService>? ImplementationFactory;
        public TokenServiceOptions(Type? serviceType, Func<IServiceProvider, ITokenService>? implementationFactory)
        {
            ServiceType = serviceType;
            ImplementationFactory = implementationFactory;
        }
    }
}
