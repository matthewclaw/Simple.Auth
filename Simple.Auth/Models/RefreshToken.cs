using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Models
{
    [ExcludeFromCodeCoverage]
    public class RefreshToken
    {
        public RefreshToken(string token, string ipAddress, DateTimeOffset expiry)
        {
            Token = token;
            IpAddress = ipAddress;
            Expiry = expiry;
        }
        public RefreshToken() { }

        public string Token { get; set; }
        public string IpAddress { get; set; }
        public DateTimeOffset Expiry { get; set; }
    }
}
