using FinalProject_ITI.Models;

namespace FinalProject_ITI.DTO;

public class BazarDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Location { get; set; }
    public string? Entry { get; set; }
    public ICollection<BazarBrand>? BazarBrands { get; set; }
}
