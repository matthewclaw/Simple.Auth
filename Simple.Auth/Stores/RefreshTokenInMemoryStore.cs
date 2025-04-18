using Simple.Auth.Interfaces.Stores;
using Simple.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Stores
{
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
        {
            var token = new RefreshTokenDetails(refreshToken, ipAddress, expiry);
            _refreshTokens[refreshToken] = token;
            return await Task.FromResult(true);
        }
    }
}
