using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Configuration
{
    public class CookieAccessorOptions
    {
        public CookieAccessorOptions(CookieOptions tokenSetOptions, CookieOptions refreshTokenOptions)
        {
            TokenSetOptions = tokenSetOptions;
            RefreshSetTokenOptions = refreshTokenOptions;
        }
        public CookieAccessorOptions() : this(Default.TokenSetOptions, Default.RefreshSetTokenOptions) { }

        public CookieOptions TokenSetOptions { get; set; }
        public CookieOptions RefreshSetTokenOptions { get; set; }
        public static CookieAccessorOptions Default => new CookieAccessorOptions(new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict, // Or Lax, depending on your needs.
            Expires = DateTimeOffset.UtcNow.AddMinutes(20) // adjust expiration
        },
          new CookieOptions
          {
              HttpOnly = true,
              Secure = true,
              SameSite = SameSiteMode.Strict, // Or Lax, depending on your needs.
              Expires = DateTimeOffset.UtcNow.AddDays(30) // adjust expiration
          });


    }
}
