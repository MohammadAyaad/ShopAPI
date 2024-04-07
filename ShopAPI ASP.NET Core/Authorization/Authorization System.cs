using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using ShopAPI.Authorization.Permissions_Roles;
using ShopAPI.Constants;
using static ShopAPI.Constants.ShopAPIDatabaseConstants;
using System.Net.Mail;
using System.Net;
using ShopAPI.Data;
using ShopAPI_ASP.NET_Core.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopAPI.Model.Users;
namespace ShopAPI.Authorization
{


    public static class AuthorizationConstants
    {
        public const string Bearer = "Bearer";
    }
    public class AccountManager
    {
        private static (string token_id, string token) ParseToken(string token)
        {
            string[] s = token.Split(':');
            if (s.Length < 2)
            {
                throw new Exception("Invalid Token");
            }
            else
            {
                return (s[0], s[1]);
            }
        }
        private static string MakeToken(string token_id, string token)
        {
            return token_id + ":" + token;
        }
        public static string GetEmail(ShopDBContext context, string token)
        {
            (string token_id, string _token) = ParseToken(token);

            return context.JwtAccessTables.Where(t => t.Id.ToString() == token_id).First().Email;
        }
        public static (bool valid, string secretKey, string encryptionKey) ValidateTokenPermissionsAndGetInfo(ShopDBContext context, string token, Permissions requiredPermissions)
        {
            (string token_id, string _token) = ParseToken(token);
            var data = context.JwtAccessTables.Where(t => t.Id.ToString() == token_id).First();            
            string secretKey = data.SecretKey;
            string encryptionKey = data.EncryptionKey;
            uint token_permissions = Convert.ToUInt32(JwtTokenManager.ExtractClaims(_token, encryptionKey, (c) => { return c.Type == "permissions"; }).First().Value);
            if ((token_permissions & (uint)requiredPermissions) == (uint)requiredPermissions)
            {
                bool valid = JwtTokenManager.ValidateToken(_token, secretKey, encryptionKey);
                if (valid)
                {
                    return (valid, secretKey, encryptionKey);
                }
                else
                {
                    return (false, null, null); //just for security to avoid mistakenly leaking the secret keys when the token is invalid anyway
                }
            }
            else
            {
                return (false, null, null);
            }
        }
        
        
        private static bool CheckAccountExists(ShopDBContext context, string email)
        {
                //ignored the case where there could be more than 1 row holding the same email because it must not be possible
                return context.UserAccounts.Where(u => u.Email == email).Count() == 1;
        }
        public static (string token, string error) Login(ShopDBContext context, string email, string passhash, Permissions required_permissions)
        {
            if (CheckAccountExists(context, email))
            {
                try
                {
                    var accounts = context.UserAccounts.Where(a => a.Email == email);
                    if (accounts.Count() < 1) ;//NOT FOUND
                    JsonArray result_email_perms = JsonNode.Parse(Queries.ExecuteQuery_JSON(connection, $"SELECT {TN_USERS_CL_EMAIL},{TN_USERS_CL_ROLE} FROM `{TN_USERS}` WHERE {TN_USERS_CL_EMAIL}=\"{email}\" AND {TN_USERS_CL_PASSHASH}=\"{passhash}\";")).AsArray();
                    bool successful_login = result_email_perms.Count == 1;
                    if (successful_login)
                    {
                        if (((uint)required_permissions & Convert.ToUInt32(result_email_perms[0][TN_USERS_CL_ROLE].ToString())) == (uint)required_permissions)
                        {
                            (string token_id, string token, string secretKey, string encryptionKey) =
                            JwtTokenManager.GenerateToken(
                                new List<Claim>()
                                {
                                    new Claim("email",email),
                                    new Claim("permissions",((uint)required_permissions).ToString())
                                },
                                "elmazien-api",
                                "elmazien-api",
                                new TimeSpan(7, 0, 0, 0),
                                new TimeSpan(0));

                            Queries.ExecuteQuery_JSON(connection, $"DELETE FROM `{TN_LOGIN_USERS_JWT}` WHERE {TN_LOGIN_USERS_JWT_CL_TOKEN_EMAIL}='{email}';");
                            Queries.ExecuteQuery_JSON(connection, $"INSERT INTO {TN_LOGIN_USERS_JWT}({TN_LOGIN_USERS_JWT_CL_TOKEN_ID},{TN_LOGIN_USERS_JWT_CL_TOKEN_EMAIL},{TN_LOGIN_USERS_JWT_CL_TOKEN_SECRET_KEY},{TN_LOGIN_USERS_JWT_CL_TOKEN_ENCRYPTION_KEY}) VALUES('{token_id}','{email}','{secretKey}','{encryptionKey}');");

                            return (MakeToken(token_id, token), null);

                        }
                        else
                        {
                            return (null, "Failed to log in : No Permissions!");
                        }
                    }
                    else
                    {
                        return (null, "Failed to log in : Incorrect password!");
                    }
                }
                catch (Exception e)
                {
                    return (null, "Failed to log in : An error occurred!");
                }
            }
            else
            {
                return (null, "Failed to log in : Email was not found!");
            }
        }
        public static void Logout(ShopDBContext context, string token)
        {
            (string token_id, _) = ParseToken(token);
            if (ValidateTokenPermissionsAndGetInfo(connection, token, 0).valid)
            {
                Queries.ExecuteQuery_JSON(connection, $"DELETE FROM `{TN_LOGIN_USERS_JWT}` WHERE {TN_LOGIN_USERS_JWT_CL_TOKEN_ID}='{token_id}'");
            }
        }
        public static bool DeleteAccount(ShopDBContext context, string token)
        {
            (string token_id, string _token) = ParseToken(token);
            (bool valid, string secretKey, string encryptionKey) = ValidateTokenPermissionsAndGetInfo(connection, token, Permissions.DELETE_ACCOUNT);
            if (valid)
            {
                string email = JwtTokenManager.ExtractClaims(_token, encryptionKey, (c) => { return c.Type == "email"; }).First().Value;
                Queries.ExecuteQuery_JSON(connection, $"DELETE FROM `{TN_USERS}` WHERE `{TN_USERS_CL_EMAIL}`='{email}';");
                return true;
            }
            return false;
        }
    }
    public static class JwtTokenManager
    {
        private static JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

        public static List<Claim> ExtractClaims(string token, string encryptionKey, Func<Claim, bool> claimIdentificationCondition)
        {
            string decryptedSignedToken = Decrypt(token, encryptionKey);
            JwtSecurityToken jwttoken = handler.ReadJwtToken(decryptedSignedToken);
            return jwttoken.Claims.Where(claimIdentificationCondition).ToList();
        }
        public static bool ValidateToken(string token, string secretKey, string encryptionKey)
        {
            string decryptedSignedToken = Decrypt(token, encryptionKey);

            try
            {
                JwtSecurityToken jwttoken = handler.ReadJwtToken(decryptedSignedToken);
                SymmetricSecurityKey _secretKey = new SymmetricSecurityKey(Convert.FromBase64String(secretKey));
                var __validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _secretKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };

                handler.ValidateToken(decryptedSignedToken, __validationParameters, out SecurityToken t);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static (string token_id, string token, string secretKey, string encryptionKey) GenerateToken(List<Claim> claims, string issuer, string audience, TimeSpan TokenLifeSpan, TimeSpan TokenValidAfter)
        {
            string token_id = Guid.NewGuid().ToString();

            List<Claim> _claims = new List<Claim>(claims);
            _claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()));
            //_claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, DateTimeOffset.Now.Add(TokenValidAfter).ToUnixTimeMilliseconds().ToString()));
            _claims.Add(new Claim(JwtRegisteredClaimNames.Jti, token_id));

            SymmetricSecurityKey secretKey = new SymmetricSecurityKey(generateKey());

            SigningCredentials scredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);


            JwtSecurityToken token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: _claims,
            expires: DateTimeOffset.Now.Add(TokenValidAfter).Add(TokenLifeSpan).DateTime,
            signingCredentials: scredentials);

            string signedToken = handler.WriteToken(token);

            (string encryptedToken, string encryptionKey) = Encrypt(signedToken);
            string base64secretKey = Convert.ToBase64String(secretKey.Key);


            return (token_id, encryptedToken, base64secretKey, encryptionKey);
        }

        private static (string encryptedString, string encryptionKey) Encrypt(string plainText)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.GenerateKey();
                aes.GenerateIV();

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        var encryptedBytes = msEncrypt.ToArray();
                        var encryptedString = Convert.ToBase64String(encryptedBytes);
                        var secretKey = Convert.ToBase64String(aes.Key);
                        var iv = Convert.ToBase64String(aes.IV);

                        string secret = secretKey + ":" + iv;

                        return (encryptedString, secret);
                    }
                }
            }
        }

        private static string Decrypt(string encryptedString, string encryptionKey)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedString);
            string[] keys = encryptionKey.Split(':');
            var key = Convert.FromBase64String(keys[0]);
            var iv = Convert.FromBase64String(keys[1]);

            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var msDecrypt = new MemoryStream(encryptedBytes))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            var decryptedText = srDecrypt.ReadToEnd();
                            return decryptedText;
                        }
                    }
                }
            }
        }
        private static byte[] generateKey()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] secretKey = new byte[32]; // 256 bits
                rng.GetBytes(secretKey);

                var secretKeyBase64 = Convert.ToBase64String(secretKey);
                Console.WriteLine($"Secret Key: {secretKeyBase64}");

                return secretKey;
            }
        }
    }
    public static class JwtTokenCore
    {
        private static JwtSecurityTokenHandler _handler = new JwtSecurityTokenHandler();
        public static JwtSecurityToken GetToken(string token)
        {
            return new JwtSecurityToken(token);
        }
        public static bool TokenValid(string token, SymmetricSecurityKey secretKey)
        {
            var __validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = secretKey,
                ValidateIssuer = true,
                ValidateAudience = true
            };
            try
            {
                _handler.ValidateToken(token, __validationParameters, out SecurityToken t);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
