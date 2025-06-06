﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Simple.Auth.Interfaces;
using System.Text.Encodings.Web;

namespace Simple.Auth.Middleware.Handlers.Authentication
{
    public class SimpleAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly Interfaces.Authentication.IAuthenticationService _athenticationService;
        private readonly ICorrelationService _correlationService;
        private readonly TimeSpan _refreshThreshold;
        private readonly ICorrelationLogger _logger;
        public SimpleAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ICorrelationLoggerFactory loggerFactory, UrlEncoder encoder, Interfaces.Authentication.IAuthenticationService authorizationService, ICorrelationService correlationService) : base(options, loggerFactory, encoder)
        {
            _athenticationService = authorizationService;
            _correlationService = correlationService;
            _logger = loggerFactory.CreateLogger<SimpleAuthenticationHandler>();
            _refreshThreshold = TimeSpan.FromMinutes(5);
        }

        private async Task<bool> CheckForAutomaticRefreshAsync()
        {
            var tokenExpiry = _athenticationService.GetTokenExpiry();
            var timeLeft = tokenExpiry - DateTimeOffset.UtcNow;
            if (timeLeft > _refreshThreshold)
            {
                return true;
            }
            _logger.LogInformation("Refreshing token");
            return await _athenticationService.TryRefreshAccessAsync();
        }
        private bool CheckForBypass()
        {
            var request = Context.Request;
            if (request.Path.Value?.Contains("swagger") ?? false)
            {
                return true;
            }
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
            {
                return true;
            }
            return false;
        }
        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (CheckForBypass())
            {
                return AuthenticateResult.NoResult();
            }
            try
            {
                string errorMessage;
                string correlationId = _correlationService.GetCorrelationId() ?? _correlationService.GenerateCorrelationId();
                bool stateRefreshed;
                _correlationService.SetCorrelationId(correlationId);
                var sessionState = await _athenticationService.GetSessionStateAsync();
                switch (sessionState)
                {
                    case Enums.SessionState.Valid:
                        stateRefreshed = await CheckForAutomaticRefreshAsync();
                        break;

                    case Enums.SessionState.RefreshValid:
                        stateRefreshed = await _athenticationService.TryRefreshAccessAsync();
                        break;

                    default:
                    case Enums.SessionState.None:
                    case Enums.SessionState.Invalid:
                        errorMessage = "Unauthorized: No Session or Session expired";
                        _logger.LogWarning(errorMessage);
                        return AuthenticateResult.Fail(errorMessage);
                }
                if (!stateRefreshed)
                {
                    errorMessage = "Unauthorized: Session could not be refreshed";
                    _logger.LogWarning(errorMessage);
                    return AuthenticateResult.Fail(errorMessage);
                }
                var userAuthResult = await _athenticationService.AuthenticateAsync();
                var ticket = new AuthenticationTicket(userAuthResult.Principal!, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return AuthenticateResult.Fail(ex);
            }
        }
    }
}