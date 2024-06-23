using ShopAPI.Model.Moderation;

namespace ShopAPI.Model.Users;

public class UserAccount
{
    public Guid Id { get; set; }
    public Guid GetId() => Id;
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public long CreatedAt { get; }

    public Permissions Role { get; set; }
    public DateOnly DateOfBirth { get; set; }

    public Permissions GetRole() => this.Role;
    public void SetRole(Permissions role) => this.Role = role;

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

    public UserAccountDTO DTO => new UserAccountDTO()
    {
        Email = this.Email,
        UserName = this.UserName,
        FirstName = this.FirstName,
        LastName = this.LastName,
        CreatedAt = this.CreatedAt,
        DateOfBirth = this.DateOfBirth,
    };

    public void ModifyByDTO(UserAccountDTO userAccountDTO)
    {
        this.Email = userAccountDTO.Email;
        this.UserName = userAccountDTO.UserName;
        this.FirstName = userAccountDTO.FirstName;
        this.LastName = userAccountDTO.LastName;
        this.DateOfBirth = userAccountDTO.DateOfBirth;
    }
}

public class UserAccountDTO
{
    public string Email { get; set; }
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public long CreatedAt { get; set; }
    public DateOnly DateOfBirth { get; set; }

    public UserAccount ToNewUserAccount
    {
        get
        {
            var u = new UserAccount();
            u.Email = this.Email;
            u.UserName = this.UserName;
            u.FirstName = this.FirstName;
            u.LastName = this.LastName;
            u.DateOfBirth = this.DateOfBirth;
            return u;
        }
    }
}
