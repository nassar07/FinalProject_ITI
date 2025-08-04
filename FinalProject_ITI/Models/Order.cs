namespace FinalProject_ITI.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; }
    public int? OrderTypeID { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; }
    public string UserID { get; set; }
    public ApplicationUser? User { get; set; }
    public string? DeliveryBoyID { get; set; }
    public ApplicationUser? DeliveryBoy { get; set; }
    public OrderType? OrderType { get; set; }
    public Payment? Payment { get; set; }
}
