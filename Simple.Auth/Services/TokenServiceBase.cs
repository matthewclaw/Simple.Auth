using Simple.Auth.Interfaces.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Services
{
    [ExcludeFromCodeCoverage]
    public abstract class TokenServiceBase : ITokenService
    {
        public abstract string GenerateRefreshToken();

        public abstract string GenerateToken(IEnumerable<Claim> claims, TimeSpan expiry);

        public virtual string GenerateToken(TimeSpan expiry)
        {
            return GenerateToken(GetClaims(), expiry);
        }

        public virtual string GenerateToken() => GenerateToken(TimeSpan.FromHours(1));

        public virtual IEnumerable<Claim> GetClaims() => Array.Empty<Claim>();

        public abstract DateTimeOffset GetTokenExpiry(string token);

        public abstract Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string accessToken, string refreshToken);

        public abstract Task<bool> ValidateTokenAsync(string token);
    }
}
