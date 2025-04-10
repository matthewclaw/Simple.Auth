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
    public class AuthorizationService : IAuthorizationService
    {
        protected readonly IHttpContextAccessor HttpContextAccessor;
        protected readonly IRefreshTokenStore RefreshTokenStore;
        public readonly HttpTokenAccessor TokenAccessor;
        protected readonly ITokenService TokenService;
        protected readonly ICorrelationService CorrelationService;
        protected virtual TimeSpan RefreshTokenLifeSpan => TimeSpan.FromDays(7);

        public AuthorizationService(IHttpContextAccessor httpContextAccessor, 
            HttpTokenAccessor tokenAccessor, ITokenService tokenService, IRefreshTokenStore refreshTokenStore,
            ICorrelationService correlationService)
        {
            TokenAccessor = tokenAccessor;
            TokenService = tokenService;
            HttpContextAccessor = httpContextAccessor;
            RefreshTokenStore = refreshTokenStore;
            CorrelationService = correlationService;
        }

        public virtual async Task<IEnumerable<Claim>> GetClaimsAsync()
        {
            return await Task.FromResult(Array.Empty<Claim>());
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

        public async Task<(string accessToken, string refreshToken)> StartSessionAsync()
        {
            var claims = await GetClaimsAsync();
            var token = TokenService.GenerateToken(claims, TimeSpan.FromMinutes(30));
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
    }
}