using Simple.Auth.Demo.Models;
using Simple.Auth.Interfaces.Authentication;
using Simple.Auth.Models;
using System.Security.Claims;

namespace Simple.Auth.Demo.Services
{
    public class UserAuthenticator : IUserAuthenticator
    {
        private readonly User[] users;
        public UserAuthenticator()
        {
            users = new User[]
        {
            new User { Id = Guid.NewGuid().ToString(), Name = "Alice Wonderland", Email = "alice@example.com", Password = "password123" },
            new User { Id = Guid.NewGuid().ToString(), Name = "Bob The Builder", Email = "bob@example.com", Password = "buildit456" },
            new User { Id = Guid.NewGuid().ToString(), Name = "Charlie Chaplin", Email = "charlie@example.com", Password = "silent789" },
            new User { Id = Guid.NewGuid().ToString(), Name = "Diana Prince", Email = "diana@example.com", Password = "wonderwoman" },
            new User { Id = Guid.NewGuid().ToString(), Name = "Eve Harrington", Email = "eve@example.com", Password = "ingenue007" }
        };
        }
        public async Task<AuthenticationResult> AuthenticateUserAsync(object request)
        {
            dynamic details = (dynamic)request;
            var user = users.FirstOrDefault(u => u.Email.ToLower() == details.Email.ToLower() && u.Password== details.Password);
            if (user == null)
            {
                return AuthenticationResult.Failure("Invalid username/password");
            }
            var principle = GetClaimsPrincipal(user);
            return AuthenticationResult.Success(principle);
        }

        public async Task<AuthenticationResult> AuthenticateUserAsync(string accessToken)
        {
            return AuthenticationResult.Success(GetClaimsPrincipal(users[1]));
        }

        private ClaimsPrincipal GetClaimsPrincipal(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name),
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims));
        }
    }
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
