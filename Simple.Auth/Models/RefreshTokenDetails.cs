namespace Simple.Auth.Models
{
    public class RefreshTokenDetails
    {
        public RefreshTokenDetails(string token, string ipAddress, DateTimeOffset expiry)
        {
            Token = token;
            IpAddress = ipAddress;
            Expiry = expiry;
        }

        public RefreshTokenDetails()
        { }

        public string Token { get; set; }
        public string IpAddress { get; set; }
        public DateTimeOffset Expiry { get; set; }
    }
}