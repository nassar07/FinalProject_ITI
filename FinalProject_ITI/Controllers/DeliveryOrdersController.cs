using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Implementations;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ITI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DeliveryOrdersController : ControllerBase
{
    private readonly IRepository<Order> _Order;
    private readonly IRepository<DeliveryBoy> _DeliveryBoy;
    public DeliveryOrdersController(Repository<Order> Order, Repository<DeliveryBoy> DeliveryBoy)
    {
        _Order = Order;
        _DeliveryBoy = DeliveryBoy;
    }

    [HttpPost("Pick/{OrderId}/{DeliveryBoyId}")]
    public async Task<IActionResult> PickOrder(int OrderId, int DeliveryBoyId) {

        //get order from db
        Order Picked = await _Order.GetById(OrderId);

        if (Picked == null) return BadRequest("Order doesn't existe");

        //check Availability
        if (Picked?.Status == OrderStatus.Cancelled)
            return BadRequest("Order is Cancelled");

        if (Picked?.Status == OrderStatus.OutForDelivery)
            return BadRequest("Order is OutForDelivery");

        if (Picked?.Status == OrderStatus.Delivered)
            return BadRequest("Order is Delivered");


        //change Status
        Picked.Status = OrderStatus.OutForDelivery;

        //add delivry Boy Id to order
        var DeliveryP = await _DeliveryBoy.GetById(DeliveryBoyId);
        if (Picked == null) return BadRequest("DeliveryBoy doesn't existe");

        Picked.DeliveryBoyID = DeliveryP.Id;

        //add order to delivery Boy
        DeliveryP.Orders.Add(Picked);

        await _Order.SaveChanges();
        await _DeliveryBoy.SaveChanges();

        return Ok("Order Picked successfully");
    }

    [HttpPost("Release/{OrderId}/{DeliveryBoyId}")]
    public async Task<IActionResult> ReleaseOrder(int OrderId, int DeliveryBoyId)
    {
        // Check if this order was picked by the delivery boy
        var pickedBefore = _Order.GetQuery()
            .Include(o => o.DeliveryBoy)
            .FirstOrDefault(o => o.Id == OrderId && o.DeliveryBoyID == DeliveryBoyId);

        if (pickedBefore == null)
            return BadRequest("Order doesn't exist or wasn't assigned to this delivery boy.");

        // Change status to Available and remove the delivery boy
        pickedBefore.Status = OrderStatus.Available;
        pickedBefore.DeliveryBoyID = null; // Assuming it's nullable

        await _Order.SaveChanges();

        return Ok("Released order successfully.");
    }

    [HttpPost("Deliver/{OrderId}/{DeliveryBoyId}")]
    public async Task<IActionResult> DeliverOrder(int orderId, int deliveryBoyId)
    {
        var order = _Order.GetQuery()
            .FirstOrDefault(o => o.Id == orderId && o.DeliveryBoyID == deliveryBoyId);

        if (order == null)
            return NotFound("Order not found or not assigned to this delivery boy.");

        if (order.Status != OrderStatus.OutForDelivery)
            return BadRequest("Order is not out for delivery and cannot be marked as delivered.");

        order.Status = OrderStatus.Delivered;

        await _Order.SaveChanges();

        return Ok("Order marked as delivered.");
    }

    [HttpGet("Available")]
    public IActionResult GetAvailableOrders()
    {
        var availableOrders = _Order.GetQuery()
            .Where(o => o.Status == OrderStatus.Available)
            .ToList();

        return Ok(availableOrders);
    }

    [HttpGet("MyOrders")]
    public IActionResult GetMyOrders(int deliveryBoyId)
    {
        var orders = _Order.GetQuery()
            .Where(o => o.DeliveryBoyID == deliveryBoyId && o.Status != OrderStatus.Delivered)
            .ToList();

        return Ok(orders);
    }

    [HttpGet("Delivered")]
    public IActionResult GetDeliveryHistory(int deliveryBoyId)
    {
        var deliveredOrders = _Order.GetQuery()
            .Where(o => o.DeliveryBoyID == deliveryBoyId && o.Status == OrderStatus.Delivered)
            .ToList();

        return Ok(deliveredOrders);
    }

    [HttpPost("Cancel")]
    public async Task<IActionResult> CancelOrder(int orderId, int deliveryBoyId)
    {
        var order = _Order.GetQuery()
            .FirstOrDefault(o => o.Id == orderId && o.DeliveryBoyID == deliveryBoyId);

        if (order == null)
            return NotFound("Order not found or not assigned to this delivery boy.");

        if (order.Status != OrderStatus.OutForDelivery)
            return BadRequest("Only out-for-delivery orders can be cancelled.");

        order.Status = OrderStatus.Cancelled;
        await _Order.SaveChanges();

        return Ok("Order cancelled.");
    }
}
