using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopAPI.Data;
using ShopAPI.Model.Products;

namespace ShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductRatingsController : ControllerBase
    {
        private readonly ShopDBContext _context;

        public ProductRatingsController(ShopDBContext context)
        {
            _context = context;
        }

        // GET: api/ProductRatings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductRating>>> GetProductRatings()
        {
            return await _context.ProductRatings.ToListAsync();
        }

        // GET: api/ProductRatings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductRating>> GetProductRating(Guid id)
        {
            var productRating = await _context.ProductRatings.FindAsync(id);

            if (productRating == null)
            {
                return NotFound();
            }

            return productRating;
        }

        // PUT: api/ProductRatings/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductRating(Guid id, ProductRating productRating)
        {
            if (id != productRating.Id)
            {
                return BadRequest();
            }

            _context.Entry(productRating).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductRatingExists(id))
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

        // POST: api/ProductRatings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProductRating>> PostProductRating(ProductRating productRating)
        {
            _context.ProductRatings.Add(productRating);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductRating", new { id = productRating.Id }, productRating);
        }

        // DELETE: api/ProductRatings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductRating(Guid id)
        {
            var productRating = await _context.ProductRatings.FindAsync(id);
            if (productRating == null)
            {
                return NotFound();
            }

            _context.ProductRatings.Remove(productRating);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductRatingExists(Guid id)
        {
            return _context.ProductRatings.Any(e => e.Id == id);
        }
    }
}
