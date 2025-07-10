using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Implementations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ITI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BazarBrandController : ControllerBase
{
    private readonly Repository<BazarBrand> _BazarBrand;
    private readonly BazarBrandRepository<BazarBrand> _BazarBrandRepository;
    public BazarBrandController(Repository<BazarBrand> BazarBrand, BazarBrandRepository<BazarBrand> BrandRepository)
    {
        _BazarBrand = BazarBrand;
        _BazarBrandRepository = BrandRepository;
    }

    public async Task<IActionResult> AddBrandToBazar(int BazarId, Brand Brand) {

        int brandId = Brand.Id;
        var existed = await _BazarBrandRepository.AnyAsync(b => b.BazarID == BazarId && b.BrandID == brandId);

        if (!existed) return BadRequest("Brand already existed in the Bazar");

        BazarBrand NewBrand = new BazarBrand {
        BazarID = BazarId,
        BrandID = Brand.Id
        };
        await _BazarBrand.Add(NewBrand);
        await _BazarBrand.SaveChanges();

        return Ok("Brand assigned to Bazar.");
    }

    public async Task<IActionResult> RemoveBrandFromBazar(int BazarId, Brand Brand) {

        int brandId = Brand.Id;
        var existed = await _BazarBrandRepository.FirstOrDefaultAsync(b => b.BazarID == BazarId && b.BrandID == brandId);

        if (existed != null) return BadRequest("Brand already existed in the Bazar");

         _BazarBrand.Delete(existed!);
        await _BazarBrand.SaveChanges();

        return Ok("Brand removed from Bazar.");
    }

    [HttpGet("{bazarId}/brands")]
    public async Task<IActionResult> GetBrandsInBazar(int bazarId)
    {
        var brands = await _BazarBrand.GetQuery().Where(bb => bb.BazarID == bazarId)
            .Include(bb => bb.Brand) // eager load brand details
            .Select(bb => new {
                bb.Brand.Id,
                bb.Brand.Name,
                bb.Brand.Description
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
                bb.Bazar.Name
            })
            .ToListAsync();

        return Ok(bazars);
    }
}
