using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Enums
{
    public enum SessionState
    {
        None = 0,
        Invalid,
        Valid,
        RefreshValid
    }
}
