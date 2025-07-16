using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ITI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandController : ControllerBase
    {
        private readonly IRepository<Brand> _brand;

        public BrandController(IRepository<Brand> brand)
        {
            _brand = brand;
        }

        //Get All Brands
        [HttpGet("all")]
        public async Task<IActionResult> GetAllBrands()
        {
            var brands = await _brand.GetQuery()
                .Include(b => b.Category)
                .Include(b => b.Products)
                    .ThenInclude(p => p.Reviews)
                .Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    Address = b.Address,
                    Image = b.Image,
                    Category = b.Category.Name,
                    ProductCount = b.Products.Count,
                    AverageRating = b.Products.SelectMany(p => p.Reviews).Any()
                        ? b.Products.SelectMany(p => p.Reviews).Average(r => (double?)r.Rating) ?? 0
                        : 0
                })
                .ToListAsync();

            return Ok(brands);
        }

        //Get Brand By Id
        [HttpGet("{id}")]
        public async Task<IActionResult>GetBrandById(int id)
        {
            var res = await _brand.GetById(id);
            if (res == null)
            {
                return BadRequest("Brand doesn't exist");
            }
            return Ok(res);

        }

        //Create Brand
        [HttpPost("add")]
        public async Task<IActionResult> CreateBrand( BrandCreateDto brandDto)
        {
            if (brandDto == null)
            {
                return BadRequest("Brand data is null");
            }
            var brand = new Brand
            {
                Name = brandDto.Name,
                Description = brandDto.Description,
                Address = brandDto.Address,
                Image = brandDto.Image,
                CategoryID = brandDto.CategoryID,
                OwnerID = brandDto.OwnerID
            };
            await _brand.Add(brand);
            await _brand.SaveChanges();
            return Ok("Brand added successfully");
        }
        //Update Brand
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateBrand(int id, BrandCreateDto brandDto)
        {
            if (brandDto == null)
            {
                return BadRequest("Brand data is null");
            }
            var existingBrand = await _brand.GetById(id);
            if (existingBrand == null)
            {
                return NotFound("Brand not found");
            }
            existingBrand.Name = brandDto.Name;
            existingBrand.Description = brandDto.Description;
            existingBrand.Address = brandDto.Address;
            existingBrand.Image = brandDto.Image;
            existingBrand.CategoryID = brandDto.CategoryID;
            existingBrand.OwnerID = brandDto.OwnerID;
            _brand.Update(existingBrand);
            await _brand.SaveChanges();
            return Ok("Brand updated successfully");
        }
        //Delete Brand
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var existingBrand = await _brand.GetById(id);
            if (existingBrand == null)
            {
                return NotFound("Brand not found");
            }
            _brand.Delete(existingBrand);
            await _brand.SaveChanges();
            return Ok("Brand deleted successfully");
        }

        //filter Brands by Category




        //GET: api/brand/top
       [HttpGet("top")]
        public async Task<IActionResult> GetTopBrands()
        {
            var topBrands = await _brand.GetQuery()
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
