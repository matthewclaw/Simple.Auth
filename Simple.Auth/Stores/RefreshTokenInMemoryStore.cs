using Simple.Auth.Interfaces.Stores;
using Simple.Auth.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Stores
{
    [ExcludeFromCodeCoverage]
    public class RefreshTokenInMemoryStore : IRefreshTokenStore
    {
        private Dictionary<string, RefreshTokenDetails> _refreshTokens = new Dictionary<string, RefreshTokenDetails>();
        public async Task BlacklistAsync(string refreshToken)
        {
            var storedToken = await GetAsync(refreshToken);
            if (storedToken == null)
            {
                return;
            }
            storedToken.Expiry = DateTimeOffset.MinValue;
            _refreshTokens[refreshToken] = storedToken;
        }

        public async Task<RefreshTokenDetails> GetAsync(string refreshToken)
        {
            if(_refreshTokens.TryGetValue(refreshToken, out var storedToken))
            {
                return await Task.FromResult(storedToken);
            }
            return null;
        }

        public async Task<bool> InsertAsync(string refreshToken, string ipAddress, DateTimeOffset expiry)
        =>await InsertAsync(new RefreshTokenDetails(refreshToken, ipAddress, expiry));

        public async Task<bool> InsertAsync(RefreshTokenDetails details)
        {
            _refreshTokens[details.Token] = details;
            return await Task.FromResult(true);
        }
    }
}
