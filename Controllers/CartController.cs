using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.Models;
using ECommerceAPI.Services;

namespace ECommerceAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        // GET: api/cart/{customerId}
        [HttpGet("{customerId:length(24)}")]
        public async Task<ActionResult<Cart>> Get(string customerId)
        {
            var cart = await _cartService.GetCartByCustomerIdAsync(customerId);
            if (cart == null)
                return NotFound("Cart not found.");

            return cart;
        }

        // POST: api/cart/{customerId}/add
        [HttpPost("{customerId:length(24)}/add")]
        public async Task<IActionResult> AddItem(string customerId, [FromBody] CartItem item)
        {
            await _cartService.AddItemToCartAsync(customerId, item);
            return Ok("Item added to cart successfully.");
        }

        // POST: api/cart/{customerId}/clear
        [HttpPost("{customerId:length(24)}/clear")]
        public async Task<IActionResult> ClearCart(string customerId)
        {
            await _cartService.ClearCartAsync(customerId);
            return Ok("Cart cleared successfully.");
        }
    }
}
