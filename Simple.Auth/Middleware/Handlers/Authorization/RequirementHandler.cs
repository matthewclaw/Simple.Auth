using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Requirements;
using Simple.Auth.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISimpleAuthorizationService = Simple.Auth.Interfaces.Authentication.IAuthenticationService;

namespace Simple.Auth.Middleware.Handlers.Authorization
{
    public class RequirementHandler : AuthorizationHandler<SimpleRequirement>
    {
        private readonly ICorrelationLogger _logger;

        public RequirementHandler(ICorrelationLoggerFactory loggerFactory, ISimpleAuthorizationService authorizationService,
            ICorrelationService correlationService)
        {
            _logger = loggerFactory.CreateLogger<RequirementHandler>();
        }



        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SimpleRequirement requirement)
        {
            try
            {
                if (!context.User?.Identity?.IsAuthenticated ?? true)
                {
                    context.Fail();
                    return;
                }
                context.Succeed(requirement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                context.Fail();
                throw;
            }
        }
    }
}
