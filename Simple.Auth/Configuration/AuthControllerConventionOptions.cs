using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Configuration
{
    public class AuthControllerConventionOptions
    {
        public readonly List<Type> AuthControllerTypes;
        public AuthControllerConventionOptions(List<Type> authControllerTypes)
        {
            AuthControllerTypes = authControllerTypes;
        }
    }
}
