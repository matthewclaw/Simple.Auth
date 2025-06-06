﻿using Microsoft.AspNetCore.Http;
using Simple.Auth.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Simple.Auth.Services
{
    /// <summary>
    /// Provides access to tokens stored in cookies.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CookieTokenAccessor : HttpTokenAccessor
    {
        /// <summary>
        /// The key used to store the refresh token in the cookie.
        /// </summary>
        protected virtual string CookieRefreshTokenKey => "sa_r_token";

        /// <summary>
        /// The key used to store the access token in the cookie.
        /// </summary>
        protected virtual string CookieAccessTokenTokenKey => "sa_token";

        protected readonly CookieAccessorOptions CookieAccessorOptions;

        public CookieTokenAccessor(IHttpContextAccessor httpContextAccessor, CookieAccessorOptions options) : base(httpContextAccessor)
        {
            CookieAccessorOptions = options;
        }

        /// <summary>
        /// To only be used in unit testing substitues
        /// </summary>
        public CookieTokenAccessor() : this(null, new CookieAccessorOptions())
        {
        }

        public CookieTokenAccessor(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            CookieAccessorOptions = CookieAccessorOptions.Default;
        }

        public override void SetRefreshToken(string token)
        {
            HttpContext.Response.Cookies.Append(CookieRefreshTokenKey, token, CookieAccessorOptions.RefreshSetTokenOptions);
        }

        public override void SetToken(string token)
        {
            HttpContext.Response.Cookies.Append(CookieAccessTokenTokenKey, token, CookieAccessorOptions.TokenSetOptions);
        }

        public override bool TryGetRefreshToken(out string token)
        {
            token = HttpContext.Request.Cookies[CookieRefreshTokenKey] ?? string.Empty;
            return !string.IsNullOrEmpty(token);
        }

        public override bool TryGetToken(out string token)
        {
            token = HttpContext.Request.Cookies[CookieAccessTokenTokenKey] ?? string.Empty;
            return !string.IsNullOrEmpty(token);
        }

        public override void RemoveTokens()
        {
            HttpContext.Response.Cookies.Delete(CookieAccessTokenTokenKey);
            HttpContext.Response.Cookies.Delete(CookieRefreshTokenKey);
        }
    }
}