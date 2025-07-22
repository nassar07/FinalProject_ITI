namespace FinalProject_ITI.Models;

public class Review
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserID { get; set; }
    public int ProductID { get; set; }
    public ApplicationUser? User { get; set; }
    public Product? Product { get; set; }
}
