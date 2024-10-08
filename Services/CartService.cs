using MongoDB.Driver;
using ECommerceAPI.Models;
using Microsoft.Extensions.Options;
using ECommerceAPI.Data;

namespace ECommerceAPI.Services
{
    public class CartService
    {
        private readonly IMongoCollection<Cart> _cartCollection;

        public CartService(IOptions<DatabaseSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.Connection);
            var mongoDb = mongoClient.GetDatabase("EAD");
            _cartCollection = mongoDb.GetCollection<Cart>("Carts");
        }

        // Get cart by customerId
        public async Task<Cart> GetCartByCustomerIdAsync(string customerId) =>
            await _cartCollection.Find(c => c.CustomerId == customerId).FirstOrDefaultAsync();

        // Add item to cart
        public async Task AddItemToCartAsync(string customerId, CartItem newItem)
        {
            var cart = await GetCartByCustomerIdAsync(customerId);
            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    Items = new List<CartItem> { newItem }
                };
                await _cartCollection.InsertOneAsync(cart);
            }
            else
            {
                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == newItem.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += newItem.Quantity;
                }
                else
                {
                    cart.Items.Add(newItem);
                }
                await _cartCollection.ReplaceOneAsync(c => c.Id == cart.Id, cart);
            }
        }

        // Clear the cart
        public async Task ClearCartAsync(string customerId)
        {
            var cart = await GetCartByCustomerIdAsync(customerId);
            if (cart != null)
            {
                cart.Items.Clear();
                await _cartCollection.ReplaceOneAsync(c => c.Id == cart.Id, cart);
            }
        }
    }
}
