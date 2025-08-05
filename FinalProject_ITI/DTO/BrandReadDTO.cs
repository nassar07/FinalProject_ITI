
namespace FinalProject_ITI.DTO;

public class BrandReadDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? ImageFile { get; set; }
    public string? ProfileImage { get; set; }
    public int? ProductCount { get; set; }
    public double? AverageRating { get; set; }
    public int CategoryID { get; set; }
    public string OwnerID { get; set; }
    public int? SubscribeID { get; set; }
}
