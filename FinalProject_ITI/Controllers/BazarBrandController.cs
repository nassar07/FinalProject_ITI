using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Implementations;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ITI.Controllers;

//[Authorize(Roles = "ADMIN")]
[Route("api/[controller]")]
[ApiController]
public class BazarBrandController : ControllerBase
{
    private readonly IRepository<BazarBrand> _BazarBrand;
    private readonly IBazarBrandRepository<BazarBrand> _BazarBrandRepository;
    public BazarBrandController(IRepository<BazarBrand> BazarBrand, IBazarBrandRepository<BazarBrand> BazarBrandRepository)
    {
        _BazarBrand = BazarBrand;
        _BazarBrandRepository = BazarBrandRepository;
    }

    [HttpPost("AddBrandToBazar/{BazarId}/{BrandId}")]
    public async Task<IActionResult> AddBrandToBazar(int BazarId, int BrandId)
    {
        var existed = await _BazarBrandRepository.FirstOrDefaultAsync(b => b.BazarID == BazarId && b.BrandID == BrandId);

        if (existed != null)
            return BadRequest(new { message = "Brand already exists in the Bazar." });

        var newRelation = new BazarBrand
        {
            BazarID = BazarId,
            BrandID = BrandId
        };

        await _BazarBrand.Add(newRelation);
        await _BazarBrand.SaveChanges();

        return Ok(new { message = "Brand assigned to Bazar." });
    }

    [HttpDelete("RemoveBrandFromBazar/{BazarId}/{BrandId}")]
    public async Task<IActionResult> RemoveBrandFromBazar(int BazarId, int BrandId)
    {
        var existed = await _BazarBrandRepository.FirstOrDefaultAsync(b => b.BazarID == BazarId && b.BrandID == BrandId);

        if (existed == null)
            return NotFound(new { message = "Brand not found in the Bazar" });

        _BazarBrand.Delete(existed);
        await _BazarBrand.SaveChanges();

        return Ok(new { message = "Brand removed from Bazar." });
    }

    [HttpGet("{bazarId}/brands")]
    public async Task<IActionResult> GetBrandsInBazar(int bazarId)
    {
        var brands = await _BazarBrand.GetQuery()
            .Where(bb => bb.BazarID == bazarId)
            .Include(bb => bb.Brand).ThenInclude(c=>c.Category)
            .Select(bb => new BrandReadDTO
            {
                Id = bb.Brand.Id,
                Name = bb.Brand.Name,
                Description = bb.Brand.Description,
                Address = bb.Brand.Address,
                ImageFile = bb.Brand.Image,
                ProfileImage = bb.Brand.ProfileImage,
                CategoryID = bb.Brand.CategoryID,
                ProductCount = bb.Brand.Products.Count,
                CategoryName = bb.Brand.Category.Name,
                OwnerID = bb.Brand.OwnerID,
                SubscribeID = bb.Brand.SubscribeID,
                AverageRating = bb.Brand.Products
                    .SelectMany(p => p.Reviews)
                    .Average(r => (double?)r.Rating) ?? 0
            })
            .ToListAsync();

        return Ok(brands);
    }


    [HttpGet("brand/{brandId}/bazars")]
    public async Task<IActionResult> GetBazarsForBrand(int brandId)
    {
        var bazars = await _BazarBrand.GetQuery()
            .Where(bb => bb.BrandID == brandId)
            .Include(bb => bb.Bazar)
            .Select(bb => new
            {
                bb.Bazar.Id,
                bb.Bazar.Title
            })
            .ToListAsync();

        return Ok(bazars);
    }
}
