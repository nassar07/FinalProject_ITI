using NetTopologySuite.Geometries;
using GeoPoint = NetTopologySuite.Geometries.Point;
namespace FinalProject_ITI.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public GeoPoint? Embedding { get; set; }
    public ICollection<Brand>? Brands { get; set; }
}
