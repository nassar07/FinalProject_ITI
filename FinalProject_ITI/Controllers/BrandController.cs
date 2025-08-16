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

        [HttpGet("all")]
        public async Task<IActionResult> GetAllBrands()
        {
            var brands = await _brand.GetQuery().Include(b=> b.Category).Include(b => b.Products).ThenInclude(p => p.Reviews)
                .Select(b => new BrandReadDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    Address = b.Address,
                    ImageFile = b.Image,
                    ProfileImage = b.ProfileImage,
                    CategoryID = b.CategoryID,
                    CategoryName = b.Category.Name,
                    ProductCount = b.Products.Count,
                    OwnerID = b.OwnerID,
                    SubscribeID = b.SubscribeID,
                    AverageRating = b.Products.SelectMany(p => p.Reviews).Average(r => (double?)r.Rating) ?? 0
                })
                .ToListAsync();

            return Ok(brands);
        }

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

        [HttpPost("add")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateBrand([FromForm] BrandDTO Brand)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? imagePath = null;
            string? profileImagePath = null;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (Brand.ImageFile != null && Brand.ImageFile.Length > 0)
            {
                var extension = Path.GetExtension(Brand.ImageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Unsupported image format for ImageFile" });

                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Brands");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Brand.ImageFile.CopyToAsync(fileStream);
                    }

                    imagePath = Path.Combine("Brands", uniqueFileName).Replace("\\", "/");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Image upload failed", error = ex.Message });
                }
            }

            if (Brand.ProfileImage != null && Brand.ProfileImage.Length > 0)
            {
                var extension = Path.GetExtension(Brand.ProfileImage.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Unsupported image format for ProfileImage" });

                try
                {
                    var profileFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Brands", "ProfileImages");
                    if (!Directory.Exists(profileFolder))
                        Directory.CreateDirectory(profileFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(profileFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Brand.ProfileImage.CopyToAsync(fileStream);
                    }

                    profileImagePath = Path.Combine("Brands", "ProfileImages", uniqueFileName).Replace("\\", "/");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Profile image upload failed", error = ex.Message });
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
                ProfileImage = profileImagePath ?? "",
                SubscribeID = Brand.SubscribeID
            };

            try
            {
                await _brand.Add(NewBrand);
                await _brand.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving brand", error = ex.Message });
            }

            return Ok(new { message = "Brand added successfully" });
        }

        [HttpPut("update")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateBrand([FromForm] BrandDTO Brand)
        {
            var existedBrand = await _brand.GetById(Brand.Id);
            if (existedBrand == null)
                return NotFound(new { message = "Brand not found" });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (Brand.ImageFile != null && Brand.ImageFile.Length > 0)
            {
                var extension = Path.GetExtension(Brand.ImageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Unsupported image format for ImageFile" });

                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Brands");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Brand.ImageFile.CopyToAsync(fileStream);
                    }

                    if (!string.IsNullOrEmpty(existedBrand.Image))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existedBrand.Image.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                            System.IO.File.Delete(oldImagePath);
                    }

                    existedBrand.Image = Path.Combine("Brands", uniqueFileName).Replace("\\", "/");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Image upload failed", error = ex.Message });
                }
            }

            if (Brand.ProfileImage != null && Brand.ProfileImage.Length > 0)
            {
                var extension = Path.GetExtension(Brand.ProfileImage.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Unsupported image format for ProfileImage" });

                try
                {
                    var profileFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Brands", "ProfileImages");
                    if (!Directory.Exists(profileFolder))
                        Directory.CreateDirectory(profileFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(profileFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Brand.ProfileImage.CopyToAsync(fileStream);
                    }

                    if (!string.IsNullOrEmpty(existedBrand.ProfileImage))
                    {
                        var oldProfilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existedBrand.ProfileImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldProfilePath))
                            System.IO.File.Delete(oldProfilePath);
                    }

                    existedBrand.ProfileImage = Path.Combine("Brands", "ProfileImages", uniqueFileName).Replace("\\", "/");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Profile image upload failed", error = ex.Message });
                }
            }

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

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var existingBrand = await _brand.GetById(id);
            if (existingBrand == null)
            {
                return NotFound(new { message = "Brand not found" });
            }

            try
            {
                if (!string.IsNullOrEmpty(existingBrand.Image))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingBrand.Image.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                if (!string.IsNullOrEmpty(existingBrand.ProfileImage))
                {
                    var profileImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingBrand.ProfileImage.TrimStart('/'));
                    if (System.IO.File.Exists(profileImagePath))
                    {
                        System.IO.File.Delete(profileImagePath);
                    }
                }

                _brand.Delete(existingBrand);
                await _brand.SaveChanges();

                return Ok(new { message = "Brand deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting brand", error = ex.Message });
            }
        }


        [HttpGet("top")]
        public async Task<IActionResult> GetTopBrands()
        {
            var topBrands = await _brand.GetQuery().Include(b=>b.Category).Include(b => b.Products).ThenInclude(p => p.Reviews)
                .Where(b => b.Products.Any(p => p.Reviews.Any()))
                .Select(b => new BrandReadDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    Address = b.Address,
                    ImageFile = b.Image,
                    ProfileImage = b.ProfileImage,
                    CategoryID = b.CategoryID,
                    CategoryName = b.Category.Name,
                    ProductCount = b.Products.Count,
                    OwnerID = b.OwnerID,
                    SubscribeID = b.SubscribeID,
                    AverageRating = b.Products.SelectMany(p => p.Reviews).Average(r => (double?)r.Rating) ?? 0
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

        [HttpGet("HasBrand/{userId}")]
        public async Task<IActionResult> HasBrand(string userId)
        {
            var hasBrand = await _brand.GetQuery().AnyAsync(b => b.OwnerID == userId);
            return Ok(new { hasBrand });
        }

    }
}
