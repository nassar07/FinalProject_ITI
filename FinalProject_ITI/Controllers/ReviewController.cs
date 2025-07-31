using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet("{brandId}/reviews")]
        public async Task<IActionResult> GetReviewsForBrand(int brandId)
        {
            var reviews = await _Review.GetQuery()
                .Where(r => r.Product.BrandID == brandId)
                .Include(r => r.Product)
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetReviewById(int ID)
        {
            var Res = await _Review.GetById(ID);

            if (Res == null) BadRequest(new { message = "Review Doesn't exist" });

            return Ok(Res);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddReview(ReviewDTO reviewDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var review = new Review
            {
                UserID = reviewDto.UserID,
                Comment = reviewDto.Comment,
                Rating = reviewDto.Rating,
                CreatedAt = reviewDto.CreatedAt,
                ProductID = reviewDto.ProductID
            };

            await _Review.Add(review);
            await _Review.SaveChanges();

            return Ok(new { message = "Review has been submitted" });
        }


        [HttpPut("update")]
        public async Task<IActionResult> UpdateReview(ReviewDTO Review)
        {
            var Res = await _Review.GetById(Review.Id);

            if (Res == null) BadRequest(new { message = "Review Doesn't exist" });

            Res.Rating = Review.Rating;
            Res.Comment = Review.Comment;
            Res.CreatedAt = Review.CreatedAt;
            Res.UserID = Review.UserID;
            Res.ProductID = Review.ProductID;

            _Review.Update(Res);
            await _Review.SaveChanges();
            return Ok(new { message = "Review Updated" });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteReview(int ID)
        {
            var Review = await _Review.GetById(ID);

            if (Review != null)
            {
                _Review.Delete(Review);
                await _Review.SaveChanges();
                return Ok(new { message = "Review deleted" });
            }

            return BadRequest(new { message = "Review Doesn't exist" });
        }
    }
}
