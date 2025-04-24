using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Enums
{
    public enum AuthenticationCacheType
    {
        None = 0,
        Refresh,
        Principal,
        BlackListed
    }
}
