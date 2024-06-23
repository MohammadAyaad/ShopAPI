namespace ShopAPI.Model.Moderation;

public static class UserRoles
{
    public const Permissions PERMISSIONS_AnonymousUser = Permissions.READ_PRODUCTS | Permissions.READ_PACKAGES;
    public const Permissions PERMISSIONS_DefaultUserAccount = Permissions.READ_ACCOUNT |
                                                              Permissions.READ_PRODUCTS |
                                                              Permissions.ORDER_PRODUCTS |
                                                              Permissions.REVIEW_PRODUCTS |
                                                              Permissions.READ_PACKAGES |
                                                              Permissions.REVIEW_PACKAGES |
                                                              Permissions.READ_ACCOUNT |
                                                              Permissions.READ_ACCOUNT_ORDERS |
                                                              Permissions.MANAGE_ACCOUNT |
                                                              Permissions.MANAGE_ACCOUNT_ORDERS |
                                                              Permissions.DELETE_ACCOUNT;
    public const Permissions PERMISSIONS_UserAccountDeleteAccessToken = Permissions.DELETE_ACCOUNT;
    public const Permissions PERMISSIONS_Adminstrator = (Permissions)0x7fffffff; //everything except creation of new admins
    public const Permissions PERMISSIONS_Owner = (Permissions)0xffffffff; //everything

    private static readonly Dictionary<string, Permissions> permissionTable = new Dictionary<string, Permissions>()
    {
        { "anonymous",PERMISSIONS_AnonymousUser },
        { "default",PERMISSIONS_DefaultUserAccount },
        { "delete_account",PERMISSIONS_UserAccountDeleteAccessToken},
        { "admin",PERMISSIONS_Adminstrator },
        { "owner",PERMISSIONS_Owner }
    };

    public static Dictionary<string, Permissions> PermissionTable => permissionTable;
}
