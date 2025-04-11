
using Simple.Auth.Builders;
using Simple.Auth.Configuration;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Simple.Auth.Middleware;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Stores;
using Simple.Auth.Stores;
using Simple.Auth.Requirements;
using Microsoft.Extensions.DependencyInjection;
namespace Simple.Auth
{
    public static class Initializer
    {
        public static IServiceCollection AddSimpleAuth(this IServiceCollection services, Action<AuthenticationOptionsBuilder> options)
        {
            AuthenticationOptionsBuilder builder = new AuthenticationOptionsBuilder();
            options?.Invoke(builder);
            AuthenticationOptions authenticationOptions = builder.Build();

            services.AddTokenAccessor(authenticationOptions.TokenAccessOptions)
                .AddTokenService(authenticationOptions.TokenServiceOptions);

            services.AddStores();
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Constants.Policies.DEFAULT, policy =>
                policy.Requirements.Add(new SimpleRequirement()));
            });

            services.AddSingleton<ICorrelationLoggerFactory, CorrelationLoggerFactory>();
            services.AddScoped<ICorrelationService, CorrelationService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();

            return services;

        }

        //private static IServiceCollection AddPolicies(this IServiceCollection services)
        //{

        //    services.AddAuthorization( options =>
        //    {
        //        options.AddPolicy(Constants.Policies.DEFAULT, policy =>
        //        policy.Requirements.Add(new SimpleRequirement()));
        //    });
        //    return services;
        //}

        private static IServiceCollection AddStores(this IServiceCollection services)
        {
            services.AddScoped<IRefreshTokenStore, RefreshTokenInMemoryStore>();
            return services;
        }
        private static IServiceCollection AddTokenAccessor(this IServiceCollection services, TokenAccessOptions options)
        {
            if (options.ImplementationFactory != null)
            {
                services.AddScoped<HttpTokenAccessor>(options.ImplementationFactory);
                return services;
            }
            if (typeof(CookieTokenAccessor).IsAssignableFrom(options.TokenAccessorType))
            {
                services.AddScoped(services => options.CookieAccessorOptions!);
            }
            services.AddScoped(typeof(HttpTokenAccessor), options.TokenAccessorType!);
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
