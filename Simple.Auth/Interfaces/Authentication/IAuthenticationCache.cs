using Simple.Auth.Models;
using System.Security.Claims;

namespace Simple.Auth.Interfaces.Stores
{
    public interface IAuthenticationCache
    {
        AuthenticationCacheResults GetPrincipal(string accessToken);

        AuthenticationCacheResults GetRefreshTokenDetails(string refreshToken);

        void RemoveDetailsAndBlacklistRefreshToken(string refreshToken, DateTime? date = null);

        void RemovePrincipalAndBlacklistToken(string accessToken, DateTime? date = null);

        void SetPrincipal(string accessToken, ClaimsPrincipal principal);

        void SetRefreshTokenDetails(string refreshToken, RefreshTokenDetails tokenDetails);

        bool IsBlacklisted(string token, out DateTime? date);
    }
}