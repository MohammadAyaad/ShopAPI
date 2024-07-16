using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonTokens.ComponentBasedTokens.ComponentSet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopAPI.Authorization;
using ShopAPI.Data;
using ShopAPI.Model.Packages;
using ShopAPI.Model.Products;
using ShopAPI.Authorization;
using static ShopAPI.Model.Moderation.Permissions;
using static ShopAPI.Model.Moderation.UserRoles;
using System.Net;
using JsonTokens.ComponentBasedTokens.ComponentSet;
using ShopAPI.Model.Tokens.TokenComponents;

namespace ShopAPI.Controllers
{

    //variants adding under here
    [Route("api/packages")]
    [ApiController]
    public class PackagesController : ControllerBase
    {
        private readonly ShopDBContext _context;

        public PackagesController(ShopDBContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Package>>> GetPackages([FromHeader(Name = "Authorization")] string authorization, [FromQuery(Name = "index")] uint index = 0, [FromQuery(Name = "count")] uint count = 20)
        {
            var result = AuthorizationService.AuthorizeAccess(authorization, _context, READ_PACKAGES);

            (JCST userToken, string email, AccessToken access) = result.Value;

            if (userToken == null) return result.Result;

            if (count > 50) return BadRequest();
            return _context.Packages.Skip((int)index).Take((int)count).ToList();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Package>> GetPackage([FromHeader(Name = "Authorization")] string authorization, int id, [FromQuery(Name = "rating_index")] uint rating_index = 0, [FromQuery(Name = "rating_count")] uint ratings_count = 20)
        {
            var result = AuthorizationService.AuthorizeAccess(authorization, _context, READ_PACKAGES);

            (JCST userToken, string email, AccessToken access) = result.Value;

            if (userToken == null) return result.Result;

            var package = await _context.Packages.FindAsync(id);

            if (package == null)
            {
                return NotFound();
            }

            package.PackageContent = _context.PackageContent.Where(p => p.PackageId == package.Id).ToList();

            package.PackageRatings = _context.PackageRatings.Where(r => r.PackageId == package.Id).Skip((int)rating_index).Take((int)ratings_count).ToList();

            return package;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPackage([FromHeader(Name = "Authorization")] string authorization, int id, Package package)
        {
            var result = AuthorizationService.AuthorizeAccess(authorization, _context, EDIT_PRODUCTS);

            (JCST userToken, string email, AccessToken access) = result.Value;

            if (userToken == null) return result.Result;

            if (id != package.Id)
            {
                return BadRequest();
            }

            _context.Entry(package).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PackageExists(id))
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

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Package>> PostPackage([FromHeader(Name = "Authorization")] string authorization, Package package)
        {
            var result = AuthorizationService.AuthorizeAccess(authorization, _context, CREATE_PACKAGES);

            (JCST userToken, string email, AccessToken access) = result.Value;

            if (userToken == null) return result.Result;

            _context.Packages.Add(package);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = package.Id }, package);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackage([FromHeader(Name = "Authorization")] string authorization, int id)
        {
            var result = AuthorizationService.AuthorizeAccess(authorization, _context, DELETE_PACKAGES);

            (JCST userToken, string email, AccessToken access) = result.Value;

            if (userToken == null) return result.Result;

            var package = await _context.Packages.FindAsync(id);
            if (package == null)
            {
                return NotFound();
            }

            _context.Packages.Remove(package);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PackageExists(int id)
        {
            return _context.Packages.Any(e => e.Id == id);
        }
    }
}
