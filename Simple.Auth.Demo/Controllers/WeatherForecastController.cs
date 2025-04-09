using Microsoft.AspNetCore.Mvc;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Authentication;
using System.Security.Claims;

namespace Simple.Auth.Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ITokenService _tokenService;
        private readonly ITokenAccessor _tokenAccessor;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ITokenAccessor tokenAccessor, ITokenService tokenService)
        {
            _logger = logger;
            _tokenAccessor = tokenAccessor;
            _tokenService = tokenService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            var refreshTok = _tokenService.GenerateRefreshToken();
            var testRefresh = _tokenAccessor.TryGetRefreshToken(out var refreshToken);
            var testAccess = _tokenAccessor.TryGetToken(out var accessToken);
            var verfied = await _tokenService.ValidateTokenAsync(accessToken);
            var (refreshedAcc, refreshedRefres) = await _tokenService.RefreshTokenAsync(accessToken, refreshToken);
            var newAccess = _tokenService.GenerateToken(Array.Empty<Claim>(), TimeSpan.FromHours(1));
            _tokenAccessor.SetRefreshToken(refreshTok);
            _tokenAccessor.SetToken(newAccess);
            return Ok(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray());
        }
    }
}
