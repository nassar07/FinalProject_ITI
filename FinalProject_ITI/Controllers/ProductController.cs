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
            .Include(r => r.Reviews)
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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddProduct([FromForm] ProductDTO Product)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string imagePath = null;

        if (Product.ImageFile != null && Product.ImageFile.Length > 0)
        {
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Product.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                await Product.ImageFile.CopyToAsync(fileStream);

                imagePath = "/Products/" + uniqueFileName;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Image upload failed", error = ex.Message });
            }
        }

        var newProduct = new Product
        {
            Name = Product.Name,
            Price = Product.Price,
            Description = Product.Description,
            Quantity = Product.Quantity,
            BrandID = Product.BrandID,
            Image = imagePath
        };

        await _Product.Add(newProduct);
        await _Product.SaveChanges();

        return Ok(new { message = "Product Placed Successfully" });
    }


    [HttpPut("update")]
    public async Task<IActionResult> UpdateProduct([FromForm] ProductDTO productDto)
    {
        var product = await _Product.GetById(productDto.Id);
        if (product == null)
            return BadRequest(new { message = "Product doesn't exist." });

        if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
        {
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(productDto.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await productDto.ImageFile.CopyToAsync(fileStream);
                }

                if (!string.IsNullOrEmpty(product.Image))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.Image.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                product.Image = "/Products/" + uniqueFileName;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to upload image", error = ex.Message });
            }
        }

        if (string.IsNullOrWhiteSpace(productDto.Name))
            return BadRequest(new { message = "Product name is required." });

        if (productDto.Price <= 0)
            return BadRequest(new { message = "Product price must be greater than 0." });

        product.Name = productDto.Name;
        product.Description = productDto.Description;
        product.Price = productDto.Price;
        product.Quantity = productDto.Quantity;
        product.BrandID = productDto.BrandID;
        product.Reviews = productDto.Reviews;
        product.OrderDetails = productDto.OrderDetails;

        _Product.Update(product);
        await _Product.SaveChanges();

        return Ok(new { message = "Product updated successfully." });
    }


    [HttpDelete]
    public async Task<IActionResult> DeleteProduct(int ID)
    {
        var Product = await _Product.GetById(ID);

        if (Product != null)
        {
            if (!string.IsNullOrEmpty(Product.Image))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Product.Image.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            _Product.Delete(Product);
            await _Product.SaveChanges();
            return Ok(new { message = "Product deleted" });
        }

        return BadRequest(new { message = "Product Doesn't exist" });
    }
}
