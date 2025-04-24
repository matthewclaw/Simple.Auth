using Simple.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Interfaces.Authentication
{
    public interface IUserAuthenticator
    {
        
        /// <summary>
        /// Authenticates a user asynchronously based on the provided request.
        /// </summary>
        /// <param name="request">The authentication request object.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the authentication result.</returns>
        Task<AuthenticationResult> AuthenticateUserAsync(object request);

        /// <summary>
        /// Uses the access token to retrieve user details
        /// </summary>
        /// <param name="accessToken">The access token present in the Request</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the authentication result.</returns>
        Task<AuthenticationResult> AuthenticateUserAsync(string accessToken);
    }
}
