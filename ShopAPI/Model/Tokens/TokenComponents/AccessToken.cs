using ShopAPI.Model.Moderation;

namespace ShopAPI.Model.Tokens.TokenComponents
{
    public class AccessToken
    {
        public string Email { get; set; }
        public Permissions Permissions { get; set; }
    }
}
