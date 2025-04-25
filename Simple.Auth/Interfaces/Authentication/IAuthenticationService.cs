using Simple.Auth.Enums;
using Simple.Auth.Models;
using System.Security.Claims;

namespace Simple.Auth.Interfaces.Authentication
{
    public interface IAuthenticationService : IHttpContextSwitchable
    {
        Task<bool> TryRefreshAccessAsync();

        Task<(string accessToken, string refreshToken, ClaimsPrincipal principal)> StartSessionAsync(object request);

        Task EndSessionAsync();

        Task<AuthenticationResult> AuthenticateAsync(string accessToken);

        Task<AuthenticationResult> AuthenticateAsync();

        Task<SessionState> GetSessionStateAsync();

        DateTimeOffset GetTokenExpiry();
    }
}