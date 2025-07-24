using FinalProject_ITI.Models;

namespace FinalProject_ITI.DTO;
public class BrandDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Image { get; set; }
    public int CategoryID { get; set; }
    public string OwnerID { get; set; }
    public int? SubscribeID { get; set; }
    public ICollection<Product>? Products { get; set; }
    public ICollection<BazarBrand>? BazarBrands { get; set; }
}
