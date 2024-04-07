using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopAPI.Authorization.Permissions_Roles
{
    public enum Permissions : uint
    {
        USER_BANNED = 0x00000000,
        //on the user side => the user is banned from using the website
        RESOURCE_OPEN = 0x00000000,
        //on the resource side => the resource open for everyone
        READ_PRODUCTS = 0x00000001,
        EDIT_PRODUCTS = 0x00000002,
        ORDER_PRODUCTS = 0x00000004,
        CREATE_PRODUCTS = 0x00000008,
        DELETE_PRODUCTS = 0x00000010,
        REVIEW_PRODUCTS = 0x00000020,
        READ_PACKAGES = 0x00000040,
        EDIT_PACKAGES = 0x00000080,
        CREATE_PACKAGES = 0x00000100,
        DELETE_PACKAGES = 0x00000200,
        REVIEW_PACKAGES = 0x00000400,
        READ_ACCOUNT = 0x00000800,
        MANAGE_ACCOUNT = 0x00001000,
        DELETE_ACCOUNT = 0x00002000,
        READ_ACCOUNT_ORDERS = 0x00004000,
        CREATE_ACCOUNT_ORDERS = 0x00008000,
        MANAGE_ACCOUNT_ORDERS = 0x00010000,
        CANCEL_ACCOUNT_ORDERS = 0x00020000,
        READ_ACCOUNTS_GENERAL_INFO = 0x00040000,
        LOGOUT_ACCOUNT_ALL = 0x00080000,
        CREATE_ACCOUNT = 0x00100000,
        REVIEW_RATINGS = 0x00200000,

        READ_TECHNICAL_STATISTICS = 0x08000000,
        READ_FINANCIAL_STATISTICS = 0X10000000,
        READ_LOGS = 0x20000000,
        EDIT_ROLES = 0x40000000,
        OWNER = 0x80000000


    }
    public static class UserRoles
    {


        public const Permissions PERMISSIONS_AnonymousUser =
            (
            Permissions.READ_PRODUCTS |
            Permissions.READ_PACKAGES
            );
        public const Permissions PERMISSIONS_DefaultUserAccount =
            (
            Permissions.READ_ACCOUNT |
            Permissions.READ_PRODUCTS |
            Permissions.ORDER_PRODUCTS |
            Permissions.REVIEW_PRODUCTS |
            Permissions.READ_PACKAGES |
            Permissions.REVIEW_PACKAGES |
            Permissions.READ_ACCOUNT |
            Permissions.READ_ACCOUNT_ORDERS |
            Permissions.MANAGE_ACCOUNT |
            Permissions.MANAGE_ACCOUNT_ORDERS |
            Permissions.DELETE_ACCOUNT
            );
        public const Permissions PERMISSIONS_UserAccountDeleteAccessToken = (Permissions.DELETE_ACCOUNT);
        public const Permissions PERMISSIONS_Adminstrator = (Permissions)0x7fffffff; //everything except creation of new admins
        public const Permissions PERMISSIONS_Owner = (Permissions)0xffffffff; //everything

        public static readonly Dictionary<string, Permissions> PermissionTable = new Dictionary<string, Permissions>()
        {
            { "anonymous",PERMISSIONS_AnonymousUser },
            { "default",PERMISSIONS_DefaultUserAccount },
            { "delete_account",PERMISSIONS_UserAccountDeleteAccessToken},
            { "admin",PERMISSIONS_Adminstrator },
            { "owner",PERMISSIONS_Owner }
        };

        //TODO:upcomming update ... (improvement of the way the permissions system is handled)
        /*public static readonly Dictionary<string, Permissions> AccountRoleTable = new Dictionary<string, Permissions>()
        {
            //{ "default_user",}
        };
        public static readonly Dictionary<string, Permissions> TokenPermissionTable = new Dictionary<string, Permissions>()
        {
        };*/
    }
}
