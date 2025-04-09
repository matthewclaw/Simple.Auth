using Microsoft.AspNetCore.Http;
using Simple.Auth.Interfaces.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Middleware
{
    public class SimpleAuthMiddleware
    {
        protected readonly RequestDelegate Next;
        protected readonly IAuthorizationService AuthorizationService;

        public SimpleAuthMiddleware(RequestDelegate next, IAuthorizationService authorizationService)
        {
            Next = next;
            AuthorizationService = authorizationService;
        }

        public virtual async Task InvokeAsync(HttpContext context)
        {
            
        }
    }
}
