
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Services;
using System.Security.Claims;

namespace Simple.Auth.Controllers
{
    public class AuthenticationControllerBase<TLoginRequest> : ControllerBase
    {

        #region Protected Fields

        protected IAuthenticationService AuthorizationService;

        protected ILogger Logger;

        #endregion Protected Fields

        #region Public Constructors

        public AuthenticationControllerBase(ICorrelationLoggerFactory loggerFactory, Interfaces.Authentication.IAuthenticationService authorizationService)
        {
            Logger = loggerFactory.CreateLogger<AuthenticationControllerBase<TLoginRequest>>();
            AuthorizationService = authorizationService;
        }

        #endregion Public Constructors

        #region Public Methods

        [Authorize(Policy = Constants.Policies.DEFAULT)]
        [HttpGet("me")]
        public async Task<IActionResult> Get()
        {
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> GetDummySession(TLoginRequest request)
        {
            //dynamic request = new { Email = "alice@example.com", Password = "password123" };
            await AuthorizationService.StartSessionAsync(request);
            return Ok();
        }

        #endregion Public Methods
    }
}

