using Microsoft.AspNetCore.Mvc;
using ShopAPI.Authorization;
using ShopAPI.Data;
using ShopAPI.Model.TokenComponents;
using static ShopAPI.Model.Moderation.Permissions;

namespace ShopAPI.Controllers;

[Route("api/package")]
[ApiController]
public class PackageRatingsController : ControllerBase
{
    private readonly ShopDBContext _context;

    public PackageRatingsController(ShopDBContext context) => _context = context;

    // GET: api/ProductRatings/5
    [HttpGet("{packageId}/ratings")]
    public async Task<ActionResult<IEnumerable<PackageRating>>> GetPackageRatings(
        [FromHeader(Name = "Authorization")] string authorization,
        int packageId)
    {
        var result = AuthorizationService.AuthorizeAccess(authorization, _context, READ_PACKAGES);

        (JCST userToken, string email, AccessToken access) = result.Value;

        if (userToken == null) return result.Result;

        if (_context.PackageRatings.Where(r => r.PackageId == packageId).ToList() == null)
        {
            return NotFound();
        }

        return _context.PackageRatings.Where(r => r.PackageId == packageId).ToList();
    }

    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost("{packageId}/rate")]
    public async Task<ActionResult<PackageRating>> PostPackageRating(
        [FromHeader(Name = "Authorization")] string authorization,
        int packageId,
        PackageRating packageRating)
    {
        var result = AuthorizationService.AuthorizeAccess(authorization, _context, READ_PACKAGES | REVIEW_PACKAGES);

        (JCST userToken, string email, AccessToken access) = result.Value;

        if (userToken == null) return result.Result;

        if (packageId != packageRating.PackageId) return Conflict();

        _context.PackageRatings.Add(packageRating);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPackageRating", new { id = packageRating.Id }, packageRating);
    }

    // DELETE: api/ProductRatings/5
    [HttpDelete("{packageId}/rating")]
    public async Task<IActionResult> DeletePackageRating(
        [FromHeader(Name = "Authorization")] string authorization,
        int packageId)
    {
        var result = AuthorizationService.AuthorizeAccess(authorization, _context, READ_PACKAGES | REVIEW_PACKAGES);

        (JCST userToken, string email, AccessToken access) = result.Value;

        if (userToken == null) return result.Result;

        var packageRating = _context.PackageRatings.Where(r => r.PackageId == packageId && r.RaterEmail == email).First();
        if (packageRating == null)
        {
            return NotFound();
        }

        _context.PackageRatings.Remove(packageRating);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PackageRatingExists(Guid id)
    {
        return _context.PackageRatings.Any(e => e.Id == id);
    }
}
