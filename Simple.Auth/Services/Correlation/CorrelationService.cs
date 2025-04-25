using Microsoft.AspNetCore.Http;
using Simple.Auth.Configuration;
using Simple.Auth.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Simple.Auth.Services
{
    [ExcludeFromCodeCoverage]
    public class CorrelationService : ICorrelationService
    {
        private HttpContext _context => _contextOverride ?? _contextAccessor.HttpContext;
        private CookieAccessorOptions _cookieAccessorOptions;
        private HttpContext? _contextOverride;
        private readonly IHttpContextAccessor _contextAccessor;
        public const string CORRELATION_ID_KEY = "x-correlation-id";

        public CorrelationService(IHttpContextAccessor contextAccessor, CookieAccessorOptions accessorOptions)
        {
            _contextAccessor = contextAccessor;
            _contextOverride = null;
            _cookieAccessorOptions = accessorOptions;
        }

        public void ForContext(HttpContext context)
        {
            _contextOverride = context;
        }

        public void ForDefaultContext()
        {
            _contextOverride = null;
        }

        public string GenerateCorrelationId() => Guid.NewGuid().ToString();

        public string GetCorrelationId() => _context.Request.Cookies[CORRELATION_ID_KEY];

        public void SetCorrelationId(string correlationId)
        {
            _context.Response.Cookies.Append(CORRELATION_ID_KEY, correlationId, _cookieAccessorOptions.RefreshSetTokenOptions);
        }
    }
}