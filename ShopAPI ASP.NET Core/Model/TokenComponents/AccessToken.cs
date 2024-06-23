using ShopAPI.Model.Moderation;

namespace ShopAPI.Model.TokenComponents;

public class AccessToken
{
    public string Email { get; set; }

    public Permissions Permissions { get; set; }
}
