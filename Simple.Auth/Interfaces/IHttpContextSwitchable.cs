using Microsoft.AspNetCore.Http;

namespace Simple.Auth.Interfaces
{
    public interface IHttpContextSwitchable
    {
        void ForContext(HttpContext context);

        void ForDefaultContext();
    }
}