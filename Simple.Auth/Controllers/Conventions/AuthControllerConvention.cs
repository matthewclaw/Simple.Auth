using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Simple.Auth.Configuration;

namespace Simple.Auth.Controllers.Conventions
{
    public class AuthControllerConvention : IControllerModelConvention
    {
        private readonly List<Type> _authControllerTypes;

        public AuthControllerConvention(AuthControllerConventionOptions options)
        {
            _authControllerTypes = options.AuthControllerTypes;
        }

        public void Apply(ControllerModel model)
        {
            if (model.ControllerType.Assembly != typeof(AuthControllerConvention).Assembly)
            {
                return;
            }
            if (!_authControllerTypes.Contains(model.ControllerType))
            {
                model.ApiExplorer.IsVisible = false; // Hide from API discovery
                model.Actions.Clear(); // Remove all actions, effectively making it unusable
            }
        }
    }
}