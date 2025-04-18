using Simple.Auth.Controllers.Authentication;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Tests.TestImplementations
{
    [ExcludeFromCodeCoverage]
    internal class TestAuthenticationController : AuthenticationControllerBase<TestLoginRequest>
    {
        public TestAuthenticationController(ICorrelationLoggerFactory loggerFactory, IAuthenticationService authenticationService)
            : base(loggerFactory, authenticationService)
        {
        }
    }
}
