using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopAPI.Data;
using ShopAPI.Model.Packages;

namespace ShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackageRatingsController : ControllerBase
    {
        private readonly ShopDBContext _context;

        public PackageRatingsController(ShopDBContext context)
        {
            _context = context;
        }

        // GET: api/PackageRatings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PackageRating>>> GetPackageRatings()
        {
            return await _context.PackageRatings.ToListAsync();
        }

        // GET: api/PackageRatings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PackageRating>> GetPackageRating(Guid id)
        {
            var packageRating = await _context.PackageRatings.FindAsync(id);

            if (packageRating == null)
            {
                return NotFound();
            }

            return packageRating;
        }

        // PUT: api/PackageRatings/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPackageRating(Guid id, PackageRating packageRating)
        {
            if (id != packageRating.Id)
            {
                return BadRequest();
            }

            _context.Entry(packageRating).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PackageRatingExists(id))
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

        // POST: api/PackageRatings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PackageRating>> PostPackageRating(PackageRating packageRating)
        {
            _context.PackageRatings.Add(packageRating);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPackageRating", new { id = packageRating.Id }, packageRating);
        }

        // DELETE: api/PackageRatings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackageRating(Guid id)
        {
            var packageRating = await _context.PackageRatings.FindAsync(id);
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
}
