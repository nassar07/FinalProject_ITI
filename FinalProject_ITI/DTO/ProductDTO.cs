using FinalProject_ITI.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FinalProject_ITI.DTO;

public class ProductDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int BrandID { get; set; }

    public IFormFile? ImageFile { get; set; }
    public List<Review>? Reviews { get; set; }
    public List<OrderDetail>? OrderDetails { get; set; }
}
