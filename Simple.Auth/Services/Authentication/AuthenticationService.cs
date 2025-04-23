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
        protected readonly ICorrelationLogger Logger;
        protected readonly IAuthenticationCache? Cache;
        protected virtual TimeSpan RefreshTokenLifeSpan => TimeSpan.FromDays(7);
        public AuthenticationService(IHttpContextAccessor httpContextAccessor,
    HttpTokenAccessor tokenAccessor, ITokenService tokenService, IRefreshTokenStore refreshTokenStore,
    ICorrelationService correlationService, IUserAuthenticator userAuthenticator, ICorrelationLoggerFactory loggerFactory)
            : this(httpContextAccessor, tokenAccessor, tokenService, refreshTokenStore, correlationService, userAuthenticator, loggerFactory, null)
        {

        }
        public AuthenticationService(IHttpContextAccessor httpContextAccessor,
            HttpTokenAccessor tokenAccessor, ITokenService tokenService, IRefreshTokenStore refreshTokenStore,
            ICorrelationService correlationService, IUserAuthenticator userAuthenticator, ICorrelationLoggerFactory loggerFactory, IAuthenticationCache? cache)
        {
            TokenAccessor = tokenAccessor;
            TokenService = tokenService;
            HttpContextAccessor = httpContextAccessor;
            RefreshTokenStore = refreshTokenStore;
            CorrelationService = correlationService;
            UserAuthenticator = userAuthenticator;
            Logger = loggerFactory.CreateLogger<AuthenticationService>();
            Cache = cache;
        }

        public async Task<SessionState> GetSessionStateAsync()
        {
            string token = string.Empty;
            string refresh = string.Empty;
            bool hasToken = TokenAccessor.TryGetToken(out token);
            bool hasRefresh = TokenAccessor.TryGetRefreshToken(out refresh);
            if (!hasToken && !hasRefresh)
            {
                return SessionState.None;
            }
            if(IsTokenBlacklisted(token, out var accessBlacklistedOn))
            {
                Logger.LogWarning("Access token was blacklisted on {blacklistedOn}", accessBlacklistedOn);
                return SessionState.Invalid;
            }
            if (await TokenService.ValidateTokenAsync(token) && hasRefresh)
            {
                Logger.LogDebug("Session is valid");
                return SessionState.Valid;
            }
            if (IsTokenBlacklisted(refresh, out var refreshBlacklistedOn))
            {
                Logger.LogWarning("Refresh token was blacklisted on {blacklistedOn}", refreshBlacklistedOn);
                return SessionState.Invalid;
            }
            if (!await ValidateRefreshTokenAsync(refresh))
            {
                Logger.LogWarning("Could not refresh session");
                return SessionState.Invalid;
            }
            return SessionState.RefreshValid;
        }
        private bool IsTokenBlacklisted(string token, out DateTime? date)
        {
            if (Cache == null)
            {
                date = null;
                return false;
            }
            return Cache.IsBlacklisted(token, out date);
        }
        public async Task<(string accessToken, string refreshToken, ClaimsPrincipal principal)> StartSessionAsync(object request)
        {
            var userAuthResult = await UserAuthenticator.AuthenticateUserAsync(request);
            if (!userAuthResult.Succeeded)
            {
                throw new ArgumentException("Invalid sign-in details");
            }
            var newCorrelation = CorrelationService.GenerateCorrelationId();
            CorrelationService.SetCorrelationId(newCorrelation);
            var token = TokenService.GenerateToken(userAuthResult.Principal!.Claims, TimeSpan.FromMinutes(30));
            var refresh = TokenService.GenerateRefreshToken();
            TokenAccessor.SetToken(token);
            TokenAccessor.SetRefreshToken(refresh);
            Cache?.SetPrincipal(token, userAuthResult.Principal);
            await StoreRefreshTokenAsync(refresh);
            return (token, refresh, userAuthResult.Principal!);
        }

        public async Task<bool> TryRefreshAccessAsync()
        {
            var state = await GetSessionStateAsync();
            if (state == SessionState.Invalid || state == SessionState.None)
            {
                return false;
            }
            TokenAccessor.TryGetRefreshToken(out var oldRefresh);
            TokenAccessor.TryGetToken(out var oldAccess);
            try
            {
                var (newAccess, newRefresh) = await TokenService.RefreshTokenAsync(oldAccess, oldRefresh);
                TokenAccessor.SetToken(newAccess);
                TokenAccessor.SetRefreshToken(newRefresh);
                await StoreRefreshTokenAsync(newRefresh);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                Cache?.RemoveDetailsAndBlacklistRefreshToken(oldRefresh);
                Cache?.RemovePrincipalAndBlacklistToken(oldAccess);
            }
        }

        private async Task StoreRefreshTokenAsync(string refreshToken)
        {
            var clientIp = HttpContextAccessor.GetClientIpAddress();
            var expirey = DateTimeOffset.UtcNow + RefreshTokenLifeSpan;
            var refreshTokenDetails = new RefreshTokenDetails(refreshToken, clientIp, expirey);
            await RefreshTokenStore.InsertAsync(refreshTokenDetails);
            Cache?.SetRefreshTokenDetails(refreshToken, refreshTokenDetails);
        }

        private async Task<bool> ValidateRefreshTokenAsync(string refresh)
        {
            RefreshTokenDetails? storedToken = null;
            var cached = Cache?.GetRefreshTokenDetails(refresh) ?? AuthenticationCacheResults.None();
            if (cached.CacheType == AuthenticationCacheType.BlackListed)
            {
                Logger.LogWarning("Refresh token was blacklisted on {blacklistedDate}", cached.BlacklistedOn);
                return false;
            }
            if (cached.Found)
            {
                storedToken = cached.RefreshTokenDetails!;
            }
            else
            {
                storedToken = await RefreshTokenStore.GetAsync(refresh);
            }
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
            if (!this.TokenAccessor.TryGetToken(out var token))
            {
                return DateTimeOffset.MinValue;
            }
            return TokenService.GetTokenExpiry(token);
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string accessToken)
        {
            var cached = Cache?.GetPrincipal(accessToken) ?? AuthenticationCacheResults.None();
            switch (cached.CacheType)
            {
                case AuthenticationCacheType.None:
                    return await UserAuthenticator.AuthenticateUserAsync(accessToken);
                case AuthenticationCacheType.Principal:
                    return AuthenticationResult.Success(cached.ClaimsPrincipal!);
                case AuthenticationCacheType.BlackListed:
                    Logger.LogWarning("Access Token was blacklisted on {blacklistDate}", cached.BlacklistedOn);
                    return AuthenticationResult.Failure($"Access Token was blacklisted on {cached.BlacklistedOn}");
            }

            throw new InvalidOperationException();
        }

        public async Task<AuthenticationResult> AuthenticateAsync()
        {
            if (!this.TokenAccessor.TryGetToken(out var token))
            {
                return AuthenticationResult.Failure("No Access Token");
            }
            return await AuthenticateAsync(token);
        }

        public async Task EndSessionAsync()
        {
            var refresh = "";
            var access = "";
            var hasRefresh = TokenAccessor.TryGetRefreshToken(out refresh);
            var hasAccess = TokenAccessor.TryGetToken(out access);
            if (!hasAccess && !hasRefresh)
            {
                return;
            }
            if (hasRefresh)
            {
                await RefreshTokenStore.BlacklistAsync(refresh);
                Cache?.RemoveDetailsAndBlacklistRefreshToken(refresh);
            }
            if (hasAccess)
            {
                Cache?.RemovePrincipalAndBlacklistToken(access);
            }
            TokenAccessor.RemoveTokens();
        }
    }
}