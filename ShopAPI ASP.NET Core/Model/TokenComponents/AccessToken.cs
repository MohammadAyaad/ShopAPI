using ShopAPI.Authorization.Permissions_Roles;

namespace ShopAPI_ASP.NET_Core.Model.TokenComponents
{
    public class AccessToken
    {
        public string Email { get; set; }
        public Permissions Permissions { get; set; }
    }
}
