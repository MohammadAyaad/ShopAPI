using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.IdentityModel.JsonWebTokens;
using ShopAPI.Data;
using JsonTokens.ComponentBasedTokens.ComponentSet;
using JsonTokens.ProcessingLayers;
using JsonTokens.Components;
using Newtonsoft.Json.Linq;
using System.Text;
using ShopAPI.Model.TokenId;
using ShopAPI.Authorization;
using ShopAPI.Model.Moderation;
using System.Net;
using System.Security.Cryptography;
using ShopAPI.Model.Authentication;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using ShopAPI.Model.Tokens.TokenComponents;
using ShopAPI.Model.Tokens;
using UtilitiesX;
using NuGet.Protocol;
using UtilitiesX.Extentions;


namespace ShopAPI.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserAccountsController : ControllerBase
    {
        private readonly ShopDBContext _context;
        private readonly JsonTokenProcessor jsonTokenProcessor;

        private TokenId tokenIdPL;

        public UserAccountsController(ShopDBContext context)
        {
            _context = context;
            tokenIdPL = new TokenId();
            Stack<ITokenProcessorLayer> layers = new Stack<ITokenProcessorLayer>();
            //layers.Push(new HS256TokenProtectionLayer());
            layers.Push(tokenIdPL);
            jsonTokenProcessor = new JsonTokenProcessor(layers);
        }
        // GET /create -- to get the account creation process token
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpGet("create")]
        public async Task<ActionResult<Token>> CreateUserAccount([FromQuery(Name = "email")] string email, [FromQuery(Name = "username")] string username, [FromQuery(Name = "firstname")] string firstname, [FromQuery(Name = "lastname")] string lastname, [FromQuery(Name = "birthday")] DateOnly birthday)
        {
            UserAccountDTO userAccountDTO = new UserAccountDTO(
                email,
                username,
                firstname,
                lastname,
                DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                birthday
                );

            (bool exists, ExistenceInfo info) =
            UserAccountExists(userAccountDTO.Email, userAccountDTO.UserName);

            if (exists)
            {
                string which = "";
                which += (info.HasFlag(ExistenceInfo.Email) ? "Email " : "");
                which += (info.HasFlag(ExistenceInfo.Username) ? (info.HasFlag(ExistenceInfo.Email) ? "And Username" : "Username") : "Error");
                return BadRequest($"{which} already exists");
            }

            JCST accountCreationActionToken = new JCST(RandomNumberGenerator.GetBytes(32));

            LifeTime tokenLifeTime = new LifeTime(DateTimeOffset.Now.ToUnixTimeMilliseconds(), DateTimeOffset.Now.AddHours(1).ToUnixTimeMilliseconds());

            AccountCreationDescriptor acd = new AccountCreationDescriptor(userAccountDTO);

            accountCreationActionToken.AddComponent(acd);
            accountCreationActionToken.AddComponent(tokenLifeTime);


            tokenIdPL.setId(userAccountDTO.Email);
            (string token, string secret) = jsonTokenProcessor.ToString(accountCreationActionToken);

            _context.JwtAccessTables.Add(new JwtAccessTokenDescriptor(userAccountDTO.Email, secret, tokenLifeTime.ExpiresAt));

            _context.SaveChanges();
            JObject r = new JObject();
            r.Add("token", JObject.FromObject(new Token(token, tokenLifeTime)));
            r.Add("auth_options", JArray.FromObject(AuthenticationService.GetAuthenticationMethodsList().Collect<List<string>, (string id, string name)>(new List<string>(), (i, l) => { l.Add(i.id); return l; })));
            return Ok(r.ToJson());
        }

        [HttpPost("create/authentication")] //create an authentication
        public async Task<ActionResult> CreateUserAccount([FromHeader(Name = "Authorization")] string Authorization, AuthenticationDescriptor authenticationDescription, string auth, string init = "")
        {
            var result = getTokenFromAuthorization(Authorization);

            (JCST userToken, string email) = result.Value;

            if (userToken == null) return result.Result;

            if (!userToken.ContainsComponent<AccountCreationDescriptor>()) return BadRequest();

            if (!AuthenticationService.ContainsAuthentications(authenticationDescription.AuthType)) return BadRequest();

            var method = AuthenticationService.GetAuthenticationFactory(authenticationDescription.AuthType).Create(auth);

            (bool skip, string initiationResult) = method.InitiateAuthentication(init);

            AuthenticationTracker authTracker = new AuthenticationTracker(initiationResult);

            LifeTime tokenLifeTime = new LifeTime(DateTimeOffset.Now.ToUnixTimeMilliseconds(), DateTimeOffset.Now.AddMinutes(5).ToUnixTimeMilliseconds());

            AccountCreationDescriptor accountcd = userToken.GetComponent<AccountCreationDescriptor>();

            AuthenticationCreationDescriptor authcd = new AuthenticationCreationDescriptor(authenticationDescription,method,authTracker);

            UserAccountDTO userAccountDTO = accountcd.User;

            JCST accountCreationAuthenticationToken = new JCST(RandomNumberGenerator.GetBytes(32));

            accountCreationAuthenticationToken.AddComponent(accountcd);
            accountCreationAuthenticationToken.AddComponent(authcd);
            accountCreationAuthenticationToken.AddComponent(tokenLifeTime);

            tokenIdPL.setId(userAccountDTO.Email);
            (string token, string secret) = jsonTokenProcessor.ToString(accountCreationAuthenticationToken);

            _context.JwtAccessTables.Add(new JwtAccessTokenDescriptor(userAccountDTO.Email, secret, tokenLifeTime.ExpiresAt));

            _context.SaveChanges();
            JObject r = new JObject();
            r.Add("token", JObject.FromObject(new Token(token, tokenLifeTime)));
            return Ok(r.ToJson());
        }
        [HttpPost("create")] //create an authentication
        public async Task<ActionResult> CreateUserAccount([FromHeader(Name = "Authorization")] string Authorization, string authToken)
        {
            var result = getTokenFromAuthorization(Authorization);

            (JCST userToken, string email) = result.Value;

            if (userToken == null) return result.Result;

            if (!userToken.ContainsComponent<AccountCreationDescriptor>()) return BadRequest(); 
            if (!userToken.ContainsComponent<AuthenticationCreationDescriptor>()) return BadRequest();

            AccountCreationDescriptor accountcd = userToken.GetComponent<AccountCreationDescriptor>();

            AuthenticationCreationDescriptor authcd = userToken.GetComponent<AuthenticationCreationDescriptor>();

            if (!authcd.AuthenticationMethod.Authenticate(authToken, authcd.AuthenticationTracker.InitiationResult)) return Unauthorized();

            UserAccount account = accountcd.User.ToNewUserAccount();

            Authentication authentication = Authentication.GenerateAuthentication(authcd.AuthenticationMethod, authcd.Descriptor, account);

            _context.UserAccounts.Add(account);

            _context.UserAuthentications.Add(authentication);

            _context.SaveChanges();

            return Created();
        }

        [HttpPost("login")]
        public async Task<ActionResult<JObject>> Login([FromHeader(Name = "WWW-Authenticate")] string basicAuth)
        {
            string[] basic_parts = basicAuth.Split(" ");

            if (basic_parts.Length != 2) return Unauthorized();

            if (basic_parts[0].Trim().ToLower() != "basic") return Unauthorized();

            string[] auth_parts = basic_parts[1].Trim().Split(":");

            if (auth_parts.Length != 2) return Unauthorized();

            string email = auth_parts[0].Trim();
            string password = auth_parts[1].Trim();

            new MailAddress(email);

            var account = _context.UserAccounts.FirstOrDefault(u => u.Email == email);

            if (account == null) return NotFound();

            //if (account.Password != password) return Unauthorized();

            var access_token = _context.JwtAccessTables.Where(t => t.Email == email);

            if (access_token.Count() != 0)
                foreach (var a in access_token)
                    _context.JwtAccessTables.Remove(a);

            this.tokenIdPL.setId(email);

            var jcst = new JCST(RandomNumberGenerator.GetBytes(32));

            AccessToken accessToken = new AccessToken()
            {
                Email = email,
                Permissions = account.GetRole()
            };

            jcst.AddComponent(accessToken);

            LifeTime tokenLifeTime = new LifeTime(DateTimeOffset.Now.ToUnixTimeMilliseconds(), DateTimeOffset.Now.AddDays(1).ToUnixTimeMilliseconds());

            jcst.AddComponent(tokenLifeTime);

            (string token, string secret) = jsonTokenProcessor.ToString(jcst);

            _context.JwtAccessTables.Add(new JwtAccessTokenDescriptor(email,secret,tokenLifeTime.ExpiresAt));

            _context.SaveChanges();

            JObject r = new JObject();
            r.Add("token", $"Bearer {token}");
            return Ok(r.ToString());
        }

        [HttpGet()]
        public async Task<ActionResult<UserAccountDTO>> GetUserAccount([FromHeader(Name = "Authorization")] string authorization)
        {
            var result = getTokenFromAuthorization(authorization);

            (JCST userToken, string email) = result.Value;

            if (userToken == null) return result.Result;

            throw new NotImplementedException();
            //if (!UserAccountExists(email)) return NotFound();

            AccessToken access = userToken.GetComponent<AccessToken>();

            if (access.Email != email) return Unauthorized();

            if (!access.Permissions.HasFlag(Permissions.READ_ACCOUNT)) return Unauthorized();

            var userAccount = _context.UserAccounts.Where(u => u.Email == email).FirstOrDefault();

            if (userAccount == null)
            {
                return NotFound();
            }

            return Ok(userAccount.GetDTO());
        }

        [HttpPut()]
        public async Task<ActionResult> PutUserAccount([FromHeader(Name = "Authorization")] string authorization, UserAccountDTO userAccountDTO)
        {
            var result = getTokenFromAuthorization(authorization);

            (JCST userToken, string email) = result.Value;

            if (userToken == null) return result.Result;

            throw new NotImplementedException();
            //if (!UserAccountExists(email)) return NotFound();

            AccessToken access = userToken.GetComponent<AccessToken>();

            if (!access.Permissions.HasFlag(Permissions.MANAGE_ACCOUNT)) return StatusCode(StatusCodes.Status403Forbidden);

            var account = _context.UserAccounts.FirstOrDefault(e => e.Email == email);

            if (account == null) return NotFound();

            if (account.Email != userAccountDTO.Email)
            {
                _context.JwtAccessTables.RemoveRange(_context.JwtAccessTables.Where(u => u.Email == email));
            }

            account.ModifyByDTO(userAccountDTO);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!false/*UserAccountExists(email)*/)
                {
                    throw new NotImplementedException();
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        [HttpPost("")]
        public async Task<ActionResult> LogoutUserAccount([FromHeader(Name = "Authorization")] string authorization)
        {
            var result = getTokenFromAuthorization(authorization);

            (JCST userToken, string email) = result.Value;

            if (userToken == null) return result.Result;

            _context.JwtAccessTables.RemoveRange(_context.JwtAccessTables.Where(u => u.Email == email));
            return Ok();
        }
        [HttpDelete()]
        public async Task<ActionResult> DeleteUserAccount([FromHeader(Name = "Authorization")] string authorization)
        {
            var result = getTokenFromAuthorization(authorization);

            (JCST userToken, string email) = result.Value;

            if (userToken == null) return result.Result;

            throw new NotImplementedException();
            //if (!UserAccountExists(email)) return NotFound();

            var userAccount = _context.UserAccounts.FirstOrDefault(u => u.Email == email);
            if (userAccount == null)
            {
                return NotFound();
            }

            var access_token = _context.JwtAccessTables.Where(t => t.Email == email);

            if (access_token.Count() != 0)
                foreach (var a in access_token)
                    _context.JwtAccessTables.Remove(a);

            _context.UserAccounts.Remove(userAccount);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        private ActionResult<(JCST token, string email)> getTokenFromAuthorization(string authorization)
        {
            if (string.IsNullOrEmpty(authorization)) return Unauthorized();

            string[] authParts = authorization.Split(" ");

            if (authParts.Length != 2) return Unauthorized();

            string bearer = authParts[0];
            string token = authParts[1];

            if (bearer != "Bearer") return Unauthorized();

            ((ITokenProcessorLayer)tokenIdPL).FromString(token, "");

            string email = ((IIdentifiableTokenProcessorLayer<string>)tokenIdPL).GetId();

            var secret = _context.JwtAccessTables.Where(t => t.Email == email).First();

            JCST userToken = jsonTokenProcessor.FromString(token, secret.Secret);

            if (!userToken.ContainsComponent<LifeTime>()) return Unauthorized();

            LifeTime lt = userToken.GetComponent<LifeTime>();

            if (!lt.IsValid())
            {
                _context.JwtAccessTables.Remove(secret);
                return Unauthorized();
            }

            return (userToken, email);
        }

        enum ExistenceInfo : uint
        {
            None = 0,
            Email = 1,
            Username = 2,
            EmailAndUsername = 3
        }
        private (bool exists, ExistenceInfo info) UserAccountExists(string email = null, string username = null)
        {
            if (email == null && username == null) return (false, ExistenceInfo.None);
            
            ExistenceInfo info = ExistenceInfo.None;

            if (email == null && username == null) return (false,ExistenceInfo.None);

            var existingUser = _context.UserAccounts.FirstOrDefault(u =>
                (email == null || u.Email == email) &&
                (username == null || u.UserName == username));

            if (existingUser != null)
            {
                if (email != null && existingUser.Email == email)
                    info |= ExistenceInfo.Email;

                if (username != null && existingUser.UserName == username)
                    info |= ExistenceInfo.Username;
            }

            return (info != ExistenceInfo.None,info);
        }

    }
}
