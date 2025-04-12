using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Middleware
{
    public class SimpleAuthMiddleware: IMiddleware
    {
        protected readonly RequestDelegate Next;
        protected readonly IAuthenticationService AuthorizationService;
        protected readonly ICorrelationLogger Logger;
        protected readonly ICorrelationService CorrelationService;

        public SimpleAuthMiddleware(ICorrelationLoggerFactory loggerFactory, IAuthenticationService authorizationService, ICorrelationService correlationService)
        {
            Logger = loggerFactory.CreateLogger<SimpleAuthMiddleware>();
            AuthorizationService = authorizationService;
            CorrelationService = correlationService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                AuthorizationService.ForContext(context);
                var sessionState = await AuthorizationService.GetSessionStateAsync();
                if (sessionState == Enums.SessionState.Invalid || sessionState == Enums.SessionState.None)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    Logger.LogWarning("Unauthorized: No Session or Session expired");
                    await context.Response.WriteAsync("Unauthorized: No Session or Session expired");
                    return;
                }
                if (sessionState == Enums.SessionState.RefreshValid)
                {
                    var refreshed = await AuthorizationService.TryRefreshAccessAsync();
                    if (!refreshed)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        Logger.LogWarning("Session expired");
                        await context.Response.WriteAsync("Unauthorized: Session expired");
                        return;
                    }
                }
                await next(context);

            }
            catch (Exception ex)
            {
                Logger.LogError("Exception thrown: {exception}. Stack: {stackTrace}", ex.Message, ex.StackTrace);
            }
            finally
            {
                AuthorizationService.ForDefaultContext();
            }
        }
    }
}
