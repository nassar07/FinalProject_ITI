using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ITI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandController : ControllerBase
    {
        private readonly ITIContext _context;

        public BrandController(ITIContext context)
        {
            _context = context;
        }

        // GET: api/brand/top
        [HttpGet("top")]
        public async Task<IActionResult> GetTopBrands()
        {
            var topBrands = await _context.Brands
                .Include(b => b.Products) 
                    .ThenInclude(p => p.Reviews)
                .Where(b => b.Products.Any(p => p.Reviews.Any()))
                .Select(b => new TopBrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    Image = b.Image,
                    ProductCount = b.Products.Count,
                    AverageRating = b.Products
                        .SelectMany(p => p.Reviews)
                        .Average(r => (double?)r.Rating) ?? 0
                })
                .OrderByDescending(b => b.AverageRating)
                .Take(6)
                .ToListAsync();

            return Ok(topBrands);
        }
    }
}
