using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.Models;
using ECommerceAPI.Services;

namespace ECommerceAPI.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: api/notifications/vendor/{vendorId}
        [HttpGet("vendor/{vendorId:length(24)}")]
        public async Task<ActionResult<List<Notification>>> GetByVendor(string vendorId)
        {
            var notifications = await _notificationService.GetNotificationsByVendorAsync(vendorId);
            return Ok(notifications);
        }

        // POST: api/notifications
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Notification notification)
        {
            await _notificationService.CreateNotificationAsync(notification);
            return Ok("Notification created successfully.");
        }

        // PUT: api/notifications/{id}/read
        [HttpPut("{id:length(24)}/read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok("Notification marked as read.");
        }

        // DELETE: api/notifications/{id}
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _notificationService.DeleteNotificationAsync(id);
            return Ok("Notification deleted successfully.");
        }
    }
}
