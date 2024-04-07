using Microsoft.EntityFrameworkCore;
using ShopAPI.Authorization.Permissions_Roles;

namespace ShopAPI.Model.Users
{
    public class UserAccount
    {
        public UserAccount()
        {
            this.Id = Guid.NewGuid();
            this.CreatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        public Guid Id { get; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }  
        public Permissions Role { get; private set; }
        public void SetRole(Permissions role)
        {
            this.Role = role;
        }
        public long CreatedAt { get; }
        public DateOnly DateOfBirth { get; set; }
    }
}
