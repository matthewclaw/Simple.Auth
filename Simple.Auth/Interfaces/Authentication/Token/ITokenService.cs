using System.Security.Claims;

namespace Simple.Auth.Interfaces.Authentication
{
    public interface ITokenService
    {
        string GenerateToken(IEnumerable<Claim> claims, TimeSpan expiry);

        string GenerateRefreshToken();

        Task<bool> ValidateTokenAsync(string token);

        Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string accessToken, string refreshToken);

        DateTimeOffset GetTokenExpiry(string token);
    }
}