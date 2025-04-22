using Simple.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Interfaces.Stores
{
    public interface IRefreshTokenStore
    {
        Task BlacklistAsync(string refreshToken);
        Task<bool> InsertAsync(string refreshToken, string ipAddress, DateTimeOffset expiry);
        Task<bool> InsertAsync(RefreshTokenDetails details);
        Task<RefreshTokenDetails> GetAsync(string refreshToken);
    }
}
