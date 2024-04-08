using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using ShopAPI.Data;
using JsonTokens.ComponentBasedTokens.ComponentSet;
using ShopAPI.Model.Moderation;
using ShopAPI.Model.TokenComponents;
using JsonTokens.Components;
using JsonTokens.ProcessingLayers;
using Microsoft.EntityFrameworkCore;
using ShopAPI.Model.TokenId;
using System.Net;

namespace ShopAPI.Authorization
{
    public static class AuthorizationService
    {
        public static readonly JsonTokenProcessor jsonTokenProcessor;
        public static TokenId tokenIdProcessorLayer;

        static AuthorizationService()
        {
            tokenIdProcessorLayer = new TokenId();
            Stack<ITokenProcessorLayer> layers = new Stack<ITokenProcessorLayer>();
            //layers.Push(new HS256TokenProtectionLayer());
            layers.Push(tokenIdProcessorLayer);
            jsonTokenProcessor = new JsonTokenProcessor(layers);
        }

        public static ActionResult<(JCST token, string email,AccessToken accessToken)> AuthorizeAccess(string AuthorizationHeader,ShopDBContext context,Permissions RequiredPermissions = Permissions.RESOURCE_OPEN)
        {
            var result = getTokenFromAuthorization(AuthorizationHeader, jsonTokenProcessor, tokenIdProcessorLayer,context);

            (JCST userToken, string email) = result.Value;

            if (userToken == null) return result.Result;

            if (!UserAccountExists(email, context)) return new NotFoundResult();

            AccessToken access = userToken.GetComponent<AccessToken>();

            if (access.Email != email) return new UnauthorizedResult();

            if (!((access.Permissions & RequiredPermissions) == RequiredPermissions)) return new UnauthorizedResult();

            return (userToken, email, access);
        }

        private static ActionResult<(JCST token, string email)> getTokenFromAuthorization(string authorization,JsonTokenProcessor jsonTokenProcessor,TokenId tokenIdPL,ShopDBContext _context)
        {
            if (string.IsNullOrEmpty(authorization)) return new UnauthorizedResult();

            string[] authParts = authorization.Split(" ");

            if (authParts.Length != 2) return new UnauthorizedResult();

            string bearer = authParts[0];
            string token = authParts[1];
            if (bearer != "Bearer") return new UnauthorizedResult();

            ((ITokenProcessorLayer)tokenIdPL).FromString(token, "");

            string email = ((IIdentifiableTokenProcessorLayer<string>)tokenIdPL).GetId();

            var secret = _context.JwtAccessTables.Where(t => t.Email == email).First();

            JCST userToken = jsonTokenProcessor.FromString(token, secret.Secret);

            LifeTime lt = userToken.GetComponent<LifeTime>();

            if (!lt.IsValid())
            {
                _context.JwtAccessTables.Remove(secret);
                return new UnauthorizedResult();
            }

            return (userToken, email);
        }
        private static bool UserAccountExists(string email, ShopDBContext _context)
        {
            return _context.UserAccounts.Any(e => e.Email == email);
        }
    }

    
}
