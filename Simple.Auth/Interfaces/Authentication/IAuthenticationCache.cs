using Simple.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
    }
}
