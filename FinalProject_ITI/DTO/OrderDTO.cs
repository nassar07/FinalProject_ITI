﻿using FinalProject_ITI.Models;

namespace FinalProject_ITI.DTO;

public class OrderDTO
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public int? OrderTypeID { get; set; }
    public string UserID { get; set; }
    public string? DeliveryBoyID { get; set; }
    public ICollection<OrderDetailsDTO> OrderDetails { get; set; }
}
