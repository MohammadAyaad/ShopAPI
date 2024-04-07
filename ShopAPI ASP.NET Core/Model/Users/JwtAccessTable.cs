namespace ShopAPI_ASP.NET_Core.Model.Users
{
    public class JwtAccessTable
    {
        public JwtAccessTable() { 
            Id = Guid.NewGuid();
        }
        public Guid Id { get;}
        public string Email { get; set; }
        public string Secret { get; set; }
        public long ExpiresAt { get; set; }
    }
}
