using Microsoft.AspNetCore.Mvc;
using ShopAPI.Data;
using ShopAPI.Model.Products;
using ShopAPI.Authorization;
using static ShopAPI.Model.Moderation.Permissions;
using ShopAPI.Model.TokenComponents;

namespace ShopAPI.Controllers;

[Route("api/products")]
[ApiController]
public class ProductRatingsController : ControllerBase
{
    private readonly ShopDBContext _context;

    public ProductRatingsController(ShopDBContext context)
    {
        _context = context;
    }

    // GET: api/ProductRatings/5
    [HttpGet("{id}/ratings")]
    public async Task<ActionResult<IEnumerable<ProductRating>>> GetProductRatings(
        [FromHeader(Name = "Authorization")] string authorization,
        int id)
    {
        var result = AuthorizationService.AuthorizeAccess(authorization, _context, READ_PRODUCTS);

        (JCST userToken, string email, AccessToken access) = result.Value;

        if (userToken == null) return result.Result;

        var productRating = _context.ProductRatings.Where(r => r.ProductId == id).ToList();

        if (productRating == null)
        {
            return NotFound();
        }

        return productRating;
    }

    // POST: api/ProductRatings
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost("{productId}/rate")]
    public async Task<ActionResult<ProductRating>> PostProductRating(
        [FromHeader(Name = "Authorization")] string authorization,
        int productId,
        ProductRating productRating)
    {
        var result = AuthorizationService.AuthorizeAccess(authorization, _context, READ_PRODUCTS | REVIEW_PRODUCTS);

        (JCST userToken, string email, AccessToken access) = result.Value;

        if (userToken == null) return result.Result;

        if (productRating.ProductId != productId) return BadRequest();

        _context.ProductRatings.Add(productRating);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetProductRating", new { id = productRating.Id }, productRating);
    }

    // DELETE: api/ProductRatings/5
    [HttpDelete("{productId}/rating")]
    public async Task<IActionResult> DeleteProductRating(
        [FromHeader(Name = "Authorization")] string authorization,
        int productId)
    {
        var result = AuthorizationService.AuthorizeAccess(authorization, _context, READ_PRODUCTS | REVIEW_PRODUCTS);

        (JCST userToken, string email, AccessToken access) = result.Value;

        if (userToken != null)
        {
            var productRating = _context.ProductRatings.Where(r => r.ProductId == productId && r.RaterEmail == email).First();
            if (productRating == null)
            {
                return NotFound();
            }

            _context.ProductRatings.Remove(productRating);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        return result.Result;
    }

    private bool ProductRatingExists(Guid id)
    {
        return _context.ProductRatings.Any(e => e.Id == id);
    }
}
