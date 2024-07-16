using JsonTokens.ProcessingLayers;
using JsonTokens.Exceptions;
using System.Text;
using Microsoft.AspNetCore.DataProtection;


namespace ShopAPI.Model.TokenId
{
    public class TokenId : IIdentifiableTokenProcessorLayer<string>
    {
        private string email;
        (string token, string secret) ITokenProcessorLayer.FromString(string token, string secret)
        {
            (string _token,this.email) = ParseToken(token);
            return (_token, secret);
        }

        (string token, string secret) ITokenProcessorLayer.ToString(string input, string secret)
        {
            return (MakeToken(input,this.email),secret);
        }
        string IIdentifiableTokenProcessorLayer<string>.GetId()
        {
            return this.email;
        }
        public void setId(string email)
        {
            this.email = email;
        }


        private (string token, string email) ParseToken(string token)
        {
            string[] tokenParts = token.Split(":");
            if (tokenParts.Length != 2) throw new InvalidTokenException();
            string email = Encoding.UTF8.GetString(Convert.FromBase64String(tokenParts[0]));
            string _token = tokenParts[1];
            return (_token, email);
        }
        private string MakeToken(string token, string email)
        {
            return $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(email))}:{token}";
        }
    }

}
