using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Simple.Auth.Helpers;
using Simple.Auth.Interfaces.Stores;
using Simple.Auth.Models;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Simple.Auth.Services.Authentication
{
    public class AuthenticationCache : IAuthenticationCache
    {
        public const string BLACKLISTED_KEY_PREFIX = "_sa:bl:";
        public const string REFRESH_TOKEN_KEY_PREFIX = "_sa:rt:";
        public const string USER_PRINCIPAL_KEY_PREFIX = "_sa:cp:";
        private readonly DistributedCacheEntryOptions _blacklistOptions;
        private readonly IDistributedCache? _cache;
        private readonly DistributedCacheEntryOptions _defaultOptions;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        [ExcludeFromCodeCoverage]
        public AuthenticationCache(IOptions<JsonOptions> jsonOptions) : this(null, jsonOptions)
        {
        }

        public AuthenticationCache(IDistributedCache? cache, IOptions<JsonOptions> jsonOptions)
        {
            _cache = cache;
            _defaultOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };
            _blacklistOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30) };
            _jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
        }

        public AuthenticationCacheResults GetPrincipal(string accessToken)
        {
            if (_cache == null)
            {
                return AuthenticationCacheResults.None();
            }
            var hash = accessToken.GenerateBasicHash();
            if (IsHashBlackListed(hash, out var date))
            {
                return AuthenticationCacheResults.Blacklisted(date!.Value);
            }
            var key = GetPrincipalKey(hash);
            try
            {
                var cachedItem = _cache.GetString(key);
                if (string.IsNullOrEmpty(cachedItem))
                {
                    return AuthenticationCacheResults.None();
                }
                ClaimsPrincipal principal = Deserialize<ClaimsPrincipal>(cachedItem)!;
                return AuthenticationCacheResults.ForPrincipal(principal);
            }
            catch
            {
                return AuthenticationCacheResults.None();
            }
        }

        public AuthenticationCacheResults GetRefreshTokenDetails(string refreshToken)
        {
            if (_cache == null)
            {
                return AuthenticationCacheResults.None();
            }
            var hash = refreshToken.GenerateBasicHash();
            if (IsHashBlackListed(hash, out var date))
            {
                return AuthenticationCacheResults.Blacklisted(date!.Value);
            }
            var key = GetRefreshKey(hash);
            try
            {
                var cachedItem = _cache.GetString(key);
                if (string.IsNullOrEmpty(cachedItem))
                {
                    return AuthenticationCacheResults.None();
                }
                RefreshTokenDetails refreshDetails = Deserialize<RefreshTokenDetails>(cachedItem)!;
                return AuthenticationCacheResults.ForRefreshDetails(refreshDetails);
            }
            catch
            {
                return AuthenticationCacheResults.None();
            }
        }

        public void RemoveDetailsAndBlacklistRefreshToken(string refreshToken, DateTime? date = null)
        {
            if (_cache == null)
            {
                return;
            }
            var hash = refreshToken.GenerateBasicHash();
            if (date == null)
            {
                date = DateTime.UtcNow;
            }
            BlackList(hash, date!.Value);
            _cache.Remove(GetRefreshKey(hash));
        }

        public void RemovePrincipalAndBlacklistToken(string accessToken, DateTime? date = null)
        {
            if (_cache == null)
            {
                return;
            }
            var hash = accessToken.GenerateBasicHash();
            if (date == null)
            {
                date = DateTime.UtcNow;
            }
            BlackList(hash, date!.Value);
            _cache.Remove(GetPrincipalKey(hash));
        }

        public void SetPrincipal(string accessToken, ClaimsPrincipal principal)
        {
            if (_cache == null)
            {
                return;
            }
            var hash = accessToken.GenerateBasicHash();
            var key = GetPrincipalKey(hash);
            var serialized = Serialize(principal);
            _cache.SetString(key, serialized, _defaultOptions);
        }

        public void SetRefreshTokenDetails(string refreshToken, RefreshTokenDetails tokenDetails)
        {
            if (_cache == null)
            {
                return;
            }
            var hash = refreshToken.GenerateBasicHash();
            var key = GetRefreshKey(hash);
            var serialized = Serialize(tokenDetails);
            _cache.SetString(key, serialized, _defaultOptions);
        }

        private void BlackList(string hash, DateTime date)
        {
            if (_cache == null)
            {
                return;
            }
            var key = BLACKLISTED_KEY_PREFIX + hash;
            var serialized = Serialize(date);
            _cache.SetString(key, serialized);
        }

        private string GetPrincipalKey(string accessTokenHash)
        {
            return USER_PRINCIPAL_KEY_PREFIX + accessTokenHash;
        }

        private string GetRefreshKey(string refreshTokenHash)
        {
            return REFRESH_TOKEN_KEY_PREFIX + refreshTokenHash;
        }

        public bool IsHashBlackListed(string hash, out DateTime? date)
        {
            if (_cache == null)
            {
                date = null;
                return false;
            }
            var key = BLACKLISTED_KEY_PREFIX + hash;
            try
            {
                var caheItem = _cache.GetString(key);
                if (string.IsNullOrEmpty(caheItem))
                {
                    date = null;
                    return false;
                }
                date = Deserialize<DateTime?>(caheItem);
                return true;
            }
            catch
            {
                date = null;
                return false;
            }
        }

        private string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, _jsonSerializerOptions);
        }

        private T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
        }

        public bool IsBlacklisted(string token, out DateTime? date)
        {
            var hash = token.GenerateBasicHash();
            return IsHashBlackListed(hash, out date);
        }
    }
}