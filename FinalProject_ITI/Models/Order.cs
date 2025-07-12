namespace FinalProject_ITI.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }

    public string UserID { get; set; }
    public int? DeliveryBoyID { get; set; }
    public int OrderTypeID { get; set; }

    public ApplicationUser User { get; set; }
    public DeliveryBoy DeliveryBoy { get; set; }
    public OrderType OrderType { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; }
    public Payment Payment { get; set; }
}
