using Microsoft.EntityFrameworkCore;
using ShopAPI.Model.Moderation;

namespace ShopAPI.Model.Users
{
    public class UserAccount
    {
        public UserAccount()
        {
            this.Id = Guid.NewGuid();
            this.CreatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            this.SetRole(UserRoles.PERMISSIONS_DefaultUserAccount);
        }
        public UserAccount(UserAccount accountPublicData)
        {
            this.Id = Guid.NewGuid();
            this.CreatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            this.Email = accountPublicData.Email;
            this.UserName = accountPublicData.UserName;
            this.Password = accountPublicData.Password;
            this.FirstName = accountPublicData.FirstName;
            this.LastName = accountPublicData.LastName;
            this.DateOfBirth = accountPublicData.DateOfBirth;
        }
        public Guid Id { get; private set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }  
        private Permissions Role { get; set; }
        public void SetRole(Permissions role)
        {
            this.Role = role;
        }
        public Permissions GetRole()
        {
            return this.Role;
        }
        public long CreatedAt { get; }
        public DateOnly DateOfBirth { get; set; }
    }
}
