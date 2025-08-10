using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ITI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DeliveryOrdersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _UserManager;
    private readonly IRepository<Order> _Order;
    private readonly IRepository<ApplicationUser> _User;
    public DeliveryOrdersController(IRepository<Order> Order, UserManager<ApplicationUser> UserManager, IRepository<ApplicationUser> User)
    {
        _Order = Order;
        _UserManager = UserManager;
        _User = User;
    }

    [HttpGet("MyOrders/{deliveryBoyId}")]
    public async Task<IActionResult> GetMyAssignedOrders(string deliveryBoyId)
    {
        var MyOrders = await _Order.GetQuery()
           .Where(o => o.DeliveryBoyID == deliveryBoyId)
           .Include(o => o.OrderDetails)
           .Select(o => new
           {
               o.Id,
               o.OrderDate,
               o.Status,
               o.PaymentMethod,
               o.UserID,
               o.DeliveryBoyID,
               o.IsCashDeliveredToBrand,
               o.IsDeliveryFeesCollected,
               o.TotalAmount,
               OrderDetails = o.OrderDetails.Select(od => new
               {
                   od.ProductID,
                   od.Quantity,
                   od.Price
               })
           })
           .ToListAsync();

        return Ok(MyOrders);
    }

    [HttpGet("Available")]
    public async Task<IActionResult> GetAvailableOrders()
    {
        var availableOrders = await _Order.GetQuery()
            .Where(o => o.Status == OrderStatus.Available)
            .Include(o => o.OrderDetails)
            .Select(o => new
            {
                o.Id,
                o.OrderDate,
                o.Status,
                o.PaymentMethod,
                o.UserID,
                o.DeliveryBoyID,
                o.IsCashDeliveredToBrand,
                o.IsDeliveryFeesCollected,
                o.TotalAmount,
                OrderDetails = o.OrderDetails.Select(od => new
                {
                    od.ProductID,
                    od.Quantity,
                    od.Price
                })
            })
            .ToListAsync();

        return Ok(availableOrders);
    }

    [HttpGet("MyHistory/{deliveryBoyId}")]
    public async Task<IActionResult> GetMyOrders(string deliveryBoyId)
    {
        var orders = await _Order.GetQuery()
          .Where(o =>
                (o.Status == OrderStatus.Delivered ||
                 o.Status == OrderStatus.Cancelled ||
                 o.Status == OrderStatus.CashDelivered) &&
                o.DeliveryBoyID == deliveryBoyId
            )
            .Include(o => o.OrderDetails)
            .Select(o => new
            {
                o.Id,
                o.OrderDate,
                o.Status,
                o.PaymentMethod,
                o.UserID,
                o.DeliveryBoyID,
                o.IsCashDeliveredToBrand,
                o.IsDeliveryFeesCollected,
                o.TotalAmount,
                OrderDetails = o.OrderDetails.Select(od => new
                {
                    od.ProductID,
                    od.Quantity,
                    od.Price
                })
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpPut("AssignOrderToDelivery/{orderId}/{deliveryBoyId}")]
    public async Task<IActionResult> AssignOrderToDelivery(int orderId, string deliveryBoyId)
    {
        // Get delivery user with orders
        var deliveryUser = await _User.GetQuery()
            .Include(u => u.AssignedOrders)
            .FirstOrDefaultAsync(u => u.Id == deliveryBoyId);

        if (deliveryUser == null || !await _UserManager.IsInRoleAsync(deliveryUser, "DeliveryBoy"))
            return BadRequest(new { message = "Invalid delivery boy." });

        // Check how many active orders the delivery boy already has
        int activeOrdersCount = deliveryUser.AssignedOrders
            ?.Count(o => o.Status == OrderStatus.DeliveryBrandHandingRequest || o.Status == OrderStatus.OutForDelivery) ?? 0;

        if (activeOrdersCount >= 3)
            return BadRequest("This delivery person already has 3 active orders.");

        // Retrieve order
        var order = await _Order.GetById(orderId);
        if (order == null)
            return NotFound(new { message = "Order not found." });

        if (order.Status == OrderStatus.Cancelled)
            return BadRequest(new { message = "Order is Cancelled." });

        if (order.Status == OrderStatus.OutForDelivery)
            return BadRequest(new { message = "Order is already Out For Delivery." });

        if (order.Status == OrderStatus.Delivered)
            return BadRequest(new { message = "Order is already Delivered." });

        // Assign
        order.DeliveryBoyID = deliveryBoyId;
        order.Status = OrderStatus.DeliveryBrandHandingRequest;

        await _Order.SaveChanges();

        return Ok(new { message = "Order assigned successfully." });
    }


    [HttpPut("Release/{orderId}/{deliveryBoyId}")]
    public async Task<IActionResult> ReleaseOrder(int orderId, string deliveryBoyId)
    {
        // Retrieve the order and check if it belongs to the delivery boy
        var order = await _Order.GetQuery()
            .Include(o => o.DeliveryBoy)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.DeliveryBoyID == deliveryBoyId);

        if (order == null)
            return BadRequest(new { message = "Order does not exist or is not assigned to this delivery boy." });

        // Check if it's already delivered or cancelled
        if (order.Status == OrderStatus.Delivered)
            return BadRequest(new { message = "Delivered orders cannot be released." });
        if (order.Status == OrderStatus.Cancelled)
            return BadRequest(new { message = "Cancelled orders cannot be released." });

        // Change status to Available and unassign the delivery boy
        order.Status = OrderStatus.DeliveryBrandHandingRequest;
        order.DeliveryBoyID = null;

        await _Order.SaveChanges();

        return Ok(new { message = "Order released successfully." });
    }

    [HttpPut("Deliver/{orderId}/{deliveryBoyId}")]
    public async Task<IActionResult> DeliverOrder(int orderId, string deliveryBoyId)
    {
        // Get the order assigned to the specific delivery boy
        var order = await _Order.GetQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.DeliveryBoyID == deliveryBoyId);

        if (order == null)
            return NotFound(new { message = "Order not found or not assigned to this delivery boy." });

        // Ensure order is in the correct state
        if (order.Status != OrderStatus.OutForDelivery)
            return BadRequest(new { message = "Order is not currently out for delivery." });

        // Mark as delivered
        order.Status = OrderStatus.Delivered;

        await _Order.SaveChanges();

        return Ok(new { message = "Order marked as delivered successfully." });
    }

    [HttpGet("delivery/{deliveryId}/active-orders")]
    public async Task<IActionResult> GetActiveOrdersForDelivery(string deliveryId)
    {
        var activeOrders = await _Order.GetQuery()
            .Where(o => o.DeliveryBoyID == deliveryId && 
            (o.Status == OrderStatus.DeliveryBrandHandingRequest || o.Status == OrderStatus.OutForDelivery))
            .ToListAsync();

        return Ok(activeOrders);
    }

    [HttpPost("Cancel")]
    public async Task<IActionResult> CancelOrder(int orderId, string deliveryBoyId)
    {
        var order = await _Order.GetQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.DeliveryBoyID == deliveryBoyId);

        if (order == null)
            return NotFound(new { message = "Order not found or not assigned to this delivery boy." });

        if (order.Status != OrderStatus.OutForDelivery)
            return BadRequest(new { message = "Only out-for-delivery orders can be cancelled." });

        order.Status = OrderStatus.Cancelled;

        order.DeliveryBoyID = null;

        await _Order.SaveChanges();

        return Ok(new { message = "Order cancelled successfully." });
    }
}
