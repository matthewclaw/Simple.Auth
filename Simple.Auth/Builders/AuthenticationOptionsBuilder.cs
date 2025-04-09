using Microsoft.Extensions.Configuration;
using Simple.Auth.Configuration;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Builders
{
    public class AuthenticationOptionsBuilder
    {
        private IConfiguration? _configuration;
        private Type? _tokenAccessorType;
        private Type? _tokenServiceType;
        private CookieAccessorOptions? _cookieAccessorOptions;
        private Func<IServiceProvider, ITokenAccessor>? _tokenAccessorFactory;
        private Func<IServiceProvider, ITokenService>? _tokenServiceFactory;

        /// <summary>
        /// Required. This is the instance of IConfiguration that will be used for the required services
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public AuthenticationOptionsBuilder WithConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
            return this;
        }

        /// <summary>
        /// Configures the authentication to use a custom header accessor, providing a factory for creating instances of the accessor.
        /// </summary>
        /// <typeparam name="THeaderAccessor">The type of the custom header accessor, inheriting from BearerTokenAccessor.</typeparam>
        /// <param name="factory">A factory function that resolves the custom header accessor from the service provider.</param>
        /// <returns>The AuthenticationOptionsBuilder instance for chaining.</returns>
        public AuthenticationOptionsBuilder UseAuthHeader<THeaderAccessor>(Func<IServiceProvider, THeaderAccessor> factory) where THeaderAccessor : BearerTokenAccessor
        {
            _tokenAccessorFactory = factory;
            return this;
        }

        /// <summary>
        /// Configures the authentication to use a custom header accessor.
        /// </summary>
        /// <typeparam name="THeaderAccessor">The type of the custom header accessor, inheriting from BearerTokenAccessor.</typeparam>
        /// <returns>The AuthenticationOptionsBuilder instance for chaining.</returns>
        public AuthenticationOptionsBuilder UseAuthHeader<THeaderAccessor>() where THeaderAccessor : BearerTokenAccessor
        {
            _tokenAccessorType = typeof(THeaderAccessor);
            return this;
        }

        /// <summary>
        /// Configures the authentication to use the default cookie accessor (CookieTokenAccessor).
        /// </summary>
        public AuthenticationOptionsBuilder UseCookies() => UseCookies<CookieTokenAccessor>();

        /// <summary>
        /// Configures the authentication to use a custom cookie accessor, providing a factory for creating instances of the accessor.
        /// </summary>
        /// <typeparam name="TCookieAccessor">The type of the custom cookie accessor, inheriting from CookieTokenAccessor.</typeparam>
        /// <param name="factory">A factory function that resolves the custom cookie accessor from the service provider.</param>
        /// <returns>The AuthenticationOptionsBuilder instance for chaining.</returns>
        public AuthenticationOptionsBuilder UseCookies<TCookieAccessor>(Func<IServiceProvider, TCookieAccessor> factory) where TCookieAccessor : CookieTokenAccessor
        {
            _tokenAccessorFactory = factory;
            return this;
        }

        /// <summary>
        /// Configures the authentication to use a custom cookie accessor.
        /// </summary>
        /// <typeparam name="TCookieAccessor">The type of the custom cookie accessor, inheriting from CookieTokenAccessor.</typeparam>
        public AuthenticationOptionsBuilder UseCookies<TCookieAccessor>() where TCookieAccessor : CookieTokenAccessor
        {
            _tokenAccessorType = typeof(TCookieAccessor);
            return this;
        }

        /// <summary>
        /// Configures the authentication to use the default cookie accessor (CookieTokenAccessor) with specified options.
        /// </summary>
        public AuthenticationOptionsBuilder UseCookiesWithOptions(CookieAccessorOptions options) => UseCookiesWithOptions<CookieTokenAccessor>(options);

        /// <summary>
        /// Configures the authentication to use a custom cookie accessor with specified options.
        /// </summary>
        /// <typeparam name="TCookieAccessor">The type of the custom cookie accessor, inheriting from CookieTokenAccessor.</typeparam>
        /// <param name="options">The options to configure the CookieTokenAccessor.</param>
        public AuthenticationOptionsBuilder UseCookiesWithOptions<TCookieAccessor>(CookieAccessorOptions options) where TCookieAccessor : CookieTokenAccessor
        {
            _tokenAccessorType = typeof(TCookieAccessor);
            _cookieAccessorOptions = options;
            return this;
        }

        public AuthenticationOptionsBuilder WithTokenService<TTokenService>() where TTokenService : class, ITokenService
        {
            if (typeof(TTokenService).IsAssignableFrom(typeof(JwtTokenService)))
            {
                throw new InvalidOperationException($"Please use {nameof(WithDefaultTokenService)} instead");
            }
            _tokenServiceType = typeof(TTokenService);
            return this;
        }

        /// <summary>
        /// Configures the authentication to use a custom token service, providing a factory for creating instances of the service.
        /// </summary>
        /// <typeparam name="TTokenService">The type of the custom token service, implementing ITokenService.</typeparam>
        /// <param name="factory">A factory function that resolves the custom token service from the service provider.</param>
        public AuthenticationOptionsBuilder WithTokenService<TTokenService>(Func<IServiceProvider, TTokenService> factory) where TTokenService : class, ITokenService
        {
            _tokenServiceFactory = factory;
            return this;
        }

        /// <summary>
        /// Configures the authentication to use the default JWT token service (JwtTokenService) using a factory.
        /// </summary>
        /// <param name="factory">A factory function that resolves the JwtTokenService from the service provider.</param>
        public AuthenticationOptionsBuilder WithDefaultTokenService(Func<IServiceProvider, JwtTokenService> factory) => WithTokenService(factory);
        
        /// <summary>
        /// Builds the AuthenticationOptions instance based on the configured settings.
        /// </summary>
        /// <returns>An AuthenticationOptions instance configured with the specified settings.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when:
        ///   - Neither UseAuthHeader, UseCookies, nor UseCookiesWithOptions has been called.
        ///   - Neither WithDefaultTokenService nor WithTokenService has been called.
        ///   - WithConfiguration has not been called.
        /// </exception>
        public AuthenticationOptions Build()
        {
            if(_tokenAccessorFactory == null && _tokenAccessorType == null)
            {
                throw new InvalidOperationException($"Either {nameof(UseAuthHeader)}, {nameof(UseCookies)} or {nameof(UseCookiesWithOptions)} must be called");
            }
            var tokenAccessOptions = new TokenAccessOptions(_tokenAccessorType, _cookieAccessorOptions ?? CookieAccessorOptions.Default, _tokenAccessorFactory);
            if(_tokenServiceFactory == null && _tokenServiceType == null)
            {
                throw new InvalidOperationException($"Either {nameof(WithDefaultTokenService)} or {nameof(WithTokenService)} must be called");
            }
            var tokenServiceOptions = new TokenServiceOptions(_tokenServiceType, _tokenServiceFactory);
            if(_configuration == null)
            {
                throw new InvalidOperationException($".{nameof(WithConfiguration)} must be called");
            }
            return new AuthenticationOptions(_configuration, tokenAccessOptions, tokenServiceOptions);
        }
    }
}