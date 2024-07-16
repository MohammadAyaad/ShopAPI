namespace ShopAPI.Model.Authentication
{
    public class JwtAccessTokenDescriptor
    {

        public JwtAccessTokenDescriptor(string email, string secret, long expiresAt)
        {
            Id = Guid.NewGuid();
            Email = email;
            Secret = secret;
            ExpiresAt = expiresAt;
        }

        public Guid Id { get; private set; }
        public string Email { get; set; }
        public string Secret { get; set; }
        public long ExpiresAt { get; set; }
    }
}
