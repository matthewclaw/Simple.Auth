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
            //_refreshThreshold = TimeSpan.FromMinutes(5);
            //_authorizationService = authorizationService;
            //_correlationService = correlationService;
        }



        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SimpleRequirement requirement)
        {
            try
            {
                //string message;
                //string correlationId = _correlationService.GetCorrelationId() ?? _correlationService.GenerateCorrelationId();
                //bool stateRefreshed;
                //_correlationService.SetCorrelationId(correlationId);
                //var sessionState = await _authorizationService.GetSessionStateAsync();
                //switch (sessionState)
                //{
                //    case Enums.SessionState.Valid:
                //        stateRefreshed = await CheckForAutomaticRefreshAsync();
                //        break;
                //    case Enums.SessionState.RefreshValid:
                //        stateRefreshed = await _authorizationService.TryRefreshAccessAsync();
                //        break;
                //    default:
                //    case Enums.SessionState.None:
                //    case Enums.SessionState.Invalid:
                //        message = "Unauthorized: No Session or Session expired";
                //        _logger.LogWarning(message);
                //        context.Fail();
                //        return;
                //}
                //if (!stateRefreshed)
                //{
                //    context.Fail();
                //    return;
                //}
                //context.Succeed(requirement);
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
