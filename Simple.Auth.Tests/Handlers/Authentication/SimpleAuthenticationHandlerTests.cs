using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Simple.Auth.Enums;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Middleware.Handlers.Authentication;
using Simple.Auth.Services;
using Simple.Auth.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using static Simple.Auth.Constants;

namespace Simple.Auth.Tests.Handlers.Authentication
{
    [ExcludeFromCodeCoverage]
    public class SimpleAuthenticationHandlerTests
    {
        [Fact]
        public async Task AuthenticateAsync_SessionStateNone_Fails()
        {
            // Arrange
            var mockAuthenticationService = Substitute.For<Interfaces.Authentication.IAuthenticationService>();
            var scheme = new Microsoft.AspNetCore.Authentication.AuthenticationScheme(Constants.Schemes.DEFAULT, "Simple Scheme", typeof(SimpleAuthenticationHandler));
            var handler = GetSimpleAuthenticationHandlerInstance(mockAuthenticationService);
            var context = new DefaultHttpContext();
            await handler.InitializeAsync(scheme, context);
            var expectedErrorMessage = "Unauthorized: No Session or Session expired";
            // Act
            var result = await handler.AuthenticateAsync();

            Assert.False(result.Succeeded);
            Assert.NotNull(result.Failure);
            Assert.Equal(expectedErrorMessage, result.Failure.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_SessionStateRefreshValid_And_TryRefreshAccessAsyncFalse_Fails()
        {
            // Arrange
            var mockAuthenticationService = Substitute.For<Interfaces.Authentication.IAuthenticationService>();
            mockAuthenticationService.GetSessionStateAsync().Returns(SessionState.RefreshValid);
            mockAuthenticationService.TryRefreshAccessAsync().Returns(false);

            var scheme = new Microsoft.AspNetCore.Authentication.AuthenticationScheme(Constants.Schemes.DEFAULT, "Simple Scheme", typeof(SimpleAuthenticationHandler));
            var handler = GetSimpleAuthenticationHandlerInstance(mockAuthenticationService);
            var context = new DefaultHttpContext();
            await handler.InitializeAsync(scheme, context);
            var expectedErrorMessage = "Unauthorized: Session could not be refreshed";
            // Act
            var result = await handler.AuthenticateAsync();

            Assert.False(result.Succeeded);
            Assert.NotNull(result.Failure);
            Assert.Equal(expectedErrorMessage, result.Failure.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_SessionStateValid_And_RefreshThresholdFine_DoesNotCall_TryRefreshAccessAsync()
        {
            // Arrange
            var mockAuthenticationService = Substitute.For<Interfaces.Authentication.IAuthenticationService>();
            mockAuthenticationService.GetSessionStateAsync().Returns(SessionState.Valid);
            mockAuthenticationService.GetTokenExpiry().Returns(DateTimeOffset.UtcNow.AddDays(1));
            mockAuthenticationService.AuthenticateAsync()
                .Returns(Models.AuthenticationResult.Success(TestObjectFactory.GetClaimsPrincipal()));

            var scheme = new Microsoft.AspNetCore.Authentication.AuthenticationScheme(Constants.Schemes.DEFAULT, "Simple Scheme", typeof(SimpleAuthenticationHandler));
            var handler = GetSimpleAuthenticationHandlerInstance(mockAuthenticationService);
            var context = new DefaultHttpContext();
            await handler.InitializeAsync(scheme, context);
            // Act
            _ = await handler.AuthenticateAsync();
            // Assert
            await mockAuthenticationService.DidNotReceive().TryRefreshAccessAsync();
        }

        [Fact]
        public async Task AuthenticateAsync_SessionStateValid_And_RefreshThresholdMet_Calls_TryRefreshAccessAsync()
        {
            // Arrange
            var mockAuthenticationService = Substitute.For<Interfaces.Authentication.IAuthenticationService>();
            mockAuthenticationService.GetSessionStateAsync().Returns(SessionState.Valid);
            mockAuthenticationService.GetTokenExpiry().Returns(DateTimeOffset.UtcNow.AddMinutes(1));
            mockAuthenticationService.AuthenticateAsync()
                .Returns(Models.AuthenticationResult.Success(TestObjectFactory.GetClaimsPrincipal()));

            var scheme = new Microsoft.AspNetCore.Authentication.AuthenticationScheme(Constants.Schemes.DEFAULT, "Simple Scheme", typeof(SimpleAuthenticationHandler));
            var handler = GetSimpleAuthenticationHandlerInstance(mockAuthenticationService);
            var context = new DefaultHttpContext();
            await handler.InitializeAsync(scheme, context);
            // Act
            _ = await handler.AuthenticateAsync();
            // Assert
            await mockAuthenticationService.Received().TryRefreshAccessAsync();
        }

        [Fact]
        public async Task AuthenticateAsync_SessionStateValid_Succeeds()
        {
            // Arrange
            var mockAuthenticationService = Substitute.For<Interfaces.Authentication.IAuthenticationService>();
            mockAuthenticationService.GetSessionStateAsync().Returns(SessionState.Valid);
            mockAuthenticationService.GetTokenExpiry().Returns(DateTimeOffset.UtcNow.AddDays(1));
            mockAuthenticationService.AuthenticateAsync()
                .Returns(Models.AuthenticationResult.Success(TestObjectFactory.GetClaimsPrincipal()));

            var scheme = new Microsoft.AspNetCore.Authentication.AuthenticationScheme(Constants.Schemes.DEFAULT, "Simple Scheme", typeof(SimpleAuthenticationHandler));
            var handler = GetSimpleAuthenticationHandlerInstance(mockAuthenticationService);
            var context = new DefaultHttpContext();
            await handler.InitializeAsync(scheme, context);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            Assert.True(result.Succeeded);
            Assert.Null(result.Failure);
        }

        [Fact]
        public async Task AuthenticateAsync_ExceptionThrown_FailsGracefully()
        {
            // Arrange
            var mockAuthenticationService = Substitute.For<Interfaces.Authentication.IAuthenticationService>();
            mockAuthenticationService.GetSessionStateAsync().Returns(SessionState.Valid);
            mockAuthenticationService.GetTokenExpiry().Returns(DateTimeOffset.UtcNow.AddDays(1));
            mockAuthenticationService.AuthenticateAsync().Throws(new Exception());

            var scheme = new Microsoft.AspNetCore.Authentication.AuthenticationScheme(Constants.Schemes.DEFAULT, "Simple Scheme", typeof(SimpleAuthenticationHandler));
            var handler = GetSimpleAuthenticationHandlerInstance(mockAuthenticationService);
            var context = new DefaultHttpContext();
            await handler.InitializeAsync(scheme, context);
            string expectedMessage = "Exception of type 'System.Exception' was thrown.";

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Failure);
            Assert.Equal(expectedMessage, result.Failure.Message);
        }

        private SimpleAuthenticationHandler GetSimpleAuthenticationHandlerInstance(Interfaces.Authentication.IAuthenticationService authenticationService)
        {
            var mockCorrelationService = Substitute.For<ICorrelationService>();
            var mockLoggerFactory = TestObjectFactory.GetLoggerFactory();
            var mockOptions = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
            mockOptions.Get(Arg.Any<string>()).Returns(new AuthenticationSchemeOptions());
            var scheme = new Microsoft.AspNetCore.Authentication.AuthenticationScheme(Constants.Schemes.DEFAULT, "Simple Scheme", typeof(SimpleAuthenticationHandler));
            return new SimpleAuthenticationHandler(mockOptions, mockLoggerFactory, UrlEncoder.Default,
                authenticationService, mockCorrelationService);
        }
    }
}