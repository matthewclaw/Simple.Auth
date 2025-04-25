using System.Diagnostics.CodeAnalysis;

namespace Simple.Auth.Attributes
{
    [ExcludeFromCodeCoverage]
    public class EntitlementAttribute : Attribute
    {
        public string[] Entitlements { get; set; }
    }
}