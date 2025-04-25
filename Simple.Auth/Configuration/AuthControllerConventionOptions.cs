namespace Simple.Auth.Configuration
{
    public class AuthControllerConventionOptions
    {
        public readonly List<Type> AuthControllerTypes;

        public AuthControllerConventionOptions(List<Type> authControllerTypes)
        {
            AuthControllerTypes = authControllerTypes;
        }
    }
}