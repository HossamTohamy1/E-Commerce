using E_Commers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E_Commers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly ECommerceDbContext _context;
        public ReviewController(ECommerceDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Buyer")]
        [HttpPost("add-review")]
        public async Task<IActionResult> AddReview([FromBody] ReviewRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "You must be logged in first." });
                }

                if (request.Rating < 1 || request.Rating > 5)
                {
                    return BadRequest(new { Message = "The rating must be between 1 and 5." });
                }

                var review = new Review
                {
                    ProductId = request.ProductId,
                    UserId = userId,
                    Comment = request.Comment,
                    Rating = request.Rating,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Review added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while adding the review.", Error = ex.Message });
            }
        }

        public class ReviewRequest
        {
            public int ProductId { get; set; }
            public string Comment { get; set; }
            public int Rating { get; set; }
        }
    }
}
