using Simple.Auth.Models;

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