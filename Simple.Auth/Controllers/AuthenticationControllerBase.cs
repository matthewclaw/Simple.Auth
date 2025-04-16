
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Services;
using System.Security.Claims;

namespace Simple.Auth.Controllers
{
    public class AuthenticationControllerBase<TLoginRequest> : ControllerBase where TLoginRequest : class
    {

        #region Protected Fields

        protected IAuthenticationService AuthenticationService;

        protected ICorrelationLogger Logger;

        #endregion Protected Fields

        #region Public Constructors

        public AuthenticationControllerBase(ICorrelationLoggerFactory loggerFactory, IAuthenticationService authenticationService)
        {
            Logger = loggerFactory.CreateLogger<AuthenticationControllerBase<TLoginRequest>>();
            AuthenticationService = authenticationService;
        }

        #endregion Public Constructors

        #region Public Methods
        [Authorize(Policy = Constants.Policies.DEFAULT)]
        [HttpGet("me")]
        public async Task<IActionResult> GetMeAsync()
        {
            var test = this.HttpContext.User;
            var claims = test.Claims.ToDictionary(claim => claim.Type, claim => claim.Value);
            return await Task.FromResult(Ok(claims));
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(TLoginRequest request)
        {
            await AuthenticationService.StartSessionAsync(request);
            return Ok();
        }

        [HttpDelete("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            await AuthenticationService.EndSessionAsync();
            return Ok();
        }
        #endregion Public Methods
    }
}

