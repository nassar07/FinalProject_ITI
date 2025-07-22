using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetAllBrands() {

            var brands = await _brand.GetAll();
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
        public async Task<IActionResult> CreateBrand(Brand Brand)
        {
            if (ModelState.IsValid) {
                await _brand.Add(Brand);
                await _brand.SaveChanges();
                return Ok("Brand added successfully");
            }
            return BadRequest(Brand);
        }

        //Update Brand
        [HttpPut("update")]
        public async Task<IActionResult> UpdateBrand(BrandDTO Brand)
        {
            var existedBrand = await _brand.GetById(Brand.Id);
            if (existedBrand == null)
            {
                return NotFound("Brand not found");
            }
            existedBrand.Name = Brand.Name;
            existedBrand.Description = Brand.Description;
            existedBrand.Address = Brand.Address;
            existedBrand.Image = Brand.Image;
            existedBrand.CategoryID = Brand.CategoryID;
            existedBrand.OwnerID = Brand.OwnerID;
            existedBrand.SubscribeID = Brand.SubscribeID;

            _brand.Update(existedBrand);
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
       //[HttpGet("top")]
       // public async Task<IActionResult> GetTopBrands()
       // {
       //     var topBrands = await _brand.GetQuery()
       //         .Include(b => b.Products)
       //             .ThenInclude(p => p.Reviews)
       //         .Where(b => b.Products.Any(p => p.Reviews.Any()))
       //         .Select(b => new TopBrandDto
       //         {
       //             Id = b.Id,
       //             Name = b.Name,
       //             Description = b.Description,
       //             Image = b.Image,
       //             ProductCount = b.Products.Count,
       //             AverageRating = b.Products
       //                 .SelectMany(p => p.Reviews)
       //                 .Average(r => (double?)r.Rating) ?? 0
       //         })
       //         .OrderByDescending(b => b.AverageRating)
       //         .Take(6)
       //         .ToListAsync();

       //     return Ok(topBrands);
       // }
    }
}
