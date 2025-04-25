using Microsoft.AspNetCore.Authorization;
using Simple.Auth.Interfaces;
using Simple.Auth.Requirements;
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