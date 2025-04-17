using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Models
{
    [ExcludeFromCodeCoverage]
    public class AuthenticationResult
    {
        public readonly ClaimsPrincipal? Principal;
        public readonly Exception? Exception;
        public readonly string Message;
        public bool Succeeded => Principal != null && Exception == null;

        private AuthenticationResult(ClaimsPrincipal? principal, Exception? exception, string message)
        {
            Principal = principal;
            Exception = exception;
            Message = message;
        }

        public static AuthenticationResult Success(ClaimsPrincipal principal)
                    => new AuthenticationResult(principal, null, "Success");

        public static AuthenticationResult Failure(Exception exception)
                    => new AuthenticationResult(null, exception, exception.Message);

        public static AuthenticationResult Failure(string message)
        => new AuthenticationResult(null, null, message);

        public static AuthenticationResult Failure(string message, Exception exception)
                    => new AuthenticationResult(null, exception, message);
    }
}