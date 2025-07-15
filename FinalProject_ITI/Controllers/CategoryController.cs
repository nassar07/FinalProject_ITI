using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject_ITI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IRepository<Category> _Category;
        public CategoryController(IRepository<Category> Category)
        {
            _Category = Category;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCategory()
        {
            var Category = _Category.GetAll();
            return Ok(Category);
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetCategoryById(int ID)
        {
            var Res = await _Category.GetById(ID);

            if (Res == null) BadRequest("Category Doesn't exist");

            return Ok(Res);
        }

        [HttpPost("Category")]
        public async Task<IActionResult> AddCategory(Category Category)
        {
            if (ModelState.IsValid)
            {

                await _Category.Add(Category);
                await _Category.SaveChanges();

                return Ok("Category has been submitted");
            }
            return BadRequest(ModelState);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateCategory(CategoryDTO Category)
        {
            var Res = await _Category.GetById(Category.ID);

            if (Res == null) BadRequest("Category Doesn't exist");

            Res.Name = Category.Name;

            _Category.Update(Res);
            await _Category.SaveChanges();
            return Ok("Category Updated");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCategory(int ID)
        {
            var Category = await _Category.GetById(ID);

            if (Category != null)
            {
                _Category.Delete(Category);
                await _Category.SaveChanges();
                return Ok("Category deleted");
            }

            return BadRequest("Category Doesn't exist");
        }
    }
}
