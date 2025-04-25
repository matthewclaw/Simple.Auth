using Microsoft.AspNetCore.Mvc;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Models.Requests;
using System.Diagnostics.CodeAnalysis;

namespace Simple.Auth.Controllers.Authentication
{
    [ApiController]
    [Route("auth/classic")]
    [ExcludeFromCodeCoverage]
    public class UsernameAndPasswordAuthController : AuthenticationControllerBase<UsernameAndPassword>
    {
        public UsernameAndPasswordAuthController(ICorrelationLoggerFactory loggerFactory, IAuthenticationService authorizationService) : base(loggerFactory, authorizationService)
        {
        }
    }

    [ApiController]
    [Route("auth/other")]
    [ExcludeFromCodeCoverage]
    public class OtherAuthController : AuthenticationControllerBase<UsernameAndPassword>
    {
        public OtherAuthController(ICorrelationLoggerFactory loggerFactory, IAuthenticationService authorizationService) : base(loggerFactory, authorizationService)
        {
        }
    }
}