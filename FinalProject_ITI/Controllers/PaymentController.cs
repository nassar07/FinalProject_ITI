using FinalProject_ITI.Models;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace FinalProject_ITI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IConfiguration _config;
    public PaymentController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("create-payment-intent")]
    public IActionResult CreatePaymentIntent(Payment1 Payment)
    {
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"]; ;

        var options = new PaymentIntentCreateOptions
        {
            Amount = Payment.Total,
            Currency = "usd",
            PaymentMethodTypes = new List<string> { "card" }
        };

        var service = new PaymentIntentService();
        var paymentIntent = service.Create(options);    

        return Ok(new { clientSecret = paymentIntent.ClientSecret });
    }
}
