using Microsoft.AspNetCore.Http;

namespace Simple.Auth.Helpers
{
    public static class HttpContextExtensions
    {
        public static string GetClientIpAddress(this IHttpContextAccessor httpContextAccessor)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return string.Empty;
            }

            // Try to get the forwarded IP address (from proxies) first
            string forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? string.Empty;
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // Could have multiple IP addresses, take the first one (client's original IP)
                return forwardedFor.Split(',').FirstOrDefault()!.Trim();
            }

            // If no forwarded IP, get the direct remote IP address
            var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
            return remoteIpAddress?.ToString() ?? string.Empty;
        }
    }
}