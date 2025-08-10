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
    private readonly IRepository<Payment> _Payment;
    public OrderController(IRepository<Order> Order, IRepository<OrderDetail> OrderDetail, IRepository<Product> Products, IRepository<Payment> Payment)
    {
        _Order = Order;
        _OrderDetail = OrderDetail;
        _Products = Products;
        _Payment = Payment;
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
            .Include(o => o.Payment)
            .Where(o => o.UserID == userId)
            .ToListAsync();

        if (orders.Count == 0)
            return NotFound(new { message = "No orders found for this user." });

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _Order.GetQuery()
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Include(o => o.Payment) 
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound(new { message = "Order doesn't exist" });

        return Ok(order);
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
                o.IsCashDeliveredToBrand,
                o.IsDeliveryFeesCollected,
                Payment = o.Payment != null ? new
                {
                    o.Payment.PaymentMethod,
                    o.Payment.PaymentStatus,
                    o.Payment.TransactionReference,
                    o.Payment.PaymentDate,
                    o.Payment.Total
                } : null,
                OrderDetails = o.OrderDetails
                    .Where(od => od.BrandID == brandId)
                    .Select(od => new {
                        od.ProductID,
                        od.Quantity,
                        od.Price
                    }).ToList()
            })
            .ToListAsync();

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
            IsCashDeliveredToBrand = Order.IsCashDeliveredToBrand,
            IsDeliveryFeesCollected = Order.IsDeliveryFeesCollected,
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
            }).ToList(),

            Payment = Order.Payment == null ? null : new Payment
            {
                PaymentMethod = Order.Payment.PaymentMethod,
                PaymentStatus = Order.Payment.PaymentStatus,
                TransactionReference = Order.Payment.TransactionReference,
                PaymentDate = Order.Payment.PaymentDate,
                Total = (long)(Order.Payment.Total * 100), // if your entity stores in cents
            }
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
            return NotFound(new { message = "Order doesn't exist" });

        // Update order properties
        existingOrder.OrderDate = Order.OrderDate;
        existingOrder.Status = Order.Status;
        existingOrder.PaymentMethod = Order.PaymentMethod;
        existingOrder.TotalAmount = Order.TotalAmount;
        existingOrder.OrderTypeID = Order.OrderTypeID;
        existingOrder.UserID = Order.UserID;
        existingOrder.DeliveryBoyID = Order.DeliveryBoyID;
        existingOrder.IsCashDeliveredToBrand = Order.IsCashDeliveredToBrand;
        existingOrder.IsDeliveryFeesCollected = Order.IsDeliveryFeesCollected;

        // Remove existing order details
        foreach (var detail in existingOrder.OrderDetails)
        {
            _OrderDetail.Delete(detail);
        }

        // Add new order details
        var newOrderDetails = new List<OrderDetail>();
        foreach (var d in Order.OrderDetails)
        {
            var product = await _Products.GetQuery().FirstOrDefaultAsync(p => p.Id == d.ProductID);
            if (product == null)
                return BadRequest(new { message = $"Product with ID {d.ProductID} not found." });

            newOrderDetails.Add(new OrderDetail
            {
                ProductID = d.ProductID,
                Quantity = d.Quantity,
                Price = d.Price,
                BrandID = product.BrandID
            });
        }
        existingOrder.OrderDetails = newOrderDetails;

        _Order.Update(existingOrder);

        await _Order.SaveChanges();  // Save once after all updates

        return Ok(new { message = "Order updated successfully" });
    }


    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _Order.GetQuery()
            .Include(o => o.OrderDetails)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound(new { message = "Order doesn't exist" });

        // Remove related OrderDetails
        if (order.OrderDetails.Any()) {
            foreach (var details in order.OrderDetails)
            {
                _OrderDetail.Delete(details);
            }
        }

        // Remove related Payment if exists
        if (order.Payment != null)
            _Payment.Delete(order.Payment);

        // Remove the Order itself
        _Order.Delete(order);

        await _OrderDetail.SaveChanges();
        await _Payment.SaveChanges();
        await _Order.SaveChanges();

        return Ok(new { message = "Order deleted successfully" });
    }

}
