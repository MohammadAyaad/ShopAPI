using ShopAPI.Authorization.Permissions_Roles;
using ShopAPI.Authorization;

namespace ShopAPI_ASP.NET_Core.Authorization
{
    public class Queries
    {
        public static (bool success, string info_msg, string token) CreateAccount(MySqlConnection connection, string email, string password_hash_base64, string username, string lastname, DateOnly birthDate)
        {
            (string token, string errormsg) = AccountManager.CreateAccount(connection, email, password_hash_base64, username, lastname, birthDate);
            if (token != null)
            {
                return (true, "Account created successfully!", token);
            }
            else
            {
                return (false, errormsg, null);
            }
        }
        public static (bool success, string info_msg, string token) Login(MySqlConnection connection, string email, string password_hash, Permissions required_permissions)
        {
            (string token, string errormsg) = AccountManager.Login(connection, email, password_hash, required_permissions);
            if (token != null)
            {
                return (true, "Logged in successfully", token);
            }
            else
            {
                return (false, errormsg, null);
            }
        }
        public static void Logout(MySqlConnection connection, string token)
        {
            AccountManager.Logout(connection, token);
        }
        public static void DeleteAccount(MySqlConnection connection, string token)
        {
            AccountManager.DeleteAccount(connection, token);
        }
    }
}
