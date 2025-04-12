using Microsoft.AspNetCore.Http;
using Simple.Auth.Enums;
using Simple.Auth.Helpers;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Interfaces.Stores;
using Simple.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        protected readonly IHttpContextAccessor HttpContextAccessor;
        protected readonly IRefreshTokenStore RefreshTokenStore;
        public readonly HttpTokenAccessor TokenAccessor;
        protected readonly ITokenService TokenService;
        protected readonly ICorrelationService CorrelationService;
        protected readonly IUserAuthenticator UserAuthenticator;
        protected virtual TimeSpan RefreshTokenLifeSpan => TimeSpan.FromDays(7);

        public AuthenticationService(IHttpContextAccessor httpContextAccessor, 
            HttpTokenAccessor tokenAccessor, ITokenService tokenService, IRefreshTokenStore refreshTokenStore,
            ICorrelationService correlationService, IUserAuthenticator userAuthenticator)
        {
            TokenAccessor = tokenAccessor;
            TokenService = tokenService;
            HttpContextAccessor = httpContextAccessor;
            RefreshTokenStore = refreshTokenStore;
            CorrelationService = correlationService;
            UserAuthenticator = userAuthenticator;
        }

        public async Task<SessionState> GetSessionStateAsync()
        {
            string token = string.Empty;
            string refresh = string.Empty;
            if (!TokenAccessor.TryGetToken(out token) && !TokenAccessor.TryGetRefreshToken(out refresh))
            {
                return SessionState.None;
            }
            if (await TokenService.ValidateTokenAsync(token) && !string.IsNullOrEmpty(refresh))
            {
                return SessionState.Valid;
            }
            if (!await ValidateRefreshTokenAsync(refresh))
            {
                return SessionState.Invalid;
            }
            return SessionState.RefreshValid;
        }

        public async Task<(string accessToken, string refreshToken)> StartSessionAsync(object request)
        {
            var userAuthResult = await UserAuthenticator.AuthenticateUserAsync(request);
            if (!userAuthResult.Succeeded)
            {
                return ("", "");
            }
            var token = TokenService.GenerateToken(userAuthResult.Principal!.Claims, TimeSpan.FromMinutes(30));
            var refresh = TokenService.GenerateRefreshToken();
            TokenAccessor.SetToken(token);
            TokenAccessor.SetRefreshToken(refresh);
            await StoreRefreshTokenAsync(refresh);
            return (token, refresh);
        }

        public async Task<bool> TryRefreshAccessAsync()
        {
            var state = await GetSessionStateAsync();
            if (state == SessionState.Invalid || state == SessionState.None)
            {
                return false;
            }
            TokenAccessor.TryGetRefreshToken(out var refresh);
            TokenAccessor.TryGetToken(out var token);
            try
            {
                var (newAccess, newRefresh) = await TokenService.RefreshTokenAsync(token, refresh);
                TokenAccessor.SetToken(newAccess);
                TokenAccessor.SetRefreshToken(newRefresh);
                await StoreRefreshTokenAsync(newRefresh);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task StoreRefreshTokenAsync(string refreshToken)
        {
            var clientIp = HttpContextAccessor.GetClientIpAddress();
            var expirey = DateTimeOffset.UtcNow + RefreshTokenLifeSpan;
            await RefreshTokenStore.InsertAsync(refreshToken, clientIp, expirey);
        }

        private async Task<bool> ValidateRefreshTokenAsync(string refresh)
        {
            var storedToken = await RefreshTokenStore.GetAsync(refresh);
            if (storedToken == null)
            {
                return false;
            }
            if (storedToken.Expiry < DateTimeOffset.UtcNow)
            {
                return false;
            }
            var clientIp = HttpContextAccessor.GetClientIpAddress();
            return storedToken.IpAddress == clientIp;
        }

        public void ForContext(HttpContext context)
        {
            TokenAccessor.ForContext(context);
        }

        public void ForDefaultContext()
        {
            TokenAccessor.ForDefaultContext();
        }

        public DateTimeOffset GetTokenExpiry()
        {
            if(!this.TokenAccessor.TryGetToken(out var token))
            {
                return DateTimeOffset.MinValue;
            }
            return TokenService.GetTokenExpiry(token);
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string accessToken)
        {
            return await UserAuthenticator.AuthenticateUserAsync(accessToken);
        }

        public async Task<AuthenticationResult> AuthenticateAsync()
        {
            if (!this.TokenAccessor.TryGetToken(out var token))
            {
                return AuthenticationResult.Failure("No Access Token");
            }
            return await AuthenticateAsync(token);
        }
    }
}