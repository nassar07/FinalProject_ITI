using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject_ITI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IRepository<Product> _Product;
        public ProductController(IRepository<Product> Product)
        {
            _Product = Product;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllProduct()
        {
            var Product = _Product.GetAll();
            return Ok(Product);
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetProductById(int ID)
        {
            var Res = await _Product.GetById(ID);

            if (Res == null) BadRequest("Product Doesn't exist");

            return Ok(Res);
        }

        [HttpPost("Order")]
        public async Task<IActionResult> AddProduct(Product Product)
        {
            if (ModelState.IsValid)
            {

                await _Product.Add(Product);
                await _Product.SaveChanges();

                return Ok("Product Placed Successfully");
            }
            return BadRequest(ModelState);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProduct(ProductDTO Product)
        {
            var Res = await _Product.GetById(Product.Id);

            if (Res == null) BadRequest("Product Doesn't exist");

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
            return Ok("Product Updated");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteOrder(int ID)
        {
            var order = await _Product.GetById(ID);

            if (order != null)
            {
                _Product.Delete(order);
                await _Product.SaveChanges();
                return Ok("Product deleted");
            }

            return BadRequest("Product Doesn't exist");
        }
    }
}
