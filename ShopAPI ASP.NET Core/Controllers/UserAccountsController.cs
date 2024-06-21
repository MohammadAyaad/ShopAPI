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
using ShopAPI.Model.Users;
using JsonTokens.ComponentBasedTokens.ComponentSet;
using JsonTokens.ProcessingLayers;
using JsonTokens.Components;
using ShopAPI.Model.TokenComponents;
using Newtonsoft.Json.Linq;
using System.Text;
using ShopAPI.Model.TokenId;
using ShopAPI.Authorization;
using ShopAPI.Model.Moderation;
using System.Net;
using System.Security.Cryptography;


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
        // POST: api/UserAccounts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        public async Task<ActionResult<UserAccountDTO>> CreateUserAccount(UserAccountDTO userAccountDTO, string pw)
        {
            new MailAddress(userAccountDTO.Email); //check if its a valid email address

            if (UserAccountExists(userAccountDTO.Email))
            {
                return BadRequest("Account already exists");
            }
            else
            {
                UserAccount account = userAccountDTO.ToNewUserAccount();
                account.SetRole(UserRoles.PERMISSIONS_DefaultUserAccount);
                account.Password = pw;
                _context.UserAccounts.Add(account);
                _context.SaveChanges();
                return Created();
            }
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

            if (account.Password != password) return Unauthorized();

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

            _context.JwtAccessTables.Add(new Model.Users.JwtAccessTable()
            {
                Email = email,
                Secret = secret,
                ExpiresAt = tokenLifeTime.ExpiresAt
            });

            _context.SaveChanges();

            JObject r = new JObject();
            r.Add("token", $"Bearer {token}");
            return Ok(r.ToString());
        }

        [HttpGet]
        public async Task<ActionResult<UserAccountDTO>> GetUserAccount([FromHeader(Name = "Authorization")] string authorization)
        {
            var result = getTokenFromAuthorization(authorization);

            (JCST userToken, string email) = result.Value;

            if (userToken == null) return result.Result;

            if (!UserAccountExists(email)) return NotFound();

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

        [HttpPut]
        public async Task<ActionResult> PutUserAccount([FromHeader(Name = "Authorization")] string authorization, UserAccountDTO userAccountDTO)
        {
            var result = getTokenFromAuthorization(authorization);

            (JCST userToken, string email) = result.Value;

            if (userToken == null) return result.Result;

            if (!UserAccountExists(email)) return NotFound();

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
                if (!UserAccountExists(email))
                {
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

            if (!UserAccountExists(email)) return NotFound();

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

            LifeTime lt = userToken.GetComponent<LifeTime>();

            if (!lt.IsValid())
            {
                _context.JwtAccessTables.Remove(secret);
                return Unauthorized();
            }

            return (userToken, email);
        }
        private bool UserAccountExists(string email)
        {
            return _context.UserAccounts.Any(e => e.Email == email);
        }
    }
}
