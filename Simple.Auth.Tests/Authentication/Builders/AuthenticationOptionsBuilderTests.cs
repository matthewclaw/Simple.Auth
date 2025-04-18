using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Simple.Auth.Builders;
using Simple.Auth.Configuration;
using Simple.Auth.Services;
using Simple.Auth.Tests.Authentication.Fakes;
using Simple.Auth.Tests.TestImplementations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Tests.Authentication.Builders
{
    [ExcludeFromCodeCoverage]
    public class AuthenticationOptionsBuilderTests
    {
        [Fact]
        public void Build_WithBaseCalls_ReturnsAuthenticationOptions()
        {
            // Arrange
            var builder = GetAuthenticationOptionsBuilderBaseline();

            var expectedTokenAccessorType = typeof(CookieTokenAccessor);
            var expectedTokenServiceType = typeof(FakeTokenService);
            var expectedUserAuthenticatorType = typeof(FakeUserAuthenticator);

            // Act
            var actual = builder.Build();

            // Assert
            Assert.Equal(expectedTokenAccessorType, actual.TokenAccessOptions.TokenAccessorType);
            Assert.Equal(expectedTokenServiceType, actual.TokenServiceOptions.ServiceType);
            Assert.Equal(expectedUserAuthenticatorType, actual.UserAuthenticatorType);
        }

        [Fact]
        public void Build_WithUseCookiesFactory_ReturnsAuthenticationOptionsWithAccessorFactory()
        {
            // Arrange
            var builder = GetAuthenticationOptionsBuilder(false, true, true, true);
            var fakeHttpAccessor = Substitute.For<IHttpContextAccessor>();
            var fakeCookieOptions = Substitute.For<CookieAccessorOptions>();

            // Act
            builder.UseCookies(sp => new TestCookieAccessor(fakeHttpAccessor, fakeCookieOptions));
            var actual = builder.Build();

            // Assert
            Assert.Null(actual.TokenAccessOptions.TokenAccessorType);
            Assert.NotNull(actual.TokenAccessOptions.ImplementationFactory);
        }

        [Fact]
        public void Build_WithWithTokenServiceFactory_ReturnsAuthenticationOptionsWithTokenServiceFactory()
        {
            // Arrange
            var builder = GetAuthenticationOptionsBuilder(true, false, true, true);

            // Act
            builder.WithDefaultTokenService(sp => new JwtTokenService("foo", "bar", "test"));
            var actual = builder.Build();

            // Assert
            Assert.Null(actual.TokenServiceOptions.ServiceType);
            Assert.NotNull(actual.TokenServiceOptions.ImplementationFactory);
        }

        [Fact]
        public void Build_WithNoUseCookiesCalls_ThrowsInvalidOperation()
        {
            // Arrange
            var builder = GetAuthenticationOptionsBuilder();
            var expectedMessage = $"Either {nameof(AuthenticationOptionsBuilder.UseCookies)} or {nameof(AuthenticationOptionsBuilder.UseCookiesWithOptions)} must be called";
            Type expectedExceptionType = typeof(InvalidOperationException);

            bool exceptionThrown = false;
            string actualMessage = string.Empty;
            Exception? actualException = null;

            // Act
            try
            {
                _ = builder.Build();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                actualException = ex;
                actualMessage = ex.Message;
            }

            // Assert
            Assert.True(exceptionThrown);
            Assert.IsType(expectedExceptionType, actualException);
            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public void Build_WithNoWithConfigurationCalls_ThrowsInvalidOperation()
        {
            // Arrange
            var builder = GetAuthenticationOptionsBuilder(true, true);
            var expectedMessage = $".{nameof(AuthenticationOptionsBuilder.WithConfiguration)} must be called";
            Type expectedExceptionType = typeof(InvalidOperationException);

            bool exceptionThrown = false;
            string actualMessage = string.Empty;
            Exception? actualException = null;

            // Act
            try
            {
                _ = builder.Build();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                actualException = ex;
                actualMessage = ex.Message;
            }

            // Assert
            Assert.True(exceptionThrown);
            Assert.IsType(expectedExceptionType, actualException);
            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public void Build_WithNoWithTokenServiceCalls_ThrowsInvalidOperation()
        {
            // Arrange
            var builder = GetAuthenticationOptionsBuilder(true);
            var expectedMessage = $"Either {nameof(AuthenticationOptionsBuilder.WithDefaultTokenService)} or {nameof(AuthenticationOptionsBuilder.WithTokenService)} must be called";
            Type expectedExceptionType = typeof(InvalidOperationException);

            bool exceptionThrown = false;
            string actualMessage = string.Empty;
            Exception? actualException = null;

            // Act
            try
            {
                _ = builder.Build();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                actualException = ex;
                actualMessage = ex.Message;
            }

            // Assert
            Assert.True(exceptionThrown);
            Assert.IsType(expectedExceptionType, actualException);
            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public void Build_WithNoWithUserAuthenticatorCall_ThrowsInvalidOperation()
        {
            // Arrange
            var builder = GetAuthenticationOptionsBuilder(true, true, true);
            var expectedMessage = $".{nameof(AuthenticationOptionsBuilder.WithUserAuthenticator)} must be called";
            Type expectedExceptionType = typeof(InvalidOperationException);

            bool exceptionThrown = false;
            string actualMessage = string.Empty;
            Exception? actualException = null;

            // Act
            try
            {
                _ = builder.Build();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                actualException = ex;
                actualMessage = ex.Message;
            }

            // Assert
            Assert.True(exceptionThrown);
            Assert.IsType(expectedExceptionType, actualException);
            Assert.Equal(expectedMessage, actualMessage);
        }
        // UseCookiesWithOptions
        [Fact]
        public void Build_WithAddSchemeCall_ReturnsAuthenticationOptionsWithSchemeAddition()
        {
            // Arrange
            var builder = GetAuthenticationOptionsBuilderBaseline();
            var expectedCount = 1;

            // Act
            builder.AddScheme<FakeAuthenticationHandler>("foo", "bar", null);
            var actual = builder.Build();

            // Assert
            Assert.NotEmpty(actual.SchemeAdditions);
            Assert.Equal(expectedCount, actual.SchemeAdditions.Count);
        }

        [Fact]
        public void Build_UseCookiesWithOptions_ReturnsAuthenticationOptionsWithCustomCookieOptions()
        {
            // Arrange
            var builder = GetAuthenticationOptionsBuilderBaseline();

            SameSiteMode expectedAccessSameSite = SameSiteMode.Lax;
            SameSiteMode expectedRefreshSameSite = SameSiteMode.None;
            var expectedAccessCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = expectedAccessSameSite,
                Expires = DateTimeOffset.UtcNow.AddMinutes(20)
            };
            var expectedRefreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = expectedRefreshSameSite,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            };

            // Act
            builder.UseCookiesWithOptions(new CookieAccessorOptions(expectedAccessCookieOptions, expectedRefreshCookieOptions));
            var actual = builder.Build();

            // Assert
            Assert.NotNull(actual.TokenAccessOptions.CookieAccessorOptions);
            Assert.NotNull(actual.TokenAccessOptions.CookieAccessorOptions.TokenSetOptions);
            Assert.Equal(expectedAccessSameSite, actual.TokenAccessOptions.CookieAccessorOptions.TokenSetOptions.SameSite);
            Assert.Equal(expectedRefreshSameSite, actual.TokenAccessOptions.CookieAccessorOptions.RefreshSetTokenOptions.SameSite);
        }

        [Fact]
        public void WithTokenService_JwtTokenService_ThrowsInvalidOperation()
        {
            // Arrange
            var builder = GetAuthenticationOptionsBuilder(true, false, true, true);
            Type expectedExceptionType = typeof(InvalidOperationException);
            string expectedMessage = $"Please use {nameof(AuthenticationOptionsBuilder.WithDefaultTokenService)} instead";

            bool exceptionThrown = false;
            string actualMessage = string.Empty;
            Exception? actualException = null;

            // Act
            try
            {
                builder.WithTokenService<JwtTokenService>();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                actualException = ex;
                actualMessage = ex.Message;
            }

            // Assert
            Assert.True(exceptionThrown);
            Assert.IsType(expectedExceptionType, actualException);
            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public void AddScheme_DuplicateAuthenticationScheme_ThrowsInvalidOperation()
        {
            // Arrange
            var builder = GetAuthenticationOptionsBuilderBaseline();

            string authenticationScheme = "fakeScheme";
            Type expectedExceptionType = typeof(InvalidOperationException);
            string expectedMessage = $"Scheme `{authenticationScheme}` already added.";

            bool exceptionThrown = false;
            string actualMessage = string.Empty;
            Exception? actualException = null;


            builder.AddScheme<FakeAuthenticationHandler>(authenticationScheme, null, null);
            // Act
            try
            {
                builder.AddScheme<FakeAuthenticationHandler2>(authenticationScheme, null, null);
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                actualException = ex;
                actualMessage = ex.Message;
            }

            // Assert
            Assert.True(exceptionThrown);
            Assert.IsType(expectedExceptionType, actualException);
            Assert.Equal(expectedMessage, actualMessage);
        }

        private AuthenticationOptionsBuilder GetAuthenticationOptionsBuilder(bool withCookieCall = false, bool withTokenServiceCall = false, bool withConfigurationCall = false,
            bool withUserAuthenticatorCall = false)
        {
            var builder = new AuthenticationOptionsBuilder();
            if (withCookieCall)
            {
                builder.UseCookies();
            }
            if (withTokenServiceCall)
            {
                builder.WithTokenService<FakeTokenService>();
            }
            if (withConfigurationCall)
            {
                var mockConfiguration = Substitute.For<IConfiguration>();
                builder.WithConfiguration(mockConfiguration);
            }
            if (withUserAuthenticatorCall)
            {
                builder.WithUserAuthenticator<FakeUserAuthenticator>();
            }
            return builder;
        }

        private AuthenticationOptionsBuilder GetAuthenticationOptionsBuilderBaseline()
        {
            return GetAuthenticationOptionsBuilder(true, true, true, true);
        }
    }
}