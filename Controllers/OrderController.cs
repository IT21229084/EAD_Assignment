using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.Models;
using ECommerceAPI.Services;
using System.Security.Claims;

namespace ECommerceAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/order
        [HttpGet]
        public async Task<List<Order>> Get() => await _orderService.GetAsync();

        // GET: api/order/{id}
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Order>> Get(string id)
        {
            var order = await _orderService.GetAsync(id);
            if (order == null)
            {
                return NotFound($"Order with id {id} not found.");
            }
            return order;
        }

        // POST: api/order
        [HttpPost]
        // POST api/order
        [HttpPost]
        public async Task<ActionResult<Order>> Post(Order newOrder)
        {
            if (newOrder.OrderItems == null || !newOrder.OrderItems.Any())
            {
                return BadRequest("Order must contain at least one item.");
            }

            await _orderService.CreateAsync(newOrder);
            return CreatedAtAction(nameof(Get), new { id = newOrder.Id }, newOrder);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetOrdersByCustomerId(string customerId)
        {
            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);

            if (orders == null || !orders.Any())
            {
                return NotFound($"No orders found for customer ID {customerId}.");
            }

            return Ok(orders);
        }


        // PUT: api/order/{id}
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Put(string id, Order updatedOrder)
        {
            try
            {
                await _orderService.UpdateAsync(id, updatedOrder);
                return Ok("Order updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:length(24)}/cancel")]
        public async Task<IActionResult> CancelOrder(string id, [FromBody] string cancellationReason)
        {
            try
            {
                await _orderService.CancelOrderAsync(id, cancellationReason);
                return Ok("Order cancelled successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

      
        [HttpPut("{orderId}/deliver")]
        public async Task<IActionResult> MarkAsDelivered(string orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get user ID from claims
            try
            {
                await _orderService.MarkOrderAsDeliveredAsync(orderId, userId);
                return Ok("Order marked as delivered successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        // DELETE: api/order/{id}
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var order = await _orderService.GetAsync(id);
            if (order == null)
            {
                return NotFound($"Order with id {id} not found.");
            }

            await _orderService.RemoveAsync(id);
            return Ok("Order deleted successfully.");
        }
    }
}
