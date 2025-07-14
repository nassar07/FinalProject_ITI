using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Implementations;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject_ITI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IRepository<Order> _Order;
    public OrderController(IRepository<Order> Order)
    {
        _Order = Order;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders()
    {
        var Orders = _Order.GetQuery();
        return Ok(Orders);
    }

    [HttpGet("{ID}")]
    public async Task<IActionResult> GetOrdersById(int ID)
    {
        var Res = await _Order.GetById(ID);

        if (Res == null) BadRequest("order Doesn't exist");

        return Ok(Res);
    }

    [HttpPost("Order")]
    public async Task<IActionResult> OrderProduct(Order Order)
    {
        if (ModelState.IsValid)
        {

            await _Order.Add(Order);
            await _Order.SaveChanges();

            return Ok("Order Placed Successfully");
        }
        return BadRequest(ModelState);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateOrder(Order Order)
    {
        var Res = await _Order.GetById(Order.Id);

        if (Res == null) BadRequest("Order Doesn't exist");

        //map here
        Res.OrderDate = Order.OrderDate;
        Res.Status = Order.Status;
        Res.TotalAmount = Order.TotalAmount;
        Res.DeliveryBoyID = Order.DeliveryBoyID;
        Res.OrderTypeID = Order.OrderTypeID;
        Res.OrderDetails = Order.OrderDetails;

        _Order.Update(Res);
        await _Order.SaveChanges();
        return Ok("Order Updated");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteOrder(int ID)
    {
        var order = await _Order.GetById(ID);

        if (order != null)
        {
            _Order.Delete(order);
            await _Order.SaveChanges();
            return Ok("Order deleted");
        }

        return BadRequest("Order Doesn't exist");
    }
}
