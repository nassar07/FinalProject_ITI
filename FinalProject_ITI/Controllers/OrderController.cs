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
    private readonly IRepository<Product> _Products;
    private readonly IRepository<OrderDetail> _OrderDetail;
    public OrderController(IRepository<Order> Order, IRepository<OrderDetail> OrderDetail, IRepository<Product> Products)
    {
        _Order = Order;
        _OrderDetail = OrderDetail;
        _Products = Products;
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
        if (orders == null) return BadRequest(new { message = "there is no order" });
        return Ok(orders);
    }

    [HttpGet("{ID}")]
    public async Task<IActionResult> GetOrdersById(int ID)
    {
        var Res = await _Order.GetQuery()
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.Id == ID);

        if (Res == null) return BadRequest(new { message = "order Doesn't exist" });

        return Ok(Res);
    }

    [HttpGet("Brand/{brandId}")]
    public async Task<IActionResult> GetOrdersByBrand(int brandId)
    {
        var result = await _Order.GetQuery()
        .Where(o => o.OrderDetails.Any(od => od.BrandID == brandId))
        .Select(o => new {
            o.Id,
            o.OrderDate,
            o.Status,
            o.PaymentMethod,
            o.TotalAmount,
            o.UserID,
            o.DeliveryBoyID,
            OrderDetails = o.OrderDetails
                .Where(od => od.BrandID == brandId)
                .Select(od => new {
                    od.ProductID,
                    od.Quantity,
                    od.Price
                })
        }).ToListAsync();
        return Ok(result);
    }

    [HttpPost("CreateOrder")]
    public async Task<IActionResult> CreateOrder(OrderDTO Order)
    {
        var NewOrder = new Order
        {
            OrderDate = Order.OrderDate,
            Status = Order.Status,
            PaymentMethod = Order.PaymentMethod,
            TotalAmount = Order.TotalAmount,
            OrderTypeID = Order.OrderTypeID,
            UserID = Order.UserID,
            DeliveryBoyID = Order.DeliveryBoyID,
            OrderDetails = Order.OrderDetails.Select(d =>
            {
                var product = _Products.GetQuery().FirstOrDefault(p => p.Id == d.ProductID);
                if (product == null)
                {
                    throw new Exception($"Product with ID {d.ProductID} not found.");
                }

                return new OrderDetail
                {
                    ProductID = d.ProductID,
                    Quantity = d.Quantity,
                    Price = d.Price,
                    BrandID = product.BrandID
                };
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
            return BadRequest(new { message = "Order doesn't exist" });

        // Update order properties
        existingOrder.OrderDate = Order.OrderDate;
        existingOrder.Status = Order.Status;
        existingOrder.PaymentMethod = Order.PaymentMethod;
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
        existingOrder.OrderDetails = Order.OrderDetails.Select(d =>
        {
            var product = _Products.GetQuery().FirstOrDefault(p => p.Id == d.ProductID);
            if (product == null)
            {
                throw new Exception($"Product with ID {d.ProductID} not found.");
            }

            return new OrderDetail
            {
                ProductID = d.ProductID,
                Quantity = d.Quantity,
                Price = d.Price,
                BrandID = product.BrandID
            };
        }).ToList();

        _Order.Update(existingOrder);

        await _OrderDetail.SaveChanges();
        await _Order.SaveChanges();
        return Ok(new { message = "Order Updated" });
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteOrder(int ID)
    {
        var order = await _Order.GetQuery()
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == ID);

        if (order == null)
            return BadRequest(new { message = "Order doesn't exist" });

        // Remove related OrderDetails
        foreach (var detail in order.OrderDetails)
        {
            _OrderDetail.Delete(detail);
        }

        // Remove the Order itself
        _Order.Delete(order);

        await _Order.SaveChanges();
        await _OrderDetail.SaveChanges();

        return Ok(new { message = "Order deleted" });
    }
}
