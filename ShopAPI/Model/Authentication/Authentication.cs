using ShopAPI.Authorization;

namespace ShopAPI.Model.Authentication
{
    public class Authentication
    {
        public Authentication(Guid id, string authType, string authToken, string authLabel, Guid userId)
        {
            Id = id;
            AuthType = authType;
            AuthToken = authToken;
            AuthLabel = authLabel;
            UserId = userId;
        }

        public static Authentication GenerateAuthentication<X,Y>(AuthenticationMethod<X,Y> method,AuthenticationDescriptor descriptor,UserAccount userAccount)
        {
            return new Authentication(
                Guid.NewGuid(),
                descriptor.AuthType,
                descriptor.AuthToken,
                descriptor.AuthLabel,
                userAccount.Id
                );
        }

        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string AuthType { get; private set; }
        public string AuthToken { get; private set; }
        public string AuthLabel { get; private set; }

        public UserAccount User { get; private set; }
    }
    public record AuthenticationDescriptor(string AuthLabel,string AuthType, string AuthToken)
    {
        public string AuthLabel { get; private set; } = AuthLabel;
        public string AuthType { get; private set; } = AuthType;
        public string AuthToken { get; private set; } = AuthToken;
    }
}
