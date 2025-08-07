using Azure.Core.GeoJson;
using System.ComponentModel.DataAnnotations.Schema;
using GeoPoint = NetTopologySuite.Geometries.Point; 
namespace FinalProject_ITI.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Image { get; set; }

    public int BrandID { get; set; }
    public Brand? Brand { get; set; }

    public ICollection<Review>? Reviews { get; set; }
    public ICollection<OrderDetail>? OrderDetails { get; set; }

    [Column(TypeName = "geography")]
    public GeoPoint? Embedding { get; set; }
}
