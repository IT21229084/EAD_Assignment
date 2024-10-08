using ECommerceAPI.Models;
using ECommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [Route("api/vendors")]
    [ApiController]
    public class VendorController : ControllerBase
    {
        private readonly VendorService _vendorService;

        public VendorController(VendorService vendorService)
        {
            _vendorService = vendorService;
        }

        // GET: api/vendors
        [HttpGet]
        public async Task<List<User>> Get()
        {
            return await _vendorService.GetAllVendorsAsync();
        }

        // GET: api/vendors/{id}
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<User>> Get(string id)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(id);
            if (vendor == null)
                return NotFound($"Vendor with id {id} not found");

            return Ok(vendor); // Return the vendor object wrapped in Ok()
        }

        // POST: api/vendors/rank/{id}
        [HttpPost("rank/{id:length(24)}")]
        public async Task<ActionResult> RankVendor(string id, [FromBody] int ranking)
        {
            if (ranking < 1 || ranking > 5)
                return BadRequest("Ranking should be between 1 and 5");

            try
            {
                await _vendorService.AddRankingAsync(id, ranking);
                return Ok("Ranking added successfully");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message); // Handle case where vendor is not found
            }
        }

        // POST: api/vendors/comment/{id}
        [HttpPost("comment/{id:length(24)}")]
        public async Task<ActionResult> CommentVendor(string id, [FromBody] VendorComment comment)
        {
            if (string.IsNullOrWhiteSpace(comment.Comment))
                return BadRequest("Comment cannot be empty.");

            try
            {
                await _vendorService.AddCommentAsync(id, comment.CustomerId, comment.Comment);
                return Ok("Comment added successfully");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message); // Handle case where vendor is not found
            }
        }

        // DELETE: api/vendors/comment/{vendorId}/{customerId}
        [HttpDelete("comment/{vendorId:length(24)}/{customerId}")]
        public async Task<ActionResult> DeleteComment(string vendorId, string customerId)
        {
            try
            {
                await _vendorService.DeleteCommentAsync(vendorId, customerId);
                return Ok("Comment deleted successfully");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message); // Handle case where vendor is not found
            }
        }

        // GET: api/vendors/feedback/{id}
        [HttpGet("feedback/{id:length(24)}")]
        public async Task<ActionResult<User>> GetVendorFeedback(string id)
        {
            var vendor = await _vendorService.GetVendorWithFeedbackAsync(id);
            if (vendor == null)
                return NotFound($"Vendor with id {id} not found");

            return Ok(vendor); // Return the vendor object wrapped in Ok()
        }
    }
}
