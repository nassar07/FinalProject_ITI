using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ITI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IRepository<Order> _Order;
    private readonly IRepository<OrderDetail> _OrderDetail;
    public OrderController(IRepository<Order> Order, IRepository<OrderDetail> OrderDetail)
    {
        _Order = Order;
        _OrderDetail = OrderDetail;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders()
    {
        var Orders = await _Order.GetAll();
        return Ok(Orders);
    }

    [HttpGet("UserOrders/{userId}")]
    public async Task<IActionResult> GetUserOrders(string userId)
    {
        var orders = await _Order.GetQuery()
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
            .Where(o => o.UserID == userId)
            .ToListAsync();
        if (orders == null) return BadRequest("there is no order");
        return Ok(orders);
    }

    [HttpGet("{ID}")]
    public async Task<IActionResult> GetOrdersById(int ID)
    {
        var Res = await _Order.GetQuery()
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.Id == ID);

        if (Res == null) return BadRequest("order Doesn't exist");

        return Ok(Res);
    }

    [HttpPost("CreateOrder")]
    public async Task<IActionResult> CreateOrder(OrderDTO Order)
    {
        var NewOrder = new Order
        {
            OrderDate = Order.OrderDate,
            Status = Order.Status,
            TotalAmount = Order.TotalAmount,
            OrderTypeID = Order.OrderTypeID,
            UserID = Order.UserID,
            DeliveryBoyID = Order.DeliveryBoyID,
            OrderDetails = Order.OrderDetails.Select(d => new OrderDetail
            {
                ProductID = d.ProductID,
                Quantity = d.Quantity,
                Price = d.Price
            }).ToList()
        };

        await _Order.Add(NewOrder);
        await _Order.SaveChanges();
        return Ok(NewOrder);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateOrder(OrderDTO Order)
    {
        var existingOrder = await _Order.GetQuery()
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == Order.Id);

        if (existingOrder == null)
            return BadRequest("Order doesn't exist");

        // Update order properties
        existingOrder.OrderDate = Order.OrderDate;
        existingOrder.Status = Order.Status;
        existingOrder.TotalAmount = Order.TotalAmount;
        existingOrder.OrderTypeID = Order.OrderTypeID;
        existingOrder.UserID = Order.UserID;
        existingOrder.DeliveryBoyID = Order.DeliveryBoyID;

        // Handle OrderDetails
        // 1. Remove existing order details (if full replacement)
        foreach (var detail in existingOrder.OrderDetails)
        {
            _OrderDetail.Delete(detail);
        }

        // 2. Add new order details
        existingOrder.OrderDetails = Order.OrderDetails.Select(od => new OrderDetail
        {
            ProductID = od.ProductID,
            Quantity = od.Quantity,
            Price = od.Price,
        }).ToList();

        _Order.Update(existingOrder);

        await _OrderDetail.SaveChanges();
        await _Order.SaveChanges();
        return Ok("Order Updated");
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteOrder(int ID)
    {
        var order = await _Order.GetQuery()
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == ID);

        if (order == null)
            return BadRequest("Order doesn't exist");

        // Remove related OrderDetails
        foreach (var detail in order.OrderDetails)
        {
            _OrderDetail.Delete(detail);
        }

        // Remove the Order itself
        _Order.Delete(order);

        await _Order.SaveChanges();
        await _OrderDetail.SaveChanges();

        return Ok("Order deleted");
    }
}
