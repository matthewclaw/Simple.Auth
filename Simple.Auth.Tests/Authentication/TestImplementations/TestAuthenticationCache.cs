using Azure.Core;
using Simple.Auth.Helpers;
using Simple.Auth.Interfaces.Stores;
using Simple.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Tests.Authentication.TestImplementations
{
    internal class TestAuthenticationCache : IAuthenticationCache
    {
        private readonly Dictionary<string, ClaimsPrincipal> _principals = new();
        private readonly Dictionary<string, RefreshTokenDetails> _refreshTokens = new();
        private readonly Dictionary<string, DateTime> _blacklist = new();
        public AuthenticationCacheResults GetPrincipal(string accessToken)
        {
            var hash = accessToken.GenerateBasicHash();
            if (_blacklist.ContainsKey(hash))
            {
                return AuthenticationCacheResults.Blacklisted(_blacklist[hash]);
            }
            if (_principals.ContainsKey(hash))
            {
                return AuthenticationCacheResults.ForPrincipal(_principals[hash]);
            }
            return AuthenticationCacheResults.None();
        }

        public AuthenticationCacheResults GetRefreshTokenDetails(string refreshToken)
        {
            var hash = refreshToken.GenerateBasicHash();
            if (_blacklist.ContainsKey(hash))
            {
                return AuthenticationCacheResults.Blacklisted(_blacklist[hash]);
            }
            if (_refreshTokens.ContainsKey(hash))
            {
                return AuthenticationCacheResults.ForRefreshDetails(_refreshTokens[hash]);
            }
            return AuthenticationCacheResults.None();
        }

        public void RemoveDetailsAndBlacklistRefreshToken(string refreshToken, DateTime? date = null)
        {
            var hash = refreshToken.GenerateBasicHash();

            _refreshTokens.Remove(hash);
            _blacklist.Add(hash, date ?? DateTime.UtcNow);
        }

        public void RemovePrincipalAndBlacklistToken(string accessToken, DateTime? date = null)
        {
            var hash = accessToken.GenerateBasicHash();
            _principals.Remove(hash);
            _blacklist.Add(hash, date ?? DateTime.UtcNow);
        }

        public void SetPrincipal(string accessToken, ClaimsPrincipal principal)
        {
            var hash = accessToken.GenerateBasicHash();
            _principals[hash] = principal;
        }

        public void SetRefreshTokenDetails(string refreshToken, RefreshTokenDetails tokenDetails)
        {
            var hash = refreshToken.GenerateBasicHash();
            _refreshTokens[hash] = tokenDetails;
        }
    }
}
