using Azure.Core;
using Microsoft.Extensions.Caching.Distributed;
using Simple.Auth.Converters;
using Simple.Auth.Helpers;
using Simple.Auth.Interfaces.Stores;
using Simple.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Simple.Auth.Services.Authentication
{
    public class AuthenticationCache : IAuthenticationCache
    {
        public const string BLACKLISTED_KEY_PREFIX = "_sa:bl:";
        public const string REFRESH_TOKEN_KEY_PREFIX = "_sa:rt:";
        public const string USER_PRINCIPAL_KEY_PREFIX = "_sa:cp:";
        private readonly DistributedCacheEntryOptions _blacklistOptions;
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _defaultOptions;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        public AuthenticationCache(IDistributedCache cache)
        {
            _cache = cache;
            _defaultOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };
            _blacklistOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30) };
            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.Converters.Add(new ClaimsPrincipalConverter());
        }

        public AuthenticationCacheResults GetPrincipal(string accessToken)
        {
            var hash = accessToken.GenerateBasicHash();
            if (IsBlackListed(hash, out var date))
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
                ClaimsPrincipal principal = JsonSerializer.Deserialize<ClaimsPrincipal>(cachedItem, _jsonSerializerOptions)!;
                return AuthenticationCacheResults.ForPrincipal(principal);
            }
            catch
            {
                return AuthenticationCacheResults.None();
            }
        }

        public AuthenticationCacheResults GetRefreshTokenDetails(string refreshToken)
        {
            var hash = refreshToken.GenerateBasicHash();
            if (IsBlackListed(hash, out var date))
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
                RefreshTokenDetails refreshDetails = JsonSerializer.Deserialize<RefreshTokenDetails>(cachedItem, _jsonSerializerOptions)!;
                return AuthenticationCacheResults.ForRefreshDetails(refreshDetails);
            }
            catch
            {
                return AuthenticationCacheResults.None();
            }
        }

        public void RemoveDetailsAndBlacklistRefreshToken(string refreshToken, DateTime? date = null)
        {
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
            var hash = accessToken.GenerateBasicHash();
            var key = GetPrincipalKey(hash);
            var serialized = Serialize(principal);
            _cache.SetString(key, serialized);
        }

        public void SetRefreshTokenDetails(string refreshToken, RefreshTokenDetails tokenDetails)
        {
            var hash = refreshToken.GenerateBasicHash();
            var key = GetRefreshKey(hash);
            var serialized = Serialize(tokenDetails);
            _cache.SetString(key, serialized);
        }

        private void BlackList(string hash, DateTime date)
        {
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
        public bool IsBlackListed(string hash, out DateTime? date)
        {
            var key = BLACKLISTED_KEY_PREFIX + hash;
            try
            {
                var caheItem = _cache.GetString(key);
                if (string.IsNullOrEmpty(caheItem))
                {
                    date = null;
                    return false;
                }
                date = JsonSerializer.Deserialize<DateTime?>(caheItem);
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
    }
}
