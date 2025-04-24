using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Simple.Auth.Interfaces;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Tests.Authentication.Fakes;
using Simple.Auth.Tests.TestImplementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Tests.Authentication.Controllers
{
    public class AuthenticationControllerBaseTests
    {
        [Fact]
        public async Task GetMeAsync_UserIsAuthenticated_ReturnsOkWithClaims()
        {
            // Arrange
            var loggerFactory = Substitute.For<ICorrelationLoggerFactory>();
            var authenticationService = Substitute.For<IAuthenticationService>();
            var controller = new TestAuthenticationController(loggerFactory, authenticationService);

            // Simulate an authenticated user with claims
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user123"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim("role", "Admin")
        };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            // Set the controller's HttpContext with the simulated user
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await controller.GetMeAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualPrinciple = Assert.IsType<ClaimsPrincipal>(okResult.Value);
            var actualClaims = actualPrinciple.Claims.ToDictionary(x=>x.Type, x => x.Value);
            Assert.Equal(3, actualPrinciple.Claims.Count());
            Assert.Equal("user123", actualClaims[ClaimTypes.NameIdentifier]);
            Assert.Equal("Test User", actualClaims[ClaimTypes.Name]);
            Assert.Equal("Admin", actualClaims["role"]);
        }

        [Fact]
        public async Task GetMeAsync_UserIsNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var loggerFactory = Substitute.For<ICorrelationLoggerFactory>();
            var authenticationService = Substitute.For<IAuthenticationService>();
            var controller = new TestAuthenticationController(loggerFactory, authenticationService);

            // Simulate an unauthenticated user
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await controller.GetMeAsync();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task LoginAsync_ValidRequest_CallsStartSessionAndReturnsOk()
        {
            // Arrange
            var loginRequest = new TestLoginRequest { UserName = "testuser", Password = "testpassword" };
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test"),
                new Claim(ClaimTypes.Name, "test"),
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Constants.Schemes.DEFAULT));
            var loggerFactory = Substitute.For<ICorrelationLoggerFactory>();
            var authenticationService = Substitute.For<IAuthenticationService>();
            authenticationService.StartSessionAsync(Arg.Is<TestLoginRequest>(x => x.UserName == loginRequest.UserName))
                .Returns(("test", "test", principal));
            var controller = new TestAuthenticationController(loggerFactory, authenticationService);

            // Act
            var result = await controller.LoginAsync(loginRequest);

            // Assert
            Assert.IsType<OkResult>(result);
            await authenticationService.Received(1).StartSessionAsync(loginRequest);
        }

        [Fact]
        public async Task LoginAsync_WrongCredentials_CallsStartSessionAndReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new TestLoginRequest { UserName = "testuser", Password = "testpassword" };
            var loggerFactory = Substitute.For<ICorrelationLoggerFactory>();
            var authenticationService = Substitute.For<IAuthenticationService>();
            authenticationService.StartSessionAsync(Arg.Is<TestLoginRequest>(x => x.UserName == loginRequest.UserName))
                .Throws<ArgumentException>();
            var controller = new TestAuthenticationController(loggerFactory, authenticationService);

            // Act
            var result = await controller.LoginAsync(loginRequest);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
            await authenticationService.Received(1).StartSessionAsync(loginRequest);
        }

        [Fact]
        public async Task LoginAsync_Exception_CallsStartSessionAndReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new TestLoginRequest { UserName = "testuser", Password = "testpassword" };
            var loggerFactory = Substitute.For<ICorrelationLoggerFactory>();
            var authenticationService = Substitute.For<IAuthenticationService>();
            authenticationService.StartSessionAsync(Arg.Is<TestLoginRequest>(x => x.UserName == loginRequest.UserName))
                .Throws<Exception>();
            var controller = new TestAuthenticationController(loggerFactory, authenticationService);

            // Act
            var result = await controller.LoginAsync(loginRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await authenticationService.Received(1).StartSessionAsync(loginRequest);
        }
        [Fact]
        public async Task LogoutAsync_Exception_CallsEndSessionAsyncAndReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new TestLoginRequest { UserName = "testuser", Password = "testpassword" };
            var loggerFactory = Substitute.For<ICorrelationLoggerFactory>();
            var authenticationService = Substitute.For<IAuthenticationService>();
            authenticationService.EndSessionAsync()
                .Throws<Exception>();
            var controller = new TestAuthenticationController(loggerFactory, authenticationService);

            // Act
            var result = await controller.LogoutAsync();

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await authenticationService.Received(1).EndSessionAsync();
        }
        [Fact]
        public async Task LogoutAsync_ValidRequest_CallsEndSessionAsyncAndReturnsOk()
        {
            // Arrange
            var loginRequest = new TestLoginRequest { UserName = "testuser", Password = "testpassword" };
            var loggerFactory = Substitute.For<ICorrelationLoggerFactory>();
            var authenticationService = Substitute.For<IAuthenticationService>();

            var controller = new TestAuthenticationController(loggerFactory, authenticationService);

            // Act
            var result = await controller.LogoutAsync();

            // Assert
            Assert.IsType<OkResult>(result);
            await authenticationService.Received(1).EndSessionAsync();
        }
    }
}
