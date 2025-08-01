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
                return BadRequest(new { message = "Brand doesn't exist" });
            }
            return Ok(res);
        }

        //Create Brand
        [HttpPost("add")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateBrand([FromForm] BrandDTO Brand)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string imagePath = null;

            if (Brand.ImageFile != null && Brand.ImageFile.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Brands");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Brand.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Brand.ImageFile.CopyToAsync(fileStream);
                    }

                    imagePath = "/Brands/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Image upload failed", error = ex.Message });
                }
            }

            var NewBrand = new Brand
            {
                Name = Brand.Name,
                Description = Brand.Description,
                Address = Brand.Address,
                OwnerID = Brand.OwnerID,
                CategoryID = Brand.CategoryID,
                Image = imagePath ?? "",
                SubscribeID = Brand.SubscribeID
            };

            await _brand.Add(NewBrand);
            await _brand.SaveChanges();

            return Ok(new { message = "Brand added successfully" });
        }

        [HttpPut("update")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateBrand([FromForm] BrandDTO Brand)
        {
            var existedBrand = await _brand.GetById(Brand.Id);
            if (existedBrand == null)
                return NotFound(new { message = "Brand not found" });

            // تحديث الصورة إن وُجدت
            if (Brand.ImageFile != null && Brand.ImageFile.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Brands");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Brand.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Brand.ImageFile.CopyToAsync(fileStream);
                    }

                    // حذف الصورة القديمة
                    if (!string.IsNullOrEmpty(existedBrand.Image))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existedBrand.Image.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                            System.IO.File.Delete(oldImagePath);
                    }

                    existedBrand.Image = "/Brands/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Image upload failed", error = ex.Message });
                }
            }

            // التحديثات الأخرى
            existedBrand.Name = Brand.Name;
            existedBrand.Description = Brand.Description;
            existedBrand.Address = Brand.Address;
            existedBrand.CategoryID = Brand.CategoryID;
            existedBrand.OwnerID = Brand.OwnerID;
            existedBrand.SubscribeID = Brand.SubscribeID;

            _brand.Update(existedBrand);
            await _brand.SaveChanges();

            return Ok(new { message = "Brand updated successfully" });
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

            // Delete image file from disk (if exists)
            if (!string.IsNullOrEmpty(existingBrand.Image))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingBrand.Image.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _brand.Delete(existingBrand);
            await _brand.SaveChanges();
            return Ok(new { message = "Brand deleted successfully" });
        }

        [HttpGet("top")]
        public async Task<IActionResult> GetTopBrands()
        {
            var topBrands = await _brand.GetQuery().Include(b => b.Products)
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

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBrandsByUserId(string userId)
        {
            var brands = await _brand.GetQuery().Where(b => b.OwnerID == userId).ToListAsync();

            if (brands == null || !brands.Any())
            {
                return NotFound(new { message = "No brands found for this user." });
            }

            return Ok(brands);
        }
    }
}
