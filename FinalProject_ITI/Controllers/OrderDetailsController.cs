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
        var Orders = await _OrderDetail.GetAll();
        return Ok(Orders);
    }

    [HttpGet("{ID}")]
    public async Task<IActionResult> GetOrdersById(int ID)
    {
        var Res = await _OrderDetail.GetById(ID);

        if (Res == null) BadRequest(new { message = "Product Doesn't exist" });

        return Ok(Res);
    }

    [HttpPost("Order")]
    public async Task<IActionResult> OrderProduct(OrderDetail OrderDetail)
    {
        if (ModelState.IsValid) {

            await _OrderDetail.Add(OrderDetail);
            await _OrderDetail.SaveChanges();

            return Ok(new { message = "Order Placed Successfully" });
        }
      return BadRequest(ModelState);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> UpdateOrder(OrderDetail OrderDetail)
    {
        var Order = await _OrderDetail.GetById(OrderDetail.Id);

        if (Order == null) BadRequest(new { message = "Order Doesn't exist" });

        Order!.Id = OrderDetail.Id;
        Order.Price = OrderDetail.Price;
        Order.Quantity = OrderDetail.Quantity;
        Order.ProductID = OrderDetail.ProductID;

        _OrderDetail.Update(Order);
        await _OrderDetail.SaveChanges();
        return Ok(new { message = "Order Updated" });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteOrder(int ID)
    {
        var order = await _OrderDetail.GetById(ID);

        if (order != null) {
            _OrderDetail.Delete(order);
            await _OrderDetail.SaveChanges();
            return Ok(new { message = "Order deleted" });
        }

        return BadRequest(new { message = "Order Doesn't exist" });
    }

}

