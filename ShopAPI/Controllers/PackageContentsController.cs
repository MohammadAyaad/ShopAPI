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
    [Route("api/package")]
    [ApiController]
    public class PackageContentsController : ControllerBase
    {
        private readonly ShopDBContext _context;

        public PackageContentsController(ShopDBContext context)
        {
            _context = context;
        }

        // GET: api/PackageContents/5
        [HttpGet("{packageId}/content")]
        public async Task<ActionResult<IEnumerable<Product>>> GetPackageContent([FromHeader(Name = "Authorization")] string authorization,int packageId)
        {
            var result = AuthorizationService.AuthorizeAccess(authorization, _context, READ_PACKAGES);

            (JCST userToken, string email, AccessToken access) = result.Value;

            if (userToken == null) return result.Result;

            var packageContents =  _context.PackageContent.Where(c => c.PackageId == packageId);

            var packageProducts = _context.Products.Where(p => packageContents.Any(c => c.ProductId == p.Id)).ToList();

            if (packageProducts.Count == 0)
            {
                return NotFound();
            }

            return packageProducts;
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{packageId}/content")]
        public async Task<ActionResult<PackageContent>> PostPackageContent([FromHeader(Name = "Authorization")] string authorization, PackageContent packageContent)
        {
            var result = AuthorizationService.AuthorizeAccess(authorization, _context, READ_PACKAGES | EDIT_PACKAGES);

            (JCST userToken, string email, AccessToken access) = result.Value;

            if (userToken == null) return result.Result;

            _context.PackageContent.Add(packageContent);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPackageContent", new { id = packageContent.Id }, packageContent);
        }

        // DELETE: api/PackageContents/5
        [HttpDelete("{packageId}/content/{productId}")]
        public async Task<IActionResult> DeletePackageContent([FromHeader(Name = "Authorization")] string authorization,int packageId, int productId)
        {
            var result = AuthorizationService.AuthorizeAccess(authorization, _context, READ_PACKAGES | EDIT_PACKAGES);

            (JCST userToken, string email, AccessToken access) = result.Value;

            if (userToken == null) return result.Result;

            var packageContent = _context.PackageContent.Where(c => c.PackageId == packageId && c.ProductId == productId).FirstOrDefault();
            if (packageContent == null)
            {
                return NotFound();
            }

            _context.PackageContent.Remove(packageContent);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PackageContentExists(int id)
        {
            return _context.PackageContent.Any(e => e.Id == id);
        }
    }
}
