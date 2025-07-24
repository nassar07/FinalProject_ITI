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
    private readonly IRepository<Order> _Order;
    private readonly UserManager<ApplicationUser> _UserManager;
    public DeliveryOrdersController(IRepository<Order> Order, UserManager<ApplicationUser> UserManager)
    {
        _Order = Order;
        _UserManager = UserManager;
    }

    [HttpGet("MyOrders/{deliveryBoyId}")]
    public async Task<IActionResult> GetMyAssignedOrders(string deliveryBoyId)
    {
        var MyOrders = await _Order.GetQuery()
           .Where(o => o.Status == OrderStatus.OutForDelivery &&
           o.DeliveryBoyID == deliveryBoyId)
           .Include(o => o.OrderDetails)
           .Select(o => new
           {
               o.Id,
               o.OrderDate,
               o.Status,
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
            .Where(o => o.Status == OrderStatus.Delivered ||
            o.Status == OrderStatus.Cancelled &&
            o.DeliveryBoyID == deliveryBoyId)
            .Include(o => o.OrderDetails)
            .Select(o => new
            {
                o.Id,
                o.OrderDate,
                o.Status,
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
        // Validate user existence and role
        var deliveryUser = await _UserManager.FindByIdAsync(deliveryBoyId);
        if (deliveryUser == null || !await _UserManager.IsInRoleAsync(deliveryUser, "DeliveryBoy"))
            return BadRequest(new { message = "Invalid delivery boy." });

        // Retrieve order
        var order = await _Order.GetById(orderId);
        if (order == null)
            return NotFound(new { message = "Order not found." });

        // Check status
        if (order.Status == OrderStatus.Cancelled)
            return BadRequest(new { message = "Order is Cancelled." });
        if (order.Status == OrderStatus.OutForDelivery)
            return BadRequest(new { message = "Order is already Out For Delivery." });
        if (order.Status == OrderStatus.Delivered)
            return BadRequest(new { message = "Order is already Delivered." });

        // Assign delivery
        order.DeliveryBoyID = deliveryBoyId;
        order.Status = OrderStatus.OutForDelivery;

        // Save
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
        order.Status = OrderStatus.Available;
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

    //for admin can cancel order
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
