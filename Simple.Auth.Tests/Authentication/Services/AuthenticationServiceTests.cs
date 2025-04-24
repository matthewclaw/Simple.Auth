using Microsoft.AspNetCore.Http;
using NSubstitute;
using Simple.Auth.Enums;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Interfaces.Stores;
using Simple.Auth.Interfaces;
using Simple.Auth.Models;
using Simple.Auth.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Simple.Auth.Helpers;
using NSubstitute.ExceptionExtensions;
using Simple.Auth.Tests.Authentication.TestImplementations;

namespace Simple.Auth.Tests.Authentication.Services
{
    public class AuthenticationServiceTests
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpTokenAccessor _tokenAccessor;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenStore _refreshTokenStore;
        private readonly ICorrelationService _correlationService;
        private readonly IUserAuthenticator _userAuthenticator;
        private readonly ICorrelationLogger _logger;
        private readonly ICorrelationLoggerFactory _loggerFactory;
        private readonly AuthenticationService _authService;
        private readonly IAuthenticationCache _cache;

        public AuthenticationServiceTests()
        {
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _tokenAccessor = Substitute.For<CookieTokenAccessor>();
            _tokenService = Substitute.For<ITokenService>();
            _refreshTokenStore = Substitute.For<IRefreshTokenStore>();
            _correlationService = Substitute.For<ICorrelationService>();
            _userAuthenticator = Substitute.For<IUserAuthenticator>();
            _logger = Substitute.For<ICorrelationLogger>();
            _loggerFactory = Substitute.For<ICorrelationLoggerFactory>();
            _loggerFactory.CreateLogger<AuthenticationService>().Returns(_logger);
            _cache = new TestAuthenticationCache();
            _authService = new AuthenticationService(
                _httpContextAccessor,
                _tokenAccessor,
                _tokenService,
                _refreshTokenStore,
                _correlationService,
                _userAuthenticator,
                _loggerFactory,
                _cache);
        }

        [Fact]
        public async Task GetSessionStateAsync_NoTokens_ReturnsNone()
        {
            // Act
            var result = await _authService.GetSessionStateAsync();

            // Assert
            Assert.Equal(SessionState.None, result);
        }

        [Fact]
        public async Task GetSessionStateAsync_ValidToken_HasRefreshToken_ReturnsValid()
        {
            // Arrange
            _tokenAccessor.TryGetToken(out Arg.Any<string>()).Returns(x => { x[0] = "valid_token"; return true; });
            _tokenAccessor.TryGetRefreshToken(out Arg.Any<string>()).Returns(x => { x[0] = "valid_refresh"; return true; });
            _tokenService.ValidateTokenAsync("valid_token").Returns(true);

            // Act
            var result = await _authService.GetSessionStateAsync();

            // Assert
            Assert.Equal(SessionState.Valid, result);
        }

        [Fact]
        public async Task GetSessionStateAsync_InvalidToken_ValidRefreshToken_ReturnsRefreshValid()
        {
            // Arrange
            var clientIp = "client_ip";
            _tokenAccessor.TryGetToken(out Arg.Any<string>()).Returns(x => { x[0] = "invalid_token"; return true; });
            _tokenAccessor.TryGetRefreshToken(out Arg.Any<string>()).Returns(x => { x[0] = "valid_refresh"; return true; });
            _tokenService.ValidateTokenAsync("invalid_token").Returns(false);
            _refreshTokenStore.GetAsync("valid_refresh").Returns(new RefreshTokenDetails { Expiry = DateTimeOffset.UtcNow.AddDays(1), IpAddress = clientIp });

            _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].Returns((Microsoft.Extensions.Primitives.StringValues)clientIp);

            // Act
            var result = await _authService.GetSessionStateAsync();

            // Assert
            Assert.Equal(SessionState.RefreshValid, result);
        }

        [Fact]
        public async Task GetSessionStateAsync_InvalidToken_ValidRefreshTokenInCache_ReturnsRefreshValid()
        {
            // Arrange
            var clientIp = "client_ip";
            var refreshToken = "valid_refresh";
            var refreshTokenDetails = new RefreshTokenDetails { Expiry = DateTimeOffset.UtcNow.AddDays(1), IpAddress = clientIp };
            _tokenAccessor.TryGetToken(out Arg.Any<string>()).Returns(x => { x[0] = "invalid_token"; return true; });
            _tokenAccessor.TryGetRefreshToken(out Arg.Any<string>()).Returns(x => { x[0] = refreshToken; return true; });
            _tokenService.ValidateTokenAsync("invalid_token").Returns(false);
            _cache.SetRefreshTokenDetails(refreshToken, refreshTokenDetails);
            _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].Returns((Microsoft.Extensions.Primitives.StringValues)clientIp);

            // Act
            var result = await _authService.GetSessionStateAsync();

            // Assert
            Assert.Equal(SessionState.RefreshValid, result);
            _refreshTokenStore.DidNotReceive().GetAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetSessionStateAsync_InvalidToken_InvalidRefreshToken_ReturnsInvalid()
        {
            // Arrange
            _tokenAccessor.TryGetToken(out Arg.Any<string>()).Returns(x => { x[0] = "invalid_token"; return true; });
            _tokenAccessor.TryGetRefreshToken(out Arg.Any<string>()).Returns(x => { x[0] = "invalid_refresh"; return true; });
            _tokenService.ValidateTokenAsync("invalid_token").Returns(false);
            _refreshTokenStore.GetAsync("invalid_refresh").Returns(null as RefreshTokenDetails);

            // Act
            var result = await _authService.GetSessionStateAsync();

            // Assert
            Assert.Equal(SessionState.Invalid, result);
            _logger.Received().LogWarning("Could not refresh session");
        }

        [Fact]
        public async Task GetSessionStateAsync_RefreshTokenCacheBlacklisted_ReturnsInvalid()
        {
            // Arrange
            var refreshToken = "invalid_refresh";
            _tokenAccessor.TryGetToken(out Arg.Any<string>()).Returns(x => { x[0] = "invalid_token"; return true; });
            _tokenAccessor.TryGetRefreshToken(out Arg.Any<string>()).Returns(x => { x[0] = refreshToken; return true; });
            _tokenService.ValidateTokenAsync("invalid_token").Returns(false);
            _cache.RemoveDetailsAndBlacklistRefreshToken(refreshToken);

            // Act
            var result = await _authService.GetSessionStateAsync();

            // Assert
            Assert.Equal(SessionState.Invalid, result);
            _logger.Received().LogWarning("Could not refresh session");
        }

        [Fact]
        public async Task StartSessionAsync_UserAuthenticationFails_ThrowsArgumentException()
        {
            // Arrange
            var request = new object();
            _userAuthenticator.AuthenticateUserAsync(request).Returns(AuthenticationResult.Failure("Invalid credentials"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _authService.StartSessionAsync(request));
            _correlationService.DidNotReceive().GenerateCorrelationId();
            _tokenService.DidNotReceive().GenerateToken(Arg.Any<IEnumerable<Claim>>(), Arg.Any<TimeSpan>());
            _tokenService.DidNotReceive().GenerateRefreshToken();
            _tokenAccessor.DidNotReceive().SetToken(Arg.Any<string>());
            _tokenAccessor.DidNotReceive().SetRefreshToken(Arg.Any<string>());
            _refreshTokenStore.DidNotReceive().InsertAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>());
        }

        [Fact]
        public async Task StartSessionAsync_UserAuthenticationSucceeds_GeneratesAndStoresTokens()
        {
            // Arrange
            var request = new object();
            var principal = new ClaimsPrincipal();
            var authResult = AuthenticationResult.Success(principal);
            var correlationId = Guid.NewGuid().ToString();
            var accessToken = "new_access_token";
            var refreshToken = "new_refresh_token";
            var clientIp = "test_client_ip";
            var refreshTokenExpiry = DateTimeOffset.UtcNow + TimeSpan.FromDays(7);

            _userAuthenticator.AuthenticateUserAsync(request).Returns(authResult);
            _correlationService.GenerateCorrelationId().Returns(correlationId);
            _tokenService.GenerateToken(Arg.Any<IEnumerable<Claim>>(), Arg.Any<TimeSpan>()).Returns(accessToken);
            _tokenService.GenerateRefreshToken().Returns(refreshToken);
            _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].Returns((Microsoft.Extensions.Primitives.StringValues)clientIp);

            // Act
            var (actualAccessToken, actualRefreshToken, actualPrincipal) = await _authService.StartSessionAsync(request);

            // Assert
            Assert.Equal(accessToken, actualAccessToken);
            Assert.Equal(refreshToken, actualRefreshToken);
            Assert.Equal(principal, actualPrincipal);
            _correlationService.Received().SetCorrelationId(correlationId);
            _tokenAccessor.Received().SetToken(accessToken);
            _tokenAccessor.Received().SetRefreshToken(refreshToken);
            await _refreshTokenStore.Received().InsertAsync(Arg.Is<RefreshTokenDetails>(r => r.Token == refreshToken));
        }

        [Fact]
        public async Task TryRefreshAccessAsync_InvalidSessionState_ReturnsFalse()
        {
            // Arrange
            _tokenAccessor.TryGetToken(out Arg.Any<string>()).Returns(x => { x[0] = "invalid_token"; return true; });
            _tokenAccessor.TryGetRefreshToken(out Arg.Any<string>()).Returns(x => { x[0] = "invalid_refresh"; return true; });
            _refreshTokenStore.GetAsync(Arg.Any<string>()).Returns(new RefreshTokenDetails("", "", DateTimeOffset.UtcNow.AddDays(-1)));

            // Act
            var result = await _authService.TryRefreshAccessAsync();

            // Assert
            Assert.False(result);
            _tokenService.DidNotReceive().RefreshTokenAsync(Arg.Any<string>(), Arg.Any<string>());
            _tokenAccessor.DidNotReceive().SetToken(Arg.Any<string>());
            _tokenAccessor.DidNotReceive().SetRefreshToken(Arg.Any<string>());
            _refreshTokenStore.DidNotReceive().InsertAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>());
        }

        [Fact]
        public async Task TryRefreshAccessAsync_RefreshTokenFails_ReturnsFalse()
        {
            // Arrange
            _refreshTokenStore.GetAsync(Arg.Any<string>()).Returns(new RefreshTokenDetails("", "", DateTimeOffset.UtcNow.AddDays(1)));
            _tokenAccessor.TryGetRefreshToken(out Arg.Any<string>()).Returns(x => { x[0] = "old_refresh"; return true; });
            _tokenAccessor.TryGetToken(out Arg.Any<string>()).Returns(x => { x[0] = "old_token"; return true; });
            _tokenService.RefreshTokenAsync("old_token", "old_refresh").ThrowsAsync(new Exception("Refresh failed"));

            // Act
            var result = await _authService.TryRefreshAccessAsync();

            // Assert
            Assert.False(result);
            _tokenService.Received().RefreshTokenAsync(Arg.Any<string>(), Arg.Any<string>());
            _tokenAccessor.DidNotReceive().SetToken(Arg.Any<string>());
            _tokenAccessor.DidNotReceive().SetRefreshToken(Arg.Any<string>());
            _refreshTokenStore.DidNotReceive().InsertAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>());
        }

        [Fact]
        public async Task TryRefreshAccessAsync_RefreshTokenSucceeds_UpdatesTokensAndReturnsTrue()
        {
            // Arrange
            _refreshTokenStore.GetAsync(Arg.Any<string>()).Returns(new RefreshTokenDetails("", "test_client_ip", DateTimeOffset.UtcNow.AddDays(1)));
            _tokenAccessor.TryGetRefreshToken(out Arg.Any<string>()).Returns(x => { x[0] = "old_refresh"; return true; });
            _tokenAccessor.TryGetToken(out Arg.Any<string>()).Returns(x => { x[0] = "old_token"; return true; });
            var newAccessToken = "new_access";
            var newRefreshToken = "new_refresh";
            var clientIp = "test_client_ip";
            var refreshTokenExpiry = DateTimeOffset.UtcNow + TimeSpan.FromDays(7);

            _tokenService.RefreshTokenAsync("old_token", "old_refresh").Returns((newAccessToken, newRefreshToken));
            _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].Returns((Microsoft.Extensions.Primitives.StringValues)clientIp);

            // Act
            var result = await _authService.TryRefreshAccessAsync();

            // Assert
            Assert.True(result);
            _tokenAccessor.Received().TryGetRefreshToken(out Arg.Any<string>());
            _tokenAccessor.Received().TryGetToken(out Arg.Any<string>());
            await _tokenService.Received().RefreshTokenAsync("old_token", "old_refresh");
            _tokenAccessor.Received().SetToken(newAccessToken);
            _tokenAccessor.Received().SetRefreshToken(newRefreshToken);
            await _refreshTokenStore.Received().InsertAsync(Arg.Is<RefreshTokenDetails>(r => r.Token == newRefreshToken));
        }

        [Fact]
        public async Task EndSessionAsync_HasRefreshToken_BlacklistsAndRemovesTokens()
        {
            // Arrange
            _tokenAccessor.TryGetRefreshToken(out Arg.Any<string>()).Returns(x => { x[0] = "old_refresh"; return true; });

            // Act
            await _authService.EndSessionAsync();

            // Assert
            await _refreshTokenStore.Received().BlacklistAsync("old_refresh");
            _tokenAccessor.Received().RemoveTokens();
        }

        [Fact]
        public async Task EndSessionAsync_NoRefreshToken_DoesNotBlacklistOrRemove()
        {
            // Arrange
            _tokenAccessor.TryGetRefreshToken(out Arg.Any<string>()).Returns(false);

            // Act
            await _authService.EndSessionAsync();

            // Assert
            await _refreshTokenStore.DidNotReceive().BlacklistAsync(Arg.Any<string>());
            _tokenAccessor.DidNotReceive().RemoveTokens();
        }

        [Fact]
        public void GetTokenExpiry_HasToken_ReturnsExpiryFromTokenService()
        {
            // Arrange
            var token = "test_token";
            var expiry = DateTimeOffset.UtcNow.AddMinutes(30);
            _tokenAccessor.TryGetToken(out Arg.Any<string>()).Returns(x => { x[0] = token; return true; });
            _tokenService.GetTokenExpiry(token).Returns(expiry);

            // Act
            var result = _authService.GetTokenExpiry();

            // Assert
            Assert.Equal(expiry, result);
            _tokenService.Received().GetTokenExpiry(token);
        }

        [Fact]
        public void GetTokenExpiry_NoToken_ReturnsMinValue()
        {
            // Arrange
            _tokenAccessor.TryGetToken(out Arg.Any<string>()).Returns(false);

            // Act
            var result = _authService.GetTokenExpiry();

            // Assert
            Assert.Equal(DateTimeOffset.MinValue, result);
            _tokenService.DidNotReceive().GetTokenExpiry(Arg.Any<string>());
        }

        [Fact]
        public async Task AuthenticateAsync_WithAccessToken_CallsUserAuthenticator()
        {
            // Arrange
            var accessToken = "test_access_token";
            var authResult = AuthenticationResult.Success(new ClaimsPrincipal());
            _userAuthenticator.AuthenticateUserAsync(accessToken).Returns(authResult);

            // Act
            var result = await _authService.AuthenticateAsync(accessToken);

            // Assert
            Assert.Equal(authResult, result);
            await _userAuthenticator.Received().AuthenticateUserAsync(accessToken);
        }

        [Fact]
        public async Task AuthenticateAsync_WithAccessTokenInCache_DoesNotCallUserAuthenticator()
        {
            // Arrange
            var accessToken = "test_access_token";
            var principal = new ClaimsPrincipal();
            var authResult = AuthenticationResult.Success(principal);
            _cache.SetPrincipal(accessToken, principal);

            // Act
            var result = await _authService.AuthenticateAsync(accessToken);

            // Assert
            Assert.True(result.Succeeded);
            await _userAuthenticator.DidNotReceive().AuthenticateUserAsync(accessToken);
        }

        [Fact]
        public async Task AuthenticateAsync_WithAccessTokenBlacklisted_DoesNotCallUserAuthenticator()
        {
            // Arrange
            var accessToken = "test_access_token";
            _cache.RemovePrincipalAndBlacklistToken(accessToken);

            // Act
            var result = await _authService.AuthenticateAsync(accessToken);

            // Assert
            Assert.False(result.Succeeded);
            await _userAuthenticator.DidNotReceive().AuthenticateUserAsync(accessToken);
        }

        [Fact]
        public async Task AuthenticateAsync_NoAccessTokenInAccessor_ReturnsFailure()
        {
            // Arrange
            _tokenAccessor.TryGetToken(out Arg.Any<string>()).Returns(false);

            // Act
            var result = await _authService.AuthenticateAsync();

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("No Access Token", result.Message);
            await _userAuthenticator.DidNotReceive().AuthenticateUserAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task AuthenticateAsync_AccessTokenInAccessor_CallsUserAuthenticator()
        {
            // Arrange
            var accessToken = "token_from_accessor";
            var authResult = AuthenticationResult.Success(new ClaimsPrincipal());
            _tokenAccessor.TryGetToken(out Arg.Any<string>()).Returns(x => { x[0] = accessToken; return true; });
            _userAuthenticator.AuthenticateUserAsync(accessToken).Returns(authResult);

            // Act
            var result = await _authService.AuthenticateAsync();

            // Assert
            Assert.Equal(authResult, result);
            await _userAuthenticator.Received().AuthenticateUserAsync(accessToken);
        }

        [Fact]
        public void ForContext_CallsTokenAccessorForContext()
        {
            // Arrange
            var context = Substitute.For<HttpContext>();

            // Act
            _authService.ForContext(context);

            // Assert
            _tokenAccessor.Received().ForContext(context);
        }

        [Fact]
        public void ForDefaultContext_CallsTokenAccessorForDefaultContext()
        {
            // Arrange & Act
            _authService.ForDefaultContext();

            // Assert
            _tokenAccessor.Received().ForDefaultContext();
        }
    }
}