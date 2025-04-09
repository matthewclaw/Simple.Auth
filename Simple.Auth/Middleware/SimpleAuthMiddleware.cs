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
    public class SimpleAuthMiddleware
    {
        protected readonly RequestDelegate Next;
        protected readonly IAuthorizationService AuthorizationService;
        protected readonly ILogger Logger;
        protected readonly ICorrelationService CorrelationService;

        public SimpleAuthMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IAuthorizationService authorizationService, ICorrelationService correlationService)
        {
            Next = next;
            Logger = loggerFactory.CreateLogger<SimpleAuthMiddleware>();
            AuthorizationService = authorizationService;
            CorrelationService = correlationService;
        }
        private void LogInformation(string message)
        {
            Logger.LogInformation(message);
        }
        private void LogError(string message)
        {
            Logger.LogError(message);
        }
        private void LogWarning(string message)
        {
            Logger.LogWarning(message);
        }
        private string LogMessage(string message, )
        {
            var correlationId = CorrelationService.GetCorrelationId();
            if (string.IsNullOrEmpty(correlationId))
            {
                return message;
            }
            return 
        }
        public virtual async Task InvokeAsync(HttpContext context)
        {
            try
            {
                AuthorizationService.ForContext(context);
                var sessionState = await AuthorizationService.GetSessionStateAsync();
                if (sessionState == Enums.SessionState.Invalid || sessionState == Enums.SessionState.None)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    await context.Response.WriteAsync("Unauthorized: No Session or Session expired");
                    return;
                }
                if (sessionState == Enums.SessionState.RefreshValid)
                {
                    var refreshed = await AuthorizationService.TryRefreshAccessAsync();
                    if (!refreshed)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized: Session expired");
                    }
                }

            }
            catch (Exception ex) { }
            finally
            {
                AuthorizationService.ForDefaultContext();
            }
        }
    }
}
