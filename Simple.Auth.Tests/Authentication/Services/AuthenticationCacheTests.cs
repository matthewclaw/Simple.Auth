using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Simple.Auth.Converters;
using Simple.Auth.Enums;
using Simple.Auth.Helpers;
using Simple.Auth.Models;
using Simple.Auth.Services.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Simple.Auth.Tests.Authentication.Services
{
    public class AuthenticationCacheTests
    {
        private readonly IDistributedCache _mockCache;
        private readonly AuthenticationCache _authenticationCache;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AuthenticationCacheTests()
        {
            _mockCache = Substitute.For<IDistributedCache>();
            _authenticationCache = new AuthenticationCache(_mockCache);
            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.Converters.Add(new ClaimsPrincipalConverter());
            _jsonSerializerOptions.Converters.Add(new ClaimConverter());
        }

        private string GenerateHash(string input) => input.GenerateBasicHash();
        private T Deserialize<T>(string input) => JsonSerializer.Deserialize<T>(input, _jsonSerializerOptions);
        private string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, _jsonSerializerOptions);
        private byte[] GetBytes(string input) => Encoding.UTF8.GetBytes(input);
        private string GetString(byte[] input) => Encoding.UTF8.GetString(input);

        [Fact]
        public void GetPrincipal_AccessTokenIsBlacklisted_ReturnsBlacklistedResult()
        {
            // Arrange
            var accessToken = "testAccessToken";
            var hash = GenerateHash(accessToken);
            var blacklistDate = DateTime.UtcNow.AddDays(-1);
            var serializedBytes = GetBytes(Serialize(blacklistDate));
            _mockCache.Get(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Returns(serializedBytes);

            // Act
            var result = _authenticationCache.GetPrincipal(accessToken);

            // Assert
            Assert.Equal(AuthenticationCacheType.BlackListed, result.CacheType);
            Assert.Equal(blacklistDate, result.BlacklistedOn);
            Assert.True(result.Found);
            Assert.Null(result.ClaimsPrincipal);
            _mockCache.DidNotReceive().Get(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash);
        }

        [Fact]
        public void GetPrincipal_AccessTokenIsNotBlacklisted_PrincipalFoundInCache_ReturnsPrincipalResult()
        {
            // Arrange
            var accessToken = "testAccessToken";
            var hash = GenerateHash(accessToken);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "123") }, "TestAuth"));
            var serializedBytes = GetBytes(Serialize(principal));
            _mockCache.Get(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash).Returns(serializedBytes);

            // Act
            var result = _authenticationCache.GetPrincipal(accessToken);

            // Assert
            Assert.Equal(AuthenticationCacheType.Principal, result.CacheType);
            Assert.NotNull(result.ClaimsPrincipal);
            Assert.True(result.Found);
            Assert.Equal(((ClaimsIdentity)principal.Identity).NameClaimType, ((ClaimsIdentity)result.ClaimsPrincipal.Identity).NameClaimType);
            _mockCache.Received(1).Get(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
            _mockCache.Received(1).Get(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash);
        }

        [Fact]
        public void GetPrincipal_AccessTokenIsNotBlacklisted_PrincipalNotFoundInCache_ReturnsNoneResult()
        {
            // Arrange
            var accessToken = "testAccessToken";
            var hash = GenerateHash(accessToken);

            // Act
            var result = _authenticationCache.GetPrincipal(accessToken);

            // Assert
            Assert.Equal(AuthenticationCacheType.None, result.CacheType);
            Assert.Null(result.ClaimsPrincipal);
            Assert.False(result.Found);
            _mockCache.Received(1).Get(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
            _mockCache.Received(1).Get(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash);
        }

        [Fact]
        public void GetPrincipal_CacheThrowsException_ReturnsNoneResult()
        {
            // Arrange
            var accessToken = "testAccessToken";
            var hash = GenerateHash(accessToken);
            _mockCache.Get(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash).Throws(new Exception("Cache error"));

            // Act
            var result = _authenticationCache.GetPrincipal(accessToken);

            // Assert
            Assert.Equal(AuthenticationCacheType.None, result.CacheType);
            Assert.Null(result.ClaimsPrincipal);
            Assert.False(result.Found);
            _mockCache.Received(1).Get(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
            _mockCache.Received(1).Get(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash);
        }

        [Fact]
        public void GetRefreshTokenDetails_RefreshTokenIsBlacklisted_ReturnsBlacklistedResult()
        {
            // Arrange
            var refreshToken = "testRefreshToken";
            var hash = GenerateHash(refreshToken);
            var blacklistDate = DateTime.UtcNow.AddDays(-2);
            var serializedBytes = GetBytes(Serialize(blacklistDate));
            _mockCache.Get(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Returns(serializedBytes);

            // Act
            var result = _authenticationCache.GetRefreshTokenDetails(refreshToken);

            // Assert
            Assert.Equal(AuthenticationCacheType.BlackListed, result.CacheType);
            Assert.Equal(blacklistDate, result.BlacklistedOn);
            Assert.True(result.Found);
            Assert.Null(result.RefreshTokenDetails);
            _mockCache.DidNotReceive().Get(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash);
        }

        [Fact]
        public void GetRefreshTokenDetails_RefreshTokenIsNotBlacklisted_DetailsFoundInCache_ReturnsDetailsResult()
        {
            // Arrange
            var refreshToken = "testRefreshToken";
            var hash = GenerateHash(refreshToken);
            var refreshTokenDetails = new RefreshTokenDetails(refreshToken,"ip_address", DateTimeOffset.UtcNow.AddDays(2));
            var serializedBytes = GetBytes(Serialize(refreshTokenDetails));
            _mockCache.Get(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash).Returns(serializedBytes);

            // Act
            var result = _authenticationCache.GetRefreshTokenDetails(refreshToken);

            // Assert
            Assert.Equal(AuthenticationCacheType.Refresh, result.CacheType);
            Assert.NotNull(result.RefreshTokenDetails);
            Assert.True(result.Found);
            Assert.Equal(refreshTokenDetails.IpAddress, result.RefreshTokenDetails.IpAddress);
            _mockCache.Received(1).Get(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
            _mockCache.Received(1).Get(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash);
        }

        [Fact]
        public void GetRefreshTokenDetails_RefreshTokenIsNotBlacklisted_DetailsNotFoundInCache_ReturnsNoneResult()
        {
            // Arrange
            var refreshToken = "testRefreshToken";
            var hash = GenerateHash(refreshToken);

            // Act
            var result = _authenticationCache.GetRefreshTokenDetails(refreshToken);

            // Assert
            Assert.Equal(AuthenticationCacheType.None, result.CacheType);
            Assert.Null(result.RefreshTokenDetails);
            Assert.False(result.Found);
            _mockCache.Received(1).Get(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
            _mockCache.Received(1).Get(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash);
        }

        [Fact]
        public void GetRefreshTokenDetails_CacheThrowsException_ReturnsNoneResult()
        {
            // Arrange
            var refreshToken = "testRefreshToken";
            var hash = GenerateHash(refreshToken);
            _mockCache.GetString(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash).Throws(new Exception("Cache error"));

            // Act
            var result = _authenticationCache.GetRefreshTokenDetails(refreshToken);

            // Assert
            Assert.Equal(AuthenticationCacheType.None, result.CacheType);
            Assert.Null(result.RefreshTokenDetails);
            Assert.False(result.Found);
            _mockCache.Received(1).Get(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
            _mockCache.Received(1).Get(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash);
        }

        [Fact]
        public void RemoveDetailsAndBlacklistRefreshToken_SetsBlacklistAndRemovesRefreshToken()
        {
            // Arrange
            var refreshToken = "testRefreshToken";
            var hash = GenerateHash(refreshToken);
            DateTime? blacklistDate = null;

            _mockCache.When(c => c.Set(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>()))
                .Do(callInfo =>
                {
                    blacklistDate = Deserialize<DateTime>(GetString(callInfo.Arg<byte[]>()));
                });

            // Act
            _authenticationCache.RemoveDetailsAndBlacklistRefreshToken(refreshToken);

            // Assert
            Assert.NotNull(blacklistDate);
            Assert.True(DateTime.UtcNow.AddSeconds(-5) < blacklistDate && blacklistDate < DateTime.UtcNow.AddSeconds(5));
            _mockCache.Received(1).Set(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>());
            _mockCache.Received(1).Remove(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash);
        }

        [Fact]
        public void RemovePrincipalAndBlacklistToken_SetsBlacklistAndRemovesPrincipal()
        {
            // Arrange
            var accessToken = "testAccessToken";
            var hash = GenerateHash(accessToken);
            DateTime? blacklistDate = null;

            _mockCache.When(c => c.Set(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>()))
                .Do(callInfo =>
                {
                    blacklistDate = Deserialize<DateTime>(GetString(callInfo.Arg<byte[]>()));
                });

            // Act
            _authenticationCache.RemovePrincipalAndBlacklistToken(accessToken);

            // Assert
            Assert.NotNull(blacklistDate);
            Assert.True(DateTime.UtcNow.AddSeconds(-5) < blacklistDate && blacklistDate < DateTime.UtcNow.AddSeconds(5));
            _mockCache.Received(1).Set(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>());
            _mockCache.Received(1).Remove(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash);
        }

        [Fact]
        public void SetPrincipal_SetsPrincipalInCache()
        {
            // Arrange
            var accessToken = "testAccessToken";
            var hash = GenerateHash(accessToken);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "456") }, "TestAuth2"));
            string storedValue = null;
            DistributedCacheEntryOptions storedOptions = null;

            _mockCache.When(c => c.Set(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>()))
                .Do(callInfo =>
                {
                    storedValue = GetString(callInfo.Arg<byte[]>());
                    storedOptions = callInfo.Arg<DistributedCacheEntryOptions>();
                });

            // Act
            _authenticationCache.SetPrincipal(accessToken, principal);

            // Assert
            Assert.NotNull(storedValue);
            var deserializedPrincipal = Deserialize<ClaimsPrincipal>(storedValue);
            Assert.Equal(((ClaimsIdentity)principal.Identity).NameClaimType, ((ClaimsIdentity)deserializedPrincipal.Identity).NameClaimType);
            Assert.Equal(TimeSpan.FromMinutes(30), storedOptions!.AbsoluteExpirationRelativeToNow);
            _mockCache.Received(1).Set(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>());
        }

        [Fact]
        public void SetRefreshTokenDetails_SetsRefreshTokenDetailsInCache()
        {
            // Arrange
            var refreshToken = "testRefreshToken";
            var hash = GenerateHash(refreshToken);
            var refreshTokenDetails = new RefreshTokenDetails(refreshToken, "ip", DateTimeOffset.UtcNow.AddHours(1));
            string storedValue = null;
            DistributedCacheEntryOptions storedOptions = null;

            _mockCache.When(c => c.Set(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>()))
                .Do(callInfo =>
                {
                    storedValue =GetString(callInfo.Arg<byte[]>());
                    storedOptions = callInfo.Arg<DistributedCacheEntryOptions>();
                });

            // Act
            _authenticationCache.SetRefreshTokenDetails(refreshToken, refreshTokenDetails);

            // Assert
            Assert.NotNull(storedValue);
            var deserializedDetails = Deserialize<RefreshTokenDetails>(storedValue);
            Assert.Equal(refreshTokenDetails.Token, deserializedDetails.Token);
            Assert.Equal(TimeSpan.FromMinutes(30), storedOptions!.AbsoluteExpirationRelativeToNow);
            _mockCache.Received(1).Set(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>());
        }

        [Fact]
        public void IsBlackListed_TokenIsBlacklisted_ReturnsTrueAndBlacklistDate()
        {
            // Arrange
            var hash = "testHash";
            var blacklistDate = DateTime.UtcNow.AddDays(-5);
            var serializedBytes = GetBytes(Serialize(blacklistDate));
            _mockCache.Get(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Returns(serializedBytes);

            // Act
            var isBlacklisted = _authenticationCache.IsBlackListed(hash, out var date);

            // Assert
            Assert.True(isBlacklisted);
            Assert.Equal(blacklistDate, date);
            _mockCache.Received(1).GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
        }

        [Fact]
        public void IsBlackListed_TokenIsNotBlacklisted_ReturnsFalseAndNullDate()
        {
            // Arrange
            var hash = "testHash";

            // Act
            var isBlacklisted = _authenticationCache.IsBlackListed(hash, out var date);

            // Assert
            Assert.False(isBlacklisted);
            Assert.Null(date);
            _mockCache.Received(1).Get(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
        }

        [Fact]
        public void IsBlackListed_CacheThrowsException_ReturnsFalseAndNullDate()
        {
            // Arrange
            var hash = "testHash";
            _mockCache.Get(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Throws(new Exception("Cache error"));

            // Act
            var isBlacklisted = _authenticationCache.IsBlackListed(hash, out var date);

            // Assert
            Assert.False(isBlacklisted);
            Assert.Null(date);
            _mockCache.Received(1).Get(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
        }
    }
}
