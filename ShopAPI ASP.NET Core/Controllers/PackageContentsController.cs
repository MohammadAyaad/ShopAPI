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
    public class PackageContentsController : ControllerBase
    {
        private readonly ShopDBContext _context;

        public PackageContentsController(ShopDBContext context)
        {
            _context = context;
        }

        // GET: api/PackageContents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PackageContent>>> GetPackageContent()
        {
            return await _context.PackageContent.ToListAsync();
        }

        // GET: api/PackageContents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PackageContent>> GetPackageContent(int id)
        {
            var packageContent = await _context.PackageContent.FindAsync(id);

            if (packageContent == null)
            {
                return NotFound();
            }

            return packageContent;
        }

        // PUT: api/PackageContents/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPackageContent(int id, PackageContent packageContent)
        {
            if (id != packageContent.Id)
            {
                return BadRequest();
            }

            _context.Entry(packageContent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PackageContentExists(id))
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

        // POST: api/PackageContents
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PackageContent>> PostPackageContent(PackageContent packageContent)
        {
            _context.PackageContent.Add(packageContent);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPackageContent", new { id = packageContent.Id }, packageContent);
        }

        // DELETE: api/PackageContents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackageContent(int id)
        {
            var packageContent = await _context.PackageContent.FindAsync(id);
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
