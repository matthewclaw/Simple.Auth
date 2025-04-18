﻿using Simple.Auth.Builders;
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
using Microsoft.AspNetCore.Authentication;
using Simple.Auth.Middleware.Handlers.Authorization;
using Simple.Auth.Middleware.Handlers.Authentication;
using Microsoft.AspNetCore.Mvc;
using Simple.Auth.Controllers.Conventions;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.InteropServices;
using AuthenticationOptions = Simple.Auth.Configuration.AuthenticationOptions;
using System.Diagnostics.CodeAnalysis;

namespace Simple.Auth
{
    [ExcludeFromCodeCoverage]
    public static class Initializer
    {
        public static IServiceCollection AddSimpleAuth(this IServiceCollection services, Action<AuthenticationOptionsBuilder> options)
        {
            AuthenticationOptionsBuilder builder = new AuthenticationOptionsBuilder();
            options?.Invoke(builder);
            var authenticationOptions = builder.Build();

            services.AddSingleton(authenticationOptions);

            services.AddTokenAccessor(authenticationOptions.TokenAccessOptions)
                .AddTokenService(authenticationOptions.TokenServiceOptions);

            services.AddStores();
            services.AddSchemes(authenticationOptions);
            services.AddTransient(typeof(IUserAuthenticator), authenticationOptions.UserAuthenticatorType);
            services.AddSingleton<ICorrelationLoggerFactory, CorrelationLoggerFactory>();
            services.AddScoped<ICorrelationService, CorrelationService>();
            services.AddScoped<Interfaces.Authentication.IAuthenticationService, Services.AuthenticationService>();
            return services;
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