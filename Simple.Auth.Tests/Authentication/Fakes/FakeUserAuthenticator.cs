using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Tests.Authentication.Fakes
{
    [ExcludeFromCodeCoverage]
    internal class FakeUserAuthenticator : IUserAuthenticator
    {
        public Task<AuthenticationResult> AuthenticateUserAsync(object request)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticationResult> AuthenticateUserAsync(string accessToken)
        {
            throw new NotImplementedException();
        }
    }
}
