using Microsoft.AspNetCore.Http;
using Simple.Auth.Configuration;
using Simple.Auth.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Tests.TestImplementations
{
    [ExcludeFromCodeCoverage]
    internal class TestCookieAccessor : CookieTokenAccessor
    {
        public TestCookieAccessor(IHttpContextAccessor httpContextAccessor, CookieAccessorOptions options) : base(httpContextAccessor, options)
        {
        }
    }
}
