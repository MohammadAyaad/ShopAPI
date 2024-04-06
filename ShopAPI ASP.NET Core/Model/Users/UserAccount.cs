using Microsoft.EntityFrameworkCore;

namespace ShopAPI.Model.Users
{
    public class UserAccount
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }  
        public long Role { get; set; }
        public long CreatedAt { get; set; }
        public DateOnly DateOfBirth { get; set; }
    }
}
