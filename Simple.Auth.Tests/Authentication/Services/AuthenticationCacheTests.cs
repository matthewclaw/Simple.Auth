//using Microsoft.Extensions.Caching.Distributed;
//using NSubstitute;
//using Simple.Auth.Converters;
//using Simple.Auth.Enums;
//using Simple.Auth.Helpers;
//using Simple.Auth.Models;
//using Simple.Auth.Services.Authentication;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace Simple.Auth.Tests.Authentication.Services
//{
//    public class AuthenticationCacheTests
//    {
//        private readonly IDistributedCache _mockCache;
//        private readonly AuthenticationCache _authenticationCache;
//        private readonly JsonSerializerOptions _jsonSerializerOptions;

//        public AuthenticationCacheTests()
//        {
//            _mockCache = Substitute.For<IDistributedCache>();
//            _authenticationCache = new AuthenticationCache(_mockCache);
//            _jsonSerializerOptions = new JsonSerializerOptions();
//            _jsonSerializerOptions.Converters.Add(new ClaimsPrincipalConverter());
//        }

//        private string GenerateHash(string input) => input.GenerateBasicHash();
//        private string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, _jsonSerializerOptions);
//        private T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions)!;

//        [Fact]
//        public void GetPrincipal_AccessTokenIsBlacklisted_ReturnsBlacklistedResult()
//        {
//            // Arrange
//            var accessToken = "testAccessToken";
//            var hash = GenerateHash(accessToken);
//            var blacklistDate = DateTime.UtcNow.AddDays(-1);
//            _mockCache.GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Returns(Serialize(blacklistDate));

//            // Act
//            var result = _authenticationCache.GetPrincipal(accessToken);

//            // Assert
//            Assert.Equal(AuthenticationCacheType.BlackListed, result.CacheType);
//            Assert.Equal(blacklistDate, result.BlacklistedOn);
//            Assert.False(result.Found);
//            Assert.Null(result.ClaimsPrincipal);
//            _mockCache.DidNotReceive().GetString(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash);
//        }

//        //[Fact]
//        //public void GetPrincipal_AccessTokenIsNotBlacklisted_PrincipalFoundInCache_ReturnsPrincipalResult()
//        //{
//        //    // Arrange
//        //    var accessToken = "testAccessToken";
//        //    var hash = GenerateHash(accessToken);
//        //    var principal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "123") }, "TestAuth"));
//        //    _mockCache.GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Returns((string)null);
//        //    _mockCache.GetString(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash).Returns(Serialize(principal));

//        //    // Act
//        //    var result = _authenticationCache.GetPrincipal(accessToken);

//        //    // Assert
//        //    Assert.Equal(AuthenticationCacheType.Principal, result.CacheType);
//        //    Assert.NotNull(result.ClaimsPrincipal);
//        //    Assert.True(result.Found);
//        //    Assert.Equal(principal.Identity.NameClaimType, result.ClaimsPrincipal.Identity.NameClaimType);
//        //    _mockCache.Received(1).GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
//        //    _mockCache.Received(1).GetString(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash);
//        //}

//        [Fact]
//        public void GetPrincipal_AccessTokenIsNotBlacklisted_PrincipalNotFoundInCache_ReturnsNoneResult()
//        {
//            // Arrange
//            var accessToken = "testAccessToken";
//            var hash = GenerateHash(accessToken);
//            _mockCache.GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Returns((string)null);
//            _mockCache.GetString(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash).Returns((string)null);

//            // Act
//            var result = _authenticationCache.GetPrincipal(accessToken);

//            // Assert
//            Assert.Equal(AuthenticationCacheType.None, result.CacheType);
//            Assert.Null(result.ClaimsPrincipal);
//            Assert.False(result.Found);
//            _mockCache.Received(1).GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
//            _mockCache.Received(1).GetString(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash);
//        }

//        [Fact]
//        public void GetPrincipal_CacheThrowsException_ReturnsNoneResult()
//        {
//            // Arrange
//            var accessToken = "testAccessToken";
//            var hash = GenerateHash(accessToken);
//            _mockCache.GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Throws(new Exception("Cache error"));

//            // Act
//            var result = _authenticationCache.GetPrincipal(accessToken);

//            // Assert
//            Assert.Equal(AuthenticationCacheType.None, result.CacheType);
//            Assert.Null(result.ClaimsPrincipal);
//            Assert.False(result.Found);
//            _mockCache.Received(1).GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
//            _mockCache.DidNotReceive().GetString(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash);
//        }

//        [Fact]
//        public void GetRefreshTokenDetails_RefreshTokenIsBlacklisted_ReturnsBlacklistedResult()
//        {
//            // Arrange
//            var refreshToken = "testRefreshToken";
//            var hash = GenerateHash(refreshToken);
//            var blacklistDate = DateTime.UtcNow.AddDays(-2);
//            _mockCache.GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Returns(Serialize(blacklistDate));

//            // Act
//            var result = _authenticationCache.GetRefreshTokenDetails(refreshToken);

//            // Assert
//            Assert.Equal(AuthenticationCacheType.BlackListed, result.CacheType);
//            Assert.Equal(blacklistDate, result.BlacklistedOn);
//            Assert.False(result.Found);
//            Assert.Null(result.RefreshTokenDetails);
//            _mockCache.DidNotReceive().GetString(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash);
//        }

//        //[Fact]
//        //public void GetRefreshTokenDetails_RefreshTokenIsNotBlacklisted_DetailsFoundInCache_ReturnsDetailsResult()
//        //{
//        //    // Arrange
//        //    var refreshToken = "testRefreshToken";
//        //    var hash = GenerateHash(refreshToken);
//        //    var refreshTokenDetails = new RefreshTokenDetails { UserId = "user123", ClientId = "client456", Expiry = DateTime.UtcNow.AddHours(1) };
//        //    _mockCache.GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Returns((string)null);
//        //    _mockCache.GetString(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash).Returns(Serialize(refreshTokenDetails));

//        //    // Act
//        //    var result = _authenticationCache.GetRefreshTokenDetails(refreshToken);

//        //    // Assert
//        //    Assert.Equal(AuthenticationCacheType.Refresh, result.CacheType);
//        //    Assert.NotNull(result.RefreshTokenDetails);
//        //    Assert.True(result.Found);
//        //    Assert.Equal(refreshTokenDetails.UserId, result.RefreshTokenDetails.UserId);
//        //    _mockCache.Received(1).GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
//        //    _mockCache.Received(1).GetString(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash);
//        //}

//        [Fact]
//        public void GetRefreshTokenDetails_RefreshTokenIsNotBlacklisted_DetailsNotFoundInCache_ReturnsNoneResult()
//        {
//            // Arrange
//            var refreshToken = "testRefreshToken";
//            var hash = GenerateHash(refreshToken);
//            _mockCache.GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Returns((string)null);
//            _mockCache.GetString(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash).Returns((string)null);

//            // Act
//            var result = _authenticationCache.GetRefreshTokenDetails(refreshToken);

//            // Assert
//            Assert.Equal(AuthenticationCacheType.None, result.CacheType);
//            Assert.Null(result.RefreshTokenDetails);
//            Assert.False(result.Found);
//            _mockCache.Received(1).GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
//            _mockCache.Received(1).GetString(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash);
//        }

//        [Fact]
//        public void GetRefreshTokenDetails_CacheThrowsException_ReturnsNoneResult()
//        {
//            // Arrange
//            var refreshToken = "testRefreshToken";
//            var hash = GenerateHash(refreshToken);
//            _mockCache.GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Throws(new Exception("Cache error"));

//            // Act
//            var result = _authenticationCache.GetRefreshTokenDetails(refreshToken);

//            // Assert
//            Assert.Equal(AuthenticationCacheType.None, result.CacheType);
//            Assert.Null(result.RefreshTokenDetails);
//            Assert.False(result.Found);
//            _mockCache.Received(1).GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
//            _mockCache.DidNotReceive().GetString(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash);
//        }

//        [Fact]
//        public void RemoveDetailsAndBlacklistRefreshToken_SetsBlacklistAndRemovesRefreshToken()
//        {
//            // Arrange
//            var refreshToken = "testRefreshToken";
//            var hash = GenerateHash(refreshToken);
//            DateTime? blacklistDate = null;

//            _mockCache.When(c => c.SetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash, Arg.Any<string>(), Arg.Any<DistributedCacheEntryOptions>()))
//                .Do(callInfo =>
//                {
//                    blacklistDate = Deserialize<DateTime>(callInfo.Arg<string>());
//                });

//            // Act
//            _authenticationCache.RemoveDetailsAndBlacklistRefreshToken(refreshToken);

//            // Assert
//            Assert.NotNull(blacklistDate);
//            Assert.True(DateTime.UtcNow.AddSeconds(-5) < blacklistDate && blacklistDate < DateTime.UtcNow.AddSeconds(5));
//            _mockCache.Received(1).SetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash, Arg.Any<string>(), Arg.Any<DistributedCacheEntryOptions>());
//            _mockCache.Received(1).Remove(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash);
//        }

//        [Fact]
//        public void RemovePrincipalAndBlacklistToken_SetsBlacklistAndRemovesPrincipal()
//        {
//            // Arrange
//            var accessToken = "testAccessToken";
//            var hash = GenerateHash(accessToken);
//            DateTime? blacklistDate = null;

//            _mockCache.When(c => c.SetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash, Arg.Any<string>(), Arg.Any<DistributedCacheEntryOptions>()))
//                .Do(callInfo =>
//                {
//                    blacklistDate = Deserialize<DateTime>(callInfo.Arg<string>());
//                });

//            // Act
//            _authenticationCache.RemovePrincipalAndBlacklistToken(accessToken);

//            // Assert
//            Assert.NotNull(blacklistDate);
//            Assert.True(DateTime.UtcNow.AddSeconds(-5) < blacklistDate && blacklistDate < DateTime.UtcNow.AddSeconds(5));
//            _mockCache.Received(1).SetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash, Arg.Any<string>(), Arg.Any<DistributedCacheEntryOptions>());
//            _mockCache.Received(1).Remove(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash);
//        }

//        //[Fact]
//        //public void SetPrincipal_SetsPrincipalInCache()
//        //{
//        //    // Arrange
//        //    var accessToken = "testAccessToken";
//        //    var hash = GenerateHash(accessToken);
//        //    var principal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "456") }, "TestAuth2"));
//        //    string storedValue = null;
//        //    DistributedCacheEntryOptions storedOptions = null;

//        //    _mockCache.When(c => c.SetString(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash, Arg.Any<string>(), Arg.Any<DistributedCacheEntryOptions>()))
//        //        .Do(callInfo =>
//        //        {
//        //            storedValue = callInfo.Arg<string>();
//        //            storedOptions = callInfo.Arg<DistributedCacheEntryOptions>();
//        //        });

//        //    // Act
//        //    _authenticationCache.SetPrincipal(accessToken, principal);

//        //    // Assert
//        //    Assert.NotNull(storedValue);
//        //    var deserializedPrincipal = Deserialize<ClaimsPrincipal>(storedValue);
//        //    Assert.Equal(principal.Identity.NameClaimType, deserializedPrincipal.Identity.NameClaimType);
//        //    Assert.Equal(TimeSpan.FromMinutes(30), storedOptions!.AbsoluteExpirationRelativeToNow);
//        //    _mockCache.Received(1).SetString(AuthenticationCache.USER_PRINCIPAL_KEY_PREFIX + hash, Arg.Any<string>(), Arg.Any<DistributedCacheEntryOptions>());
//        //}

//        [Fact]
//        public void SetRefreshTokenDetails_SetsRefreshTokenDetailsInCache()
//        {
//            // Arrange
//            var refreshToken = "testRefreshToken";
//            var hash = GenerateHash(refreshToken);
//            // string token, string ipAddress, DateTimeOffset expiry
//            var refreshTokenDetails = new RefreshTokenDetails(refreshToken,"ip", DateTimeOffset.UtcNow.AddHours(1));
//            string storedValue = null;
//            DistributedCacheEntryOptions storedOptions = null;

//            _mockCache.When(c => c.SetString(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash, Arg.Any<string>(), Arg.Any<DistributedCacheEntryOptions>()))
//                .Do(callInfo =>
//                {
//                    storedValue = callInfo.Arg<string>();
//                    storedOptions = callInfo.Arg<DistributedCacheEntryOptions>();
//                });

//            // Act
//            _authenticationCache.SetRefreshTokenDetails(refreshToken, refreshTokenDetails);

//            // Assert
//            Assert.NotNull(storedValue);
//            var deserializedDetails = Deserialize<RefreshTokenDetails>(storedValue);
//            Assert.Equal(refreshTokenDetails.Token, deserializedDetails.Token);
//            Assert.Equal(TimeSpan.FromMinutes(30), storedOptions!.AbsoluteExpirationRelativeToNow);
//            _mockCache.Received(1).SetString(AuthenticationCache.REFRESH_TOKEN_KEY_PREFIX + hash, Arg.Any<string>(), Arg.Any<DistributedCacheEntryOptions>());
//        }

//        [Fact]
//        public void IsBlackListed_TokenIsBlacklisted_ReturnsTrueAndBlacklistDate()
//        {
//            // Arrange
//            var hash = "testHash";
//            var blacklistDate = DateTime.UtcNow.AddDays(-5);
//            _mockCache.GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Returns(Serialize(blacklistDate));

//            // Act
//            var isBlacklisted = _authenticationCache.IsBlackListed(hash, out var date);

//            // Assert
//            Assert.True(isBlacklisted);
//            Assert.Equal(blacklistDate, date);
//            _mockCache.Received(1).GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
//        }

//        [Fact]
//        public void IsBlackListed_TokenIsNotBlacklisted_ReturnsFalseAndNullDate()
//        {
//            // Arrange
//            var hash = "testHash";
//            _mockCache.GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Returns((string)null);

//            // Act
//            var isBlacklisted = _authenticationCache.IsBlackListed(hash, out var date);

//            // Assert
//            Assert.False(isBlacklisted);
//            Assert.Null(date);
//            _mockCache.Received(1).GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
//        }

//        [Fact]
//        public void IsBlackListed_CacheThrowsException_ReturnsFalseAndNullDate()
//        {
//            // Arrange
//            var hash = "testHash";
//            _mockCache.GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash).Throws(new Exception("Cache error"));

//            // Act
//            var isBlacklisted = _authenticationCache.IsBlackListed(hash, out var date);

//            // Assert
//            Assert.False(isBlacklisted);
//            Assert.Null(date);
//            _mockCache.Received(1).GetString(AuthenticationCache.BLACKLISTED_KEY_PREFIX + hash);
//        }
//    }
//}
