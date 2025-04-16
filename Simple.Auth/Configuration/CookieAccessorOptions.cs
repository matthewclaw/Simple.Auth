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
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(20)
        },
          new CookieOptions
          {
              HttpOnly = true,
              Secure = true,
              SameSite = SameSiteMode.Strict,
              Expires = DateTimeOffset.UtcNow.AddDays(30)
          });


    }
}
