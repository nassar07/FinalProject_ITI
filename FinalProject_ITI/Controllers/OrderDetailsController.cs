using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject_ITI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderDetailsController : ControllerBase
{
    private readonly Repository<OrderDetail> _OrderDetail;
    public OrderDetailsController(Repository<OrderDetail> OrderDetail)
    {
        _OrderDetail = OrderDetail;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders() {
        var Orders = _OrderDetail.GetQuery();
        return Ok(Orders);
    }

    [HttpGet("{ID}")]
    public async Task<IActionResult> GetOrdersById(int ID)
    {
        var Res = await _OrderDetail.GetById(ID);

        if (Res == null) BadRequest("Product Doesn't exist");

        return Ok(Res);
    }

    [HttpPost("Order")]
    public async Task<IActionResult> OrderProduct(OrderDetail OrderDetail)
    {
        if (ModelState.IsValid) {

            await _OrderDetail.Add(OrderDetail);
            await _OrderDetail.SaveChanges();

            return Ok("Order Placed Successfully");
        }
      return BadRequest(ModelState);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> UpdateOrder(OrderDetail OrderDetail)
    {
        var Order = await _OrderDetail.GetById(OrderDetail.Id);

        if (Order == null) BadRequest("Order Doesn't exist");

        Order.Id = OrderDetail.Id;
        Order.Price = OrderDetail.Price;
        Order.Quantity = OrderDetail.Quantity;
        Order.ProductID = OrderDetail.ProductID;

        _OrderDetail.Update(Order);
        await _OrderDetail.SaveChanges();
        return Ok("Order Updated");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteOrder(int ID)
    {
        var order = await _OrderDetail.GetById(ID);

        if (order != null) {
            _OrderDetail.Delete(order);
            await _OrderDetail.SaveChanges();
            return Ok("Order deleted");
        }

        return BadRequest("Order Doesn't exist");
    }

}

