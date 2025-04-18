using Simple.Auth.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Tests.Authentication.Fakes
{
    [ExcludeFromCodeCoverage]
    internal class FakeTokenService : TokenServiceBase
    {
        public override string GenerateRefreshToken()
        {
            throw new NotImplementedException();
        }

        public override string GenerateToken(IEnumerable<Claim> claims, TimeSpan expiry)
        {
            throw new NotImplementedException();
        }

        public override DateTimeOffset GetTokenExpiry(string token)
        {
            throw new NotImplementedException();
        }

        public override Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> ValidateTokenAsync(string token)
        {
            throw new NotImplementedException();
        }
    }
}
