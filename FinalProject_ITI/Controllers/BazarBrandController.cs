using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject_ITI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BazarBrandController : ControllerBase
{
    private readonly Repository<BazarBrand> _BazarBrand;
    private readonly Repository<Brand> _Brand;
    private readonly Repository<Bazar> _Bazar;
    private readonly BrandRepository<BazarBrand> _BrandRepository;
    public BazarBrandController(Repository<BazarBrand> BazarBrand, Repository<Bazar> Bazar, Repository<Brand> Brand, BrandRepository<BazarBrand> BrandRepository)
    {
        _BazarBrand = BazarBrand;
        _Bazar = Bazar;
        _Brand = Brand;
        _BrandRepository = BrandRepository;
    }

    public async Task<IActionResult> AddBrandToBazar(int BazarId, Brand Brand) {

        int brandId = Brand.Id;
        var existed = await _BrandRepository.AnyAsync(b => b.BazarID == BazarId && b.BrandID == brandId);

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

        //check if exist
        //remove the brand from the bazar
        //keep the bazar up
        int brandId = Brand.Id;
        var existed = await _BrandRepository.FirstOrDefaultAsync(b => b.BazarID == BazarId && b.BrandID == brandId);

        if (existed != null) return BadRequest("Brand already existed in the Bazar");

         _BazarBrand.Delete(existed!);
        await _BazarBrand.SaveChanges();

        return Ok("Brand removed from Bazar.");
    }

}
