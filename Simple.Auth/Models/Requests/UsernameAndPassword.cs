using System.Diagnostics.CodeAnalysis;

namespace Simple.Auth.Models.Requests
{
    [ExcludeFromCodeCoverage]
    public class UsernameAndPassword
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}