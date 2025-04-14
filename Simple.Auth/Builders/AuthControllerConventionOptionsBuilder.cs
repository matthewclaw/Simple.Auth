using Simple.Auth.Configuration;
using Simple.Auth.Controllers.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Builders
{
    public class AuthControllerConventionOptionsBuilder
    {
        private List<Type> _types = new List<Type>();
        public AuthControllerConventionOptionsBuilder() { }
        public AuthControllerConventionOptionsBuilder WithClassic()
        {
            _types.Add(typeof(UsernameAndPasswordAuthController));
            return this;
        }
        public AuthControllerConventionOptions Build()
        => new AuthControllerConventionOptions(_types);
    }
}
