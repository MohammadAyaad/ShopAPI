using Microsoft.EntityFrameworkCore;
using ShopAPI.Model.Moderation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;

namespace ShopAPI.Model.Authentication;

public class UserAccountPrivateIdentity
{
    public Guid Id { get; private set; }
}

public class UserPublicIdentity
{
    public string Email { get; set; }
    public string UserName { get; set; }
}

public class UserAccount
{
    public UserAccount()
    {
        CreatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        SetRole(UserRoles.PERMISSIONS_DefaultUserAccount);
    }
    public UserAccount(UserAccount accountPublicData)
    {
        CreatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        Email = accountPublicData.Email;
        UserName = accountPublicData.UserName;
        FirstName = accountPublicData.FirstName;
        LastName = accountPublicData.LastName;
        DateOfBirth = accountPublicData.DateOfBirth;
    }
    public UserAccountDTO GetDTO()
    {
        return new UserAccountDTO(
            Email,
            UserName,
            FirstName,
            LastName,
            CreatedAt,
            DateOfBirth
            );
    }
    public Guid Id { get; private set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public Guid GetId() => Id;
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Permissions Role { get; set; }
    public void SetRole(Permissions role) => Role = role;
    public Permissions GetRole() => Role;
    public long CreatedAt { get; }
    public DateOnly DateOfBirth { get; set; }
    public ICollection<Authentication> AuthenticationMethods { get; set; }
    public void ModifyByDTO(UserAccountDTO userAccountDTO)
    {
        Email = userAccountDTO.Email;
        UserName = userAccountDTO.UserName;
        FirstName = userAccountDTO.FirstName;
        LastName = userAccountDTO.LastName;
        DateOfBirth = userAccountDTO.DateOfBirth;
    }
    public static implicit operator UserAccountDTO(UserAccount account)
    {
        return account.GetDTO();
    }
}

public class UserAccountDTO
{
    public UserAccountDTO(string email, string userName, string firstName, string lastName, long createdAt, DateOnly dateOfBirth)
    {
        Email = email;
        UserName = userName;
        FirstName = firstName;
        LastName = lastName;
        CreatedAt = createdAt;
        DateOfBirth = dateOfBirth;
    }
    private string _email;
    public string Email { 
        get 
        {
            return this._email;
        } 
        set
        {
            if (!string.IsNullOrEmpty(value)) {
                try { 
                    new MailAddress(value);
                }
                catch(Exception ex)
                {
                    throw new InvalidEmailException();
                }
                this._email = value;
                return;
            }
            throw new InvalidEmailException();
        }
    }
    private string _username;
    public string UserName
    {
        get
        {
            return this._username;
        }
        set
        {
            if (
                value.Length < 64 &&
                value.Length > 0
                )
                this._username = value;
            else throw new InvalidEmailException();
        }
    }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public long CreatedAt { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public UserAccount ToNewUserAccount()
    {
        var u = new UserAccount();
        u.Email = Email;
        u.UserName = UserName;
        u.FirstName = FirstName;
        u.LastName = LastName;
        u.DateOfBirth = DateOfBirth;
        return u;
    }
}



[Serializable]
public class InvalidEmailException : Exception
{
    public InvalidEmailException() { }
    public InvalidEmailException(string message) : base(message) { }
    public InvalidEmailException(string message, Exception inner) : base(message, inner) { }
    protected InvalidEmailException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}


[Serializable]
public class InvalidUsernameException : Exception
{
    public InvalidUsernameException() { }
    public InvalidUsernameException(string message) : base(message) { }
    public InvalidUsernameException(string message, Exception inner) : base(message, inner) { }
    protected InvalidUsernameException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}


