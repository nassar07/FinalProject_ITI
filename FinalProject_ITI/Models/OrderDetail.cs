namespace FinalProject_ITI.Models;

public class OrderDetail
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public int? OrderID { get; set; }
    public int ProductID { get; set; }
    public int BrandID { get; set; }
    public Brand? Brand { get; set;}
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
