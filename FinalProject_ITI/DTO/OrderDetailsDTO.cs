using FinalProject_ITI.Models;

namespace FinalProject_ITI.DTO;

public class OrderDetailsDTO
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public int OrderID { get; set; }
    public int ProductID { get; set; }
}
