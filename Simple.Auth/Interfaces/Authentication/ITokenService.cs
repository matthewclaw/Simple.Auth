using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Interfaces.Authentication
{
    public interface ITokenService
    {
        string GenerateToken(IEnumerable<Claim> claims, TimeSpan expiry);
        string GenerateToken(TimeSpan expiry);
        string GenerateToken();
        IEnumerable<Claim> GetClaims();
        string GenerateRefreshToken();
        Task<bool> ValidateTokenAsync(string token);
        Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string accessToken, string refreshToken);
        DateTimeOffset GetTokenExpiry(string token);
    }
}
