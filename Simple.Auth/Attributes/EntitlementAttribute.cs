using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Attributes
{
    public class EntitlementAttribute: Attribute
    {
        public string[] Entitlements { get; set; }
    }
}
