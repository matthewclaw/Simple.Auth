using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Interfaces
{
    public interface IHttpContextSwitchable
    {
        void ForContext(HttpContext context);
        void ForDefaultContext();
    }
}
