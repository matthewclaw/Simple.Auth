using Microsoft.Extensions.DependencyInjection;
using Simple.Auth.Builders;
using Simple.Auth.Configuration;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth
{
    public static class Initializer
    {
        public static IServiceCollection AddSimpleAuth(this IServiceCollection services, Action<AuthenticationOptionsBuilder> options)
        {
            AuthenticationOptionsBuilder builder = new AuthenticationOptionsBuilder();
            options?.Invoke(builder);
            AuthenticationOptions authenticationOptions = builder.Build();
            return services.AddTokenAccessor(authenticationOptions.TokenAccessOptions)
                .AddTokenService(authenticationOptions.TokenServiceOptions);

        }
        private static IServiceCollection AddTokenAccessor(this IServiceCollection services, TokenAccessOptions options)
        {
            if (options.ImplementationFactory != null)
            {
                services.AddScoped<ITokenAccessor>(options.ImplementationFactory);
                return services;
            }
            if (typeof(CookieTokenAccessor).IsAssignableFrom(options.TokenAccessorType))
            {
                services.AddScoped(services => options.CookieAccessorOptions!);
            }
            services.AddScoped(typeof(ITokenAccessor), options.TokenAccessorType!);
            return services;
        }
        private static IServiceCollection AddTokenService(this IServiceCollection services, TokenServiceOptions options)
        {
            if (options.ImplementationFactory != null)
            {
                services.AddScoped<ITokenService>(options.ImplementationFactory);
                return services;
            }
            services.AddScoped(typeof(ITokenService), options.ServiceType!);
            return services;
        }
    }
}
