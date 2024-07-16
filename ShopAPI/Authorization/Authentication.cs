using System;
using System.Security.Cryptography;
using System.Text;
using JsonTokens.Components;
using Newtonsoft.Json.Linq;
using ShopAPI.Model.Authentication;
using Isopoh.Cryptography.Argon2;
using UtilitiesX.Extentions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace ShopAPI.Authorization
{
    public static class AuthenticationService
    {
        static Dictionary<string, AuthenticationMethodFactory<string, string>> Authentications
            = new Dictionary<string, AuthenticationMethodFactory<string, string>>();

        static AuthenticationService()
        {
            var passwordAuthenticationFactory = new PasswordAuthenticationFactory();
            Authentications.Add(passwordAuthenticationFactory.Id, passwordAuthenticationFactory);
        }
        public static bool ContainsAuthentications(string id)
        {
            return Authentications.ContainsKey(id);
        }
        public static AuthenticationMethodFactory<string,string> GetAuthenticationFactory(string id)
        {
            return Authentications[id];
        }
        public static List<(string id,string name)> GetAuthenticationMethodsList()
        {
            return Authentications.Collect(new List<(string, string)>(), (f, c) =>
            {
                c.Add((f.Key, f.Value.Name));
                return c;
            });
        }
    }
    public class AuthenticationProgram
    {
        //an authentication interface
        public uint MethodsCount { get; set; }
        public uint MethodsAuthenticated { get; set; }
        public uint RequiredAuthentications { get; set; }

    }
    public interface AuthenticationMethodFactory<T,V>
    {
        public string Id { get; }
        public string Name { get; }
        public ulong Version { get; }
        public AuthenticationMethod<T, V> Create(string input);
    }
    public class PasswordAuthenticationFactory : AuthenticationMethodFactory<string,string>
    {
        public string Id => "PWD-0-SHA256";

        public string Name => "Password-0 SHA256";

        public ulong Version => 0;

        public AuthenticationMethod<string, string> Create(string input)
        {
            return new PasswordSHA256V0Authentication(GenerateAuthToken(input));
        }
        public string GenerateAuthToken(string input)
        {
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
        }
    }
    public class PasswordSHA256V0Authentication : AuthenticationMethod<string,string>
    {
        public string Name => "PWD-SHA256-0";
        public string PublicName => "Password";
        private string _authToken;
        string AuthenticationMethod<string, string>.AuthToken => _authToken;
        public PasswordSHA256V0Authentication(string authToken)
        {
            this._authToken = authToken;
        }

        public bool Authenticate(string userInputToken, string InitiationResult)
        {
            byte[] a = SHA256.HashData(Encoding.UTF8.GetBytes(userInputToken));
            byte[] b = Convert.FromBase64String(_authToken);
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }
        public (bool skipInitiation, string initiateResult) InitiateAuthentication(string userInput)
        {
            return (true, "");
        }
    }

    public interface AuthenticationMethod<T,V>
    {
        public string Name { get; }
        public string PublicName { get; }
        protected string AuthToken { get; }
        public (bool skipInitiation,T initiateResult) InitiateAuthentication(V userInput);
        public bool Authenticate(string userInputToken,T InitiationResult);
    }
    public class AuthenticationCreationDescriptor
    {
        public AuthenticationCreationDescriptor(AuthenticationDescriptor descriptor, AuthenticationMethod<string, string> authenticationMethod, AuthenticationTracker authenticationTracker)
        {
            Descriptor = descriptor;
            AuthenticationMethod = authenticationMethod;
            AuthenticationTracker = authenticationTracker;
        }

        public AuthenticationDescriptor Descriptor { get; }
        public AuthenticationMethod<string,string> AuthenticationMethod { get; }
        public AuthenticationTracker AuthenticationTracker { get; }

    }
    public class AuthenticationTracker
    {
        public AuthenticationTracker(string initiationResult)
        {
            InitiationResult = initiationResult;
        }

        public string InitiationResult { get; }
    }
    public class AccountCreationDescriptor
    {
        public AccountCreationDescriptor(UserAccountDTO user)
        {
            User = user;
        }

        public UserAccountDTO User { get; set; }
    }
}
