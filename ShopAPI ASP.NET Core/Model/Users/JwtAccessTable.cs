namespace ShopAPI.Model.Users
{
    public class JwtAccessTable
    {
        public JwtAccessTable() { 
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }
        public string Email { get; set; }
        public string Secret { get; set; }
        public long ExpiresAt { get; set; }
    }
}
