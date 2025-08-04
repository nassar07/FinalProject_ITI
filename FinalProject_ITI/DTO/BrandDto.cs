using FinalProject_ITI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject_ITI.DTO;
public class BrandDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    [FromForm]
    public IFormFile? ImageFile { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public int CategoryID { get; set; }
    public string OwnerID { get; set; }
    public int? SubscribeID { get; set; }
    public ICollection<Product>? Products { get; set; }
    public ICollection<BazarBrand>? BazarBrands { get; set; }
}
