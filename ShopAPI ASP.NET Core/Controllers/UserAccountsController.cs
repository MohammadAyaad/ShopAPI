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
        public async Task<ActionResult<UserAccount>> CreateUserAccount(UserAccount userAccount)
        {
            new MailAddress(userAccount.Email); //check if its a valid email address

            if (UserAccountExists(userAccount.Email))
            {
                return BadRequest("Account already exists");
            }
            else
            {
                UserAccount account = new UserAccount(userAccount);
                account.SetRole(UserRoles.PERMISSIONS_DefaultUserAccount);
                _context.UserAccounts.Add(account);
                _context.SaveChanges();
                return Created();
            }
        }
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromHeader(Name = "WWW-Authenticate")] string basicAuth)
        {
            string[] basic_parts = basicAuth.Split(" ");

            if (basic_parts.Length != 2) return Unauthorized();

            if (basic_parts[0].Trim().ToLower() != "basic") return Unauthorized();

            string[] auth_parts = basic_parts[1].Trim().Split(":");
            
            if(auth_parts.Length != 2) return Unauthorized();

            string email = auth_parts[0].Trim();
            string password = auth_parts[1].Trim();

            new MailAddress(email);

            var account = _context.UserAccounts.FirstOrDefault(u => u.Email == email);

            if (account == null) return NotFound();

            if (account.Password != password) return Unauthorized();

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

            (string token,string secret) = jsonTokenProcessor.ToString(jcst);

            _context.JwtAccessTables.Add(new Model.Users.JwtAccessTable()
            {
                Email = email,
                Secret = secret,
                ExpiresAt = tokenLifeTime.ExpiresAt
            });

            _context.SaveChanges();

            JObject r = new JObject();
            r.Add("token", token);
            return Ok(r.ToString());
        }

        [HttpGet()]
        public async Task<ActionResult<UserAccount>> GetUserAccount([FromHeader(Name = "Authorization")] string authorization)
        {
            var result = getTokenFromAuthorization(authorization);

            (JCST userToken,string email) = result.Value;

            if (userToken == null) return result.Result;

            if (!UserAccountExists(email)) return NotFound();

            AccessToken access = userToken.GetComponent<AccessToken>();

            if(access.Email != email) return Unauthorized();

            if (!access.Permissions.HasFlag(Permissions.READ_ACCOUNT)) return Unauthorized();

            var userAccount = _context.UserAccounts.Where(u => u.Email == email).FirstOrDefault();

            if (userAccount == null)
            {
                return NotFound();
            }

            return userAccount;
        }

        [HttpPut()]
        public async Task<IActionResult> PutUserAccount([FromHeader(Name = "Authorization")] string authorization, UserAccount userAccount)
        {
            var result = getTokenFromAuthorization(authorization);

            (JCST userToken, string email) = result.Value;

            if (userToken == null) return result.Result;

            if (!UserAccountExists(email)) return NotFound();

            AccessToken access = userToken.GetComponent<AccessToken>();

            if (!access.Permissions.HasFlag(Permissions.MANAGE_ACCOUNT)) return StatusCode(StatusCodes.Status403Forbidden);

            var account = _context.UserAccounts.FirstOrDefault(e => e.Email == email);

            if (account == null) return NotFound();

            if (userAccount.GetRole() != default && access.Permissions.HasFlag(Permissions.EDIT_ROLES)) account.SetRole(userAccount.GetRole());

            if (userAccount.UserName != default && account.UserName != userAccount.UserName) account.UserName = userAccount.UserName;
            if (userAccount.FirstName != default && account.FirstName != userAccount.FirstName) account.FirstName = userAccount.FirstName;
            if (userAccount.Email != default && account.Email != userAccount.Email) account.Email = userAccount.Email;
            if (userAccount.Password != default && account.Password != userAccount.Password) account.Password = userAccount.Password;
            if (userAccount.DateOfBirth != default && account.DateOfBirth != userAccount.DateOfBirth) account.DateOfBirth = userAccount.DateOfBirth;

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

        [HttpDelete()]
        public async Task<IActionResult> DeleteUserAccount([FromHeader(Name = "Authorization")] string authorization)
        {
            var result = getTokenFromAuthorization(authorization);

            (JCST userToken,string email) = result.Value;

            if (userToken == null) return result.Result;

            if (!UserAccountExists(email)) return NotFound();

            var userAccount = _context.UserAccounts.FirstOrDefault(u => u.Email == email);
            if (userAccount == null)
            {
                return NotFound();
            }

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

            if (!lt.IsValid)
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
