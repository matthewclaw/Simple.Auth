using Microsoft.AspNetCore.Mvc;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Controllers.Authentication
{
    [ApiController]
    [Route("auth/usernameAndPassword")]
    public class UsernameAndPasswordAuthController : AuthenticationControllerBase<UsernameAndPassword>
    {
        public UsernameAndPasswordAuthController(ICorrelationLoggerFactory loggerFactory, IAuthenticationService authorizationService) : base(loggerFactory, authorizationService)
        {
        }
    }
    [ApiController]
    [Route("auth/other")]
    public class OtherAuthController : AuthenticationControllerBase<UsernameAndPassword>
    {
        public OtherAuthController(ICorrelationLoggerFactory loggerFactory, IAuthenticationService authorizationService) : base(loggerFactory, authorizationService)
        {
        }
    }
}
