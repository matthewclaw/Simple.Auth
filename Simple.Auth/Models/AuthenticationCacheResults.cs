using Simple.Auth.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Models
{
    public class AuthenticationCacheResults
    {
        public readonly AuthenticationCacheType CacheType;
        /// <summary>
        /// Will be null if <see cref="CacheType"/> is not <seealso cref="AuthenticationCacheType.Principal"/>
        /// </summary>
        public readonly ClaimsPrincipal? ClaimsPrincipal;
        /// <summary>
        /// Will be null if <see cref="CacheType"/> is not <seealso cref="AuthenticationCacheType.Refresh"/> 
        /// </summary>
        public readonly RefreshTokenDetails? RefreshTokenDetails;
        /// <summary>
        /// Will be null if <see cref="CacheType"/> is not <seealso cref="AuthenticationCacheType.BlackListed"/>
        /// </summary>
        public readonly DateTime? BlacklistedOn;
        public bool Found => CacheType != AuthenticationCacheType.None;

        private AuthenticationCacheResults(AuthenticationCacheType cacheType, ClaimsPrincipal? claimsPrincipal, RefreshTokenDetails? refreshTokenDetails, DateTime? blacklistedOn)
        {
            CacheType = cacheType;
            ClaimsPrincipal = claimsPrincipal;
            RefreshTokenDetails = refreshTokenDetails;
            BlacklistedOn = blacklistedOn;
        }

        public static AuthenticationCacheResults None()
        {
            return new AuthenticationCacheResults(AuthenticationCacheType.None, null, null, null);
        }
        public static AuthenticationCacheResults ForPrincipal(ClaimsPrincipal principal)
        {
            return new AuthenticationCacheResults(AuthenticationCacheType.Principal, principal, null, null);
        }
        public static AuthenticationCacheResults ForRefreshDetails(RefreshTokenDetails refreshTokenDetails)
        {
            return new AuthenticationCacheResults(AuthenticationCacheType.Refresh, null, refreshTokenDetails, null);
        }
        public static AuthenticationCacheResults Blacklisted(DateTime blacklistedOn)
        {
            return new AuthenticationCacheResults(AuthenticationCacheType.BlackListed, null, null, blacklistedOn);
        }
    }
}
