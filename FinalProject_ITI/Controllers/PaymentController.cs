using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace FinalProject_ITI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IRepository<Order> _Order;
    public PaymentController(IConfiguration config, IRepository<Order> Order)
    {
        _config = config;
        _Order = Order;
    }

    [HttpPost("create-payment-intent")]
    public IActionResult CreatePaymentIntent(CreatePaymentDTO DTO)
    {
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(DTO.Total * 100),
            Currency = "usd",
            PaymentMethodTypes = new List<string> { "card" }
        };

        var service = new PaymentIntentService();
        var paymentIntent = service.Create(options);

        return Ok(new
        {
            clientSecret = paymentIntent.ClientSecret,
            paymentIntentId = paymentIntent.Id
        });
    }


    [HttpPost("refund/{orderId}")]
    public IActionResult RefundPayment(int orderId)
    {
        // Load order + payment
        var order = _Order.GetQuery().Include(o => o.Payment)
            .FirstOrDefault(o => o.Id == orderId);

        if (order == null || order.Payment == null)
            return NotFound("Order or payment not found");

        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        var refundService = new RefundService();
        var refund = refundService.Create(new RefundCreateOptions
        {
            PaymentIntent = order.Payment.TransactionReference
        });

        // Update status
        order.Payment.PaymentStatus = "Refunded";
        order.Status = OrderStatus.Cancelled;
        _Order.SaveChanges();

        return Ok(refund);
    }
}
