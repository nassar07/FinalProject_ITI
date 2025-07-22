using FinalProject_ITI.Models;

namespace FinalProject_ITI.DTO;

public class ProductDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Image { get; set; }
    public int BrandID { get; set; }
    public ICollection<Review>? Reviews { get; set; }
    public ICollection<OrderDetail>? OrderDetails { get; set; }
}
