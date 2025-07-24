using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ITI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IRepository<Product> _Product;
    public ProductController(IRepository<Product> Product)
    {
        _Product = Product;
    }

    [HttpGet("all/{brandID}")]
    public async Task<IActionResult> GetAllProduct(int brandID)
    {
        var Product = await _Product.GetQuery()
            .Where(p => p.BrandID == brandID)
            .ToListAsync();
        return Ok(Product);
    }

    [HttpGet("{ID}")]
    public async Task<IActionResult> GetProductById(int ID)
    {
        var Res = await _Product.GetById(ID);

        if (Res == null) BadRequest(new { message = "Product Doesn't exist" });

        return Ok(Res);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddProduct(ProductDTO Product)
    {
        if (ModelState.IsValid)
        {
            var NewProduct = new Product
            {
                Name = Product.Name,
                Price = Product.Price,
                Description = Product.Description,
                Quantity = Product.Quantity,
                Image = Product.Image,
                BrandID = Product.BrandID
            };
            await _Product.Add(NewProduct);
            await _Product.SaveChanges();
            return Ok(new { message = "Product Placed Successfully" });
        }
        return BadRequest(ModelState);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateProduct(ProductDTO Product)
    {
        var Res = await _Product.GetById(Product.Id);

        if (Res == null) BadRequest(new { message = "Product Doesn't exist" });

        //map here
        Res.Name = Product.Name;
        Res.Description = Product.Description;
        Res.Price = Product.Price;
        Res.Quantity = Product.Quantity;
        Res.Image = Product.Image;
        Res.BrandID = Product.BrandID;
        Res.Reviews = Product.Reviews;
        Res.OrderDetails = Product.OrderDetails;

        _Product.Update(Res);
        await _Product.SaveChanges();
        return Ok(new { message = "Product Updated" });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteOrder(int ID)
    {
        var order = await _Product.GetById(ID);

        if (order != null)
        {
            _Product.Delete(order);
            await _Product.SaveChanges();
            return Ok(new { message = "Product deleted" });
        }

        return BadRequest(new { message = "Product Doesn't exist" });
    }
}
