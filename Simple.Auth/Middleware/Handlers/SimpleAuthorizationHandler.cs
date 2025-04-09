using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Middleware.Handlers
{
    public class SimpleAuthorizationHandler : AuthorizationHandler<SimpleRequirement>
    {
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenAccessor _tokenAccessor;
        private readonly ITokenService _tokenService;
        private readonly DateTimeOffset _refreshThreshold;

        public SimpleAuthorizationHandler(ILoggerFactory loggerFactory, IHttpContextAccessor httpContext, ITokenAccessor tokenAccessor, ITokenService tokenService)
        {
            _logger = loggerFactory.CreateLogger<SimpleAuthorizationHandler>();
            _httpContext = httpContext;
            _tokenAccessor = tokenAccessor;
            _tokenService = tokenService;
            _refreshThreshold = DateTimeOffset.UtcNow.AddMinutes(5);
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SimpleRequirement requirement)
        {
            try
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
                context.Fail();
                throw;
            }
        }
    }
}
