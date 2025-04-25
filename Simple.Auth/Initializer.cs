using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Simple.Auth.Builders;
using Simple.Auth.Configuration;
using Simple.Auth.Controllers.Conventions;
using Simple.Auth.Converters;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Interfaces.Stores;
using Simple.Auth.Middleware.Handlers.Authentication;
using Simple.Auth.Middleware.Handlers.Authorization;
using Simple.Auth.Requirements;
using Simple.Auth.Services;
using Simple.Auth.Services.Authentication;
using Simple.Auth.Stores;
using System.Diagnostics.CodeAnalysis;
using AuthenticationOptions = Simple.Auth.Configuration.AuthenticationOptions;

namespace Simple.Auth
{
    [ExcludeFromCodeCoverage]
    public static class Initializer
    {
        public static IServiceCollection AddSimpleAuthentication(this IServiceCollection services, Action<AuthenticationOptionsBuilder> options)
        {
            services.AddCustomJsonConverters();

            AuthenticationOptionsBuilder builder = new AuthenticationOptionsBuilder();
            options?.Invoke(builder);
            var authenticationOptions = builder.Build();
            services.AddSingleton(authenticationOptions);

            services.AddTokenAccessor(authenticationOptions.TokenAccessOptions)
                .AddTokenService(authenticationOptions.TokenServiceOptions);

            services.AddStores();

            services.AddSchemes(authenticationOptions);
            services.AddScoped(typeof(IUserAuthenticator), authenticationOptions.UserAuthenticatorType);
            services.AddScoped<Interfaces.Authentication.IAuthenticationService, Services.AuthenticationService>();

            services.AddCorrelation();
            if (!authenticationOptions.CacheDisabled)
            {
                services.AddScoped<IAuthenticationCache, AuthenticationCache>();
            }
            return services;
        }

        private static IServiceCollection AddCorrelation(this IServiceCollection services)
        {
            services.AddSingleton<ICorrelationLoggerFactory, CorrelationLoggerFactory>();
            services.AddScoped<ICorrelationService, CorrelationService>();
            return services;
        }

        private static IServiceCollection AddCustomJsonConverters(this IServiceCollection services)
        {
            return services.Configure<JsonOptions>(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new ClaimsPrincipalConverter());
                options.JsonSerializerOptions.Converters.Add(new ClaimConverter());
            });
        }

        public static MvcOptions AddSimpleAuthControllers(this MvcOptions mvcOptions, Action<AuthControllerConventionOptionsBuilder> options)
        {
            var builder = new AuthControllerConventionOptionsBuilder();
            options?.Invoke(builder);
            var authControllerConventionOptions = builder.Build();
            mvcOptions.Conventions.Add(new AuthControllerConvention(authControllerConventionOptions));
            return mvcOptions;
        }

        private static IServiceCollection AddSchemes(this IServiceCollection services, AuthenticationOptions options)
        {
            var authBuilder = services.AddAuthentication(options =>
             {
                 options.DefaultScheme = Constants.Schemes.DEFAULT; // Set the default authentication scheme
                 options.DefaultChallengeScheme = Constants.Schemes.DEFAULT; // Set the default challenge scheme
             }).AddScheme<AuthenticationSchemeOptions, SimpleAuthenticationHandler>(Constants.Schemes.DEFAULT, null);
            foreach (var addition in options.SchemeAdditions)
            {
                addition.Invoke(authBuilder);
            }
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Constants.Policies.DEFAULT, policy =>
                {
                    policy.AddAuthenticationSchemes(Constants.Schemes.DEFAULT);
                    policy.Requirements.Add(new SimpleRequirement());
                });
            });
            services.AddScoped<IAuthorizationHandler, RequirementHandler>();
            return services;
        }

        private static IServiceCollection AddStores(this IServiceCollection services)
        {
            //Temporary Singleton
            services.AddSingleton<IRefreshTokenStore, RefreshTokenInMemoryStore>();
            // services.AddScoped<IRefreshTokenStore, RefreshTokenInMemoryStore>();
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