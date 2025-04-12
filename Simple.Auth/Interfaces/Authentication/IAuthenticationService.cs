using Microsoft.AspNetCore.Http;
using Simple.Auth.Enums;
using Simple.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Interfaces.Authentication
{
    public interface IAuthenticationService : IHttpContextSwitchable
    {
        Task<bool> TryRefreshAccessAsync();
        Task<(string accessToken, string refreshToken)> StartSessionAsync(object request);
        Task<AuthenticationResult> AuthenticateAsync(string accessToken);
        Task<AuthenticationResult> AuthenticateAsync();
        Task<SessionState> GetSessionStateAsync();
        DateTimeOffset GetTokenExpiry();
    }
}
