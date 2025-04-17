using Microsoft.AspNetCore.Http;
using NSubstitute;
using Simple.Auth.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Tests.Helpers
{
    public class HttpContextExtensionsTests
    {
        [Fact]
        public void GetClientIpAddress_HttpContextNull_ReturnsEmptyString()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            httpContextAccessor.HttpContext.Returns(null as HttpContext);

            // Act
            var result = httpContextAccessor.GetClientIpAddress();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetClientIpAddress_XForwardedForPresent_ReturnsFirstIpAddress()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var httpContext = Substitute.For<HttpContext>();
            var httpRequest = Substitute.For<HttpRequest>();
            var headers = Substitute.For<IHeaderDictionary>();

            headers["X-Forwarded-For"].Returns((Microsoft.Extensions.Primitives.StringValues)"192.168.1.10, 10.0.0.5");
            httpRequest.Headers.Returns(headers);
            httpContext.Request.Returns(httpRequest);
            httpContextAccessor.HttpContext.Returns(httpContext);

            // Act
            var result = httpContextAccessor.GetClientIpAddress();

            // Assert
            Assert.Equal("192.168.1.10", result);
        }

        [Fact]
        public void GetClientIpAddress_XForwardedForPresent_SingleIpAddress()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var httpContext = Substitute.For<HttpContext>();
            var httpRequest = Substitute.For<HttpRequest>();
            var headers = Substitute.For<IHeaderDictionary>();

            headers["X-Forwarded-For"].Returns((Microsoft.Extensions.Primitives.StringValues)"203.0.113.45");
            httpRequest.Headers.Returns(headers);
            httpContext.Request.Returns(httpRequest);
            httpContextAccessor.HttpContext.Returns(httpContext);

            // Act
            var result = httpContextAccessor.GetClientIpAddress();

            // Assert
            Assert.Equal("203.0.113.45", result);
        }

        [Fact]
        public void GetClientIpAddress_XForwardedForEmpty_RemoteIpAddressPresent_ReturnsRemoteIp()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var httpContext = Substitute.For<HttpContext>();
            var httpRequest = Substitute.For<HttpRequest>();
            var connectionInfo = Substitute.For<ConnectionInfo>();
            var headers = Substitute.For<IHeaderDictionary>();
            System.Net.IPAddress remoteIp = System.Net.IPAddress.Parse("172.16.0.1");

            headers["X-Forwarded-For"].Returns((Microsoft.Extensions.Primitives.StringValues)string.Empty);
            httpRequest.Headers.Returns(headers);
            httpContext.Request.Returns(httpRequest);
            connectionInfo.RemoteIpAddress.Returns(remoteIp);
            httpContext.Connection.Returns(connectionInfo);
            httpContextAccessor.HttpContext.Returns(httpContext);

            // Act
            var result = httpContextAccessor.GetClientIpAddress();

            // Assert
            Assert.Equal("172.16.0.1", result);
        }

        [Fact]
        public void GetClientIpAddress_XForwardedForMissing_RemoteIpAddressPresent_ReturnsRemoteIp()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var httpContext = Substitute.For<HttpContext>();
            var httpRequest = Substitute.For<HttpRequest>();
            var connectionInfo = Substitute.For<ConnectionInfo>();
            var headers = Substitute.For<IHeaderDictionary>();
            System.Net.IPAddress remoteIp = System.Net.IPAddress.Parse("198.51.100.68");

            headers.TryGetValue("X-Forwarded-For", out _).Returns(false); // Simulate header not being present
            httpRequest.Headers.Returns(headers);
            httpContext.Request.Returns(httpRequest);
            connectionInfo.RemoteIpAddress.Returns(remoteIp);
            httpContext.Connection.Returns(connectionInfo);
            httpContextAccessor.HttpContext.Returns(httpContext);

            // Act
            var result = httpContextAccessor.GetClientIpAddress();

            // Assert
            Assert.Equal("198.51.100.68", result);
        }

        [Fact]
        public void GetClientIpAddress_XForwardedForEmpty_RemoteIpAddressNull_ReturnsEmptyString()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var httpContext = Substitute.For<HttpContext>();
            var httpRequest = Substitute.For<HttpRequest>();
            var connectionInfo = Substitute.For<ConnectionInfo>();
            var headers = Substitute.For<IHeaderDictionary>();

            headers["X-Forwarded-For"].Returns((Microsoft.Extensions.Primitives.StringValues)string.Empty);
            httpRequest.Headers.Returns(headers);
            httpContext.Request.Returns(httpRequest);
            connectionInfo.RemoteIpAddress.Returns(null as IPAddress);
            httpContext.Connection.Returns(connectionInfo);
            httpContextAccessor.HttpContext.Returns(httpContext);

            // Act
            var result = httpContextAccessor.GetClientIpAddress();

            // Assert
            Assert.Empty(result);
        }
    }
}
