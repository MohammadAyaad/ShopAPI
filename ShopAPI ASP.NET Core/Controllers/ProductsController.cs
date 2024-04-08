using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ShopAPI.Data;
using ShopAPI.Model.Products;
using ShopAPI.Authorization;
using static ShopAPI.Model.Moderation.Permissions;
using static ShopAPI.Model.Moderation.UserRoles;
using System.Net;
using JsonTokens.ComponentBasedTokens.ComponentSet;
using ShopAPI.Model.TokenComponents;

namespace ShopAPI.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopDBContext _context;
        

        public ProductsController(ShopDBContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromHeader(Name = "Authorization")] string authorization,[FromQuery(Name = "index")] uint index = 0, [FromQuery(Name = "count")] uint count = 20)
        {
            var result = AuthorizationService.AuthorizeAccess(authorization, _context, READ_PRODUCTS);

            (JCST userToken, string email,AccessToken access) = result.Value;

            if (userToken == null) return result.Result;
            
            if (count > 50) return BadRequest();
            return _context.Products.Skip((int)index).Take((int)count).ToList();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct([FromHeader(Name = "Authorization")] string authorization, int id, [FromQuery(Name = "rating_index")] uint rating_index = 0, [FromQuery(Name = "rating_count")] uint ratings_count = 20)
        {
            var result = AuthorizationService.AuthorizeAccess(authorization, _context, READ_PRODUCTS);

            (JCST userToken, string email, AccessToken access) = result.Value;

            if (userToken == null) return result.Result;
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            product.ProductVariants = _context.ProductVariants.Where(p => p.ProductId == product.Id).ToList();

            product.ProductRatings = _context.ProductRatings.Where(r => r.ProductId == product.Id).Skip((int)rating_index).Take((int)ratings_count).ToList();

            return product;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct([FromHeader(Name = "Authorization")] string authorization, int id, Product product)
        {
            var result = AuthorizationService.AuthorizeAccess(authorization, _context, EDIT_PRODUCTS);

            (JCST userToken, string email, AccessToken access) = result.Value;

            if (userToken == null) return result.Result;

            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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
        public async Task<ActionResult<Product>> PostProduct([FromHeader(Name = "Authorization")] string authorization, Product product)
        {
            var result = AuthorizationService.AuthorizeAccess(authorization, _context, CREATE_PRODUCTS);

            (JCST userToken, string email, AccessToken access) = result.Value;

            if (userToken == null) return result.Result;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct([FromHeader(Name = "Authorization")] string authorization, int id)
        {
            var result = AuthorizationService.AuthorizeAccess(authorization, _context, DELETE_PRODUCTS);

            (JCST userToken, string email, AccessToken access) = result.Value;

            if (userToken == null) return result.Result;

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
