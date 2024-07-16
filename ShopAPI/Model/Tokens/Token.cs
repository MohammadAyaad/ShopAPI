using JsonTokens.Components;

namespace ShopAPI.Model.Tokens
{
    public class Token
    {
        public Token(string publicToken, LifeTime lifeTime)
        {
            PublicToken = publicToken;
            LifeTime = lifeTime;
        }

        public string PublicToken { get; set; }
        public LifeTime LifeTime { get; }
        public string AuthorizationHeader { 
            get
            {
                return $"Bearer {PublicToken}";
            }
        }

        public bool ShouldSerializeAuthorizationHeader() => false;
    }
}
