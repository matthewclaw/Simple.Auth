using Simple.Auth.Interfaces.Authentication;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Simple.Auth.Services
{
    [ExcludeFromCodeCoverage]
    public class JwtTokenService : TokenServiceBase
    {
        protected readonly string SecretKey;
        protected readonly string Issuer;
        protected readonly string Audience;

        public JwtTokenService(string secretKey, string issuer, string audience)
        {
            SecretKey = secretKey;
            Issuer = issuer;
            Audience = audience;
        }

        public override string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public override string GenerateToken(IEnumerable<Claim> claims, TimeSpan expiry)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                Issuer,
                Audience,
                claims,
                expires: DateTime.UtcNow.Add(expiry),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public override async Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);
            var newAccessToken = GenerateToken(principal.Claims, TimeSpan.FromMinutes(20));
            var newRefreshToken = GenerateRefreshToken();
            return await Task.FromResult((newAccessToken, newRefreshToken));
        }

        public override async Task<bool> ValidateTokenAsync(string token)
        {
            return await Task.FromResult(ValidateToken(token, true, out _));
        }
        private bool ValidateToken(string token, bool validateLifetime, out ClaimsPrincipal? principal)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.Zero,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                principal = null;
                return false;
            }
        }

        public override DateTimeOffset GetTokenExpiry(string token)
        {
            if (!ValidateToken(token, true, out var principal))
            {
                return DateTimeOffset.MinValue;
            }
            var expiryClaim = principal!.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if (!long.TryParse(expiryClaim, out var ticks))
            {
                return DateTimeOffset.MinValue;
            }
            return DateTimeOffset.FromUnixTimeSeconds(ticks);
        }

        protected ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            if (!ValidateToken(token, false, out var principal))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return principal!;
        }
    }
}
