using Microsoft.AspNetCore.Http;
using Simple.Auth.Interfaces.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Services
{
    public abstract class HttpTokenAccessor : ITokenAccessor
    {
        protected readonly IHttpContextAccessor HttpContextAccessor;
        [ExcludeFromCodeCoverage]
        protected HttpContext HttpContext => _overiddenHtppContext ?? HttpContextAccessor.HttpContext;
        private HttpContext? _overiddenHtppContext = null;
        protected HttpTokenAccessor(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }
        public abstract void SetRefreshToken(string token);
        public abstract void SetToken(string token);
        public abstract bool TryGetRefreshToken(out string token);
        public abstract bool TryGetToken(out string token);
        public void ForContext(HttpContext context)
        {
            _overiddenHtppContext = context;
        }
        public void ForDefaultContext()
        {
            _overiddenHtppContext = null;
        }

        public abstract void RemoveTokens();
    }
}
