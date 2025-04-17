using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Models.Requests
{
    [ExcludeFromCodeCoverage]
    public class UsernameAndPassword
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
