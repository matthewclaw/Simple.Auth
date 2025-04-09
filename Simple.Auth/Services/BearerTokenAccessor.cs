using Microsoft.AspNetCore.Http;
using Simple.Auth.Interfaces.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Services
{
    /// <summary>
    /// Abstract base class for accessing and managing Bearer tokens, typically used for authentication.
    /// Provides methods for setting and retrieving tokens from the HTTP context, as well as handling refresh tokens.
    /// </summary>
    public abstract class BearerTokenAccessor : HttpTokenAccessor
    {

        public BearerTokenAccessor(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

        /// <summary>
        /// Sets the bearer token in the Authorization header of the current HTTP response.
        /// </summary>
        /// <param name="token">The bearer token to set.</param>
        public override void SetToken(string token)
        {
            if (HttpContextAccessor.HttpContext != null)
            {
                HttpContextAccessor.HttpContext.Response.Headers["Authorization"] = $"Bearer {token}";
            }
        }


        /// <summary>
        /// Attempts to retrieve the bearer token from the Authorization header of the current HTTP request.
        /// </summary>
        /// <param name="token">When this method returns, contains the token if found; otherwise, an empty string.</param>
        /// <returns>True if a token was found in the Authorization header; otherwise, false.</returns>
        public override bool TryGetToken(out string token)
        {
            var authorizationHeader = HttpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase))
            {
                token = authorizationHeader.Substring("Bearer ".Length).Trim();
                return token?.Length > 0;
            }
            token = string.Empty;
            return false;
        }
    }
}
