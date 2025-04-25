namespace Simple.Auth.Interfaces.Authentication
{
    public interface ITokenAccessor : IHttpContextSwitchable
    {
        /// <summary>
        /// This method will try get the access or session token from the request
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True if a token is present</returns>
        bool TryGetToken(out string token);

        /// <summary>
        /// This method will try get refresh token from the request
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True if a token is present</returns>
        bool TryGetRefreshToken(out string token);

        /// <summary>
        /// This method will set the access or session token for the response
        /// </summary>
        /// <param name="token"></param>
        void SetToken(string token);

        /// <summary>
        /// This method will set the refresh token for the response
        /// </summary>
        /// <param name="token"></param>
        void SetRefreshToken(string token);

        void RemoveTokens();
    }
}