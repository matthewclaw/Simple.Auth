using Microsoft.AspNetCore.Mvc.ApplicationModels;
using NSubstitute;
using Simple.Auth.Configuration;
using Simple.Auth.Controllers.Authentication;
using Simple.Auth.Controllers.Conventions;
using Simple.Auth.Tests.TestImplementations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Tests.Authentication.Controllers.Conventions
{
    [ExcludeFromCodeCoverage]
    public class AuthControllerConventionTests
    {
        [Fact]
        public void Apply_AuthControllerTypeIncluded_DoesNotHideOrClear()
        {
            // Arrange
            var authControllerType = typeof(UsernameAndPasswordAuthController);
            var options = new AuthControllerConventionOptions(new List<Type> { authControllerType });
            var convention = new AuthControllerConvention(options);
            var controllerModel = new ControllerModel(authControllerType.GetTypeInfo(), new List<ActionModel>())
            {
                ApiExplorer = new ApiExplorerModel()
                {
                    IsVisible = true,
                }
            };
            controllerModel.Actions.Add(new ActionModel(Substitute.For<MethodInfo>(), Substitute.For<IReadOnlyList<object>>()));
            // Act
            convention.Apply(controllerModel);

            // Assert
            Assert.True(controllerModel.ApiExplorer.IsVisible.HasValue && controllerModel.ApiExplorer.IsVisible.Value);
            Assert.NotEmpty(controllerModel.Actions);
        }

        [Fact]
        public void Apply_AuthControllerTypeExcluded_DoesHideAndClear()
        {
            // Arrange
            var authControllerType = typeof(UsernameAndPasswordAuthController);
            var options = new AuthControllerConventionOptions(new List<Type>());
            var convention = new AuthControllerConvention(options);
            var controllerModel = new ControllerModel(authControllerType.GetTypeInfo(), new List<ActionModel>())
            {
                ApiExplorer = new ApiExplorerModel()
                {
                    IsVisible = true,
                }
            };
            controllerModel.Actions.Add(new ActionModel(Substitute.For<MethodInfo>(), Substitute.For<IReadOnlyList<object>>()));
            // Act
            convention.Apply(controllerModel);

            // Assert
            Assert.True(controllerModel.ApiExplorer.IsVisible.HasValue && !controllerModel.ApiExplorer.IsVisible.Value);
            Assert.Empty(controllerModel.Actions);
        }

        [Fact]
        public void Apply_AuthControllerExternalType_DoesNotHideOrClear()
        {
            // Arrange
            var authControllerType = typeof(TestAuthenticationController);
            var options = new AuthControllerConventionOptions(new List<Type>());
            var convention = new AuthControllerConvention(options);
            var controllerModel = new ControllerModel(authControllerType.GetTypeInfo(), new List<ActionModel>())
            {
                ApiExplorer = new ApiExplorerModel()
                {
                    IsVisible = true,
                }
            };
            controllerModel.Actions.Add(new ActionModel(Substitute.For<MethodInfo>(), Substitute.For<IReadOnlyList<object>>()));
            // Act
            convention.Apply(controllerModel);

            // Assert
            Assert.True(controllerModel.ApiExplorer.IsVisible.HasValue && controllerModel.ApiExplorer.IsVisible.Value);
            Assert.NotEmpty(controllerModel.Actions);
        }

    }
}
