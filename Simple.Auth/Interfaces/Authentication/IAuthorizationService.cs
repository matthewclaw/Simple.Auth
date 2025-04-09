using Microsoft.AspNetCore.Http;
using Simple.Auth.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Interfaces.Authentication
{
    public interface IAuthorizationService
    {
        Task<bool> TryRefreshAccessAsync();
        Task<IEnumerable<Claim>> GetClaimsAsync();
        Task<(string accessToken, string refreshToken)> StartSessionAsync();
        Task<SessionState> GetSessionStateAsync();
        void ForContext(HttpContext context);

        void ForDefaultContext();
    }
}
