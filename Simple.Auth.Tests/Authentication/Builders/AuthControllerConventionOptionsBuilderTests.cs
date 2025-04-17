using Simple.Auth.Builders;
using Simple.Auth.Controllers.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Tests.Authentication.Builders
{
    public class AuthControllerConventionOptionsBuilderTests
    {
        [Fact]
        public void Build_WithNoMethodCalls_ReturnsOptionsWithNoAuthControllerTypes()
        {
            // Arrange
            var builder = new AuthControllerConventionOptionsBuilder();

            // Act
            var actual = builder.Build();

            // Assert
            Assert.NotNull(actual);
            Assert.Empty(actual.AuthControllerTypes);
        }

        [Fact]
        public void Build_WithClassic_ReturnsOptionsWithOneAuthControllerType()
        {
            // Arrange
            var builder = new AuthControllerConventionOptionsBuilder();
            int expectedAuthControllerTypesCount = 1;
            Type expectedAuthControllerType = typeof(UsernameAndPasswordAuthController);
            // Act
            builder.WithClassic();
            var actual = builder.Build();

            // Assert
            Assert.NotNull(actual);
            Assert.NotEmpty(actual.AuthControllerTypes);
            Assert.Equal(expectedAuthControllerTypesCount, actual.AuthControllerTypes.Count());
            var actualType = actual.AuthControllerTypes.First();
            Assert.Equal(expectedAuthControllerType, actualType);
        }
    }
}
