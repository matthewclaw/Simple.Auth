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

        public abstract DateTimeOffset GetTokenExpiry(string token);

        public abstract Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string accessToken, string refreshToken);

        public abstract Task<bool> ValidateTokenAsync(string token);
    }
}
