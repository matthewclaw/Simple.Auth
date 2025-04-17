
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Services;
using System.Security.Claims;

namespace Simple.Auth.Controllers.Authentication
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
            var user = HttpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }
            var claims = GetSerializableClaims(user);
            return await Task.FromResult(Ok(claims));
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(TLoginRequest request)
        {
            try
            {
                (_, _, ClaimsPrincipal principle) = await AuthenticationService.StartSessionAsync(request);
                var claims = GetSerializableClaims(principle);
                return Ok();
            }
            catch (ArgumentException ax)
            {
                return Unauthorized(ax.Message);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                await AuthenticationService.EndSessionAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex);
                return BadRequest(ex.Message);
            }
        }
        #endregion Public Methods

        protected Dictionary<string, string> GetSerializableClaims(ClaimsPrincipal principal)
        {
            return principal.Claims.ToDictionary(claim => claim.Type, claim => claim.Value);
        }
    }
}

