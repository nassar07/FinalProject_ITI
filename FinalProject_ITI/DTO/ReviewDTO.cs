using FinalProject_ITI.Models;

namespace FinalProject_ITI.DTO;

public class ReviewDTO
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserID { get; set; }
    public int ProductID { get; set; }
}
