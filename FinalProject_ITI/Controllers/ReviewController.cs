using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject_ITI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IRepository<Review> _Review;
        public ReviewController(IRepository<Review> Review)
        {
            _Review = Review;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllReview()
        {
            var Review = await _Review.GetAll();
            return Ok(Review);
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetReviewById(int ID)
        {
            var Res = await _Review.GetById(ID);

            if (Res == null) BadRequest("Review Doesn't exist");

            return Ok(Res);
        }

        [HttpPost("Review")]
        public async Task<IActionResult> AddReview(Review Review)
        {
            if (ModelState.IsValid)
            {

                await _Review.Add(Review);
                await _Review.SaveChanges();

                return Ok("Review has been submitted");
            }
            return BadRequest(ModelState);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateReview(ReviewDTO Review)
        {
            var Res = await _Review.GetById(Review.Id);

            if (Res == null) BadRequest("Review Doesn't exist");

            Res.Rating = Review.Rating;
            Res.Comment = Review.Comment;
            Res.CreatedAt = Review.CreatedAt;
            Res.UserID = Review.UserID;
            Res.ProductID = Review.ProductID;

            _Review.Update(Res);
            await _Review.SaveChanges();
            return Ok("Review Updated");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteReview(int ID)
        {
            var Review = await _Review.GetById(ID);

            if (Review != null)
            {
                _Review.Delete(Review);
                await _Review.SaveChanges();
                return Ok("Review deleted");
            }

            return BadRequest("Review Doesn't exist");
        }
    }
}
