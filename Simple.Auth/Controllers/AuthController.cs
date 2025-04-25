//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using Simple.Auth.Interfaces.Authentication;
//using Simple.Auth.Services;
//using System.Security.Claims;

//namespace Simple.Auth.Controllers
//{
//    [ApiController]
//    [Route("[controller]")]
//    public class AuthController : ControllerBase
//    {
//        private static readonly string[] Summaries = new[]
//        {
//            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//        };

//        private readonly ILogger<AuthController> _logger;
//        private readonly ITokenService _tokenService;
//        private readonly ITokenAccessor _tokenAccessor;
//        private readonly Interfaces.Authentication.IAuthenticationService _authorizationService;

//        public AuthController(ILogger<AuthController> logger, HttpTokenAccessor tokenAccessor, ITokenService tokenService, Interfaces.Authentication.IAuthenticationService authorizationService)
//        {
//            _logger = logger;
//            _tokenAccessor = tokenAccessor;
//            _tokenService = tokenService;
//            _authorizationService = authorizationService;
//        }
//        [Authorize(Policy = Constants.Policies.DEFAULT)]
//        [HttpGet("me")]
//        public async Task<IActionResult> Get()
//        {
//            var refreshTok = _tokenService.GenerateRefreshToken();
//            var testRefresh = _tokenAccessor.TryGetRefreshToken(out var refreshToken);
//            var testAccess = _tokenAccessor.TryGetToken(out var accessToken);
//            var verfied = await _tokenService.ValidateTokenAsync(accessToken);
//            var (refreshedAcc, refreshedRefres) = await _tokenService.RefreshTokenAsync(accessToken, refreshToken);
//            var newAccess = _tokenService.GenerateToken(Array.Empty<Claim>(), TimeSpan.FromHours(1));
//            _tokenAccessor.SetRefreshToken(refreshTok);
//            _tokenAccessor.SetToken(newAccess);
//            return Ok();
//        }

//        [HttpGet("test")]
//        public async Task<IActionResult> GetDummySession()
//        {
//            dynamic request = new { Email = "alice@example.com", Password = "password123" };
//            await _authorizationService.StartSessionAsync(request);
//            return Ok();
//        }
//    }
//}