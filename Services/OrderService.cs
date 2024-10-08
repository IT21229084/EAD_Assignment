using MongoDB.Driver;
using ECommerceAPI.Models;
using Microsoft.Extensions.Options;
using ECommerceAPI.Data;

namespace ECommerceAPI.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _orderCollection;

        public OrderService(IOptions<DatabaseSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.Connection);
            var mongoDb = mongoClient.GetDatabase("EAD");
            _orderCollection = mongoDb.GetCollection<Order>("Order");
        }

        // Get all orders
        public async Task<List<Order>> GetAsync() =>
            await _orderCollection.Find(_ => true).ToListAsync();

        // Get order by ID
        public async Task<Order> GetAsync(string id) =>
            await _orderCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        // Create a new order
        //public async Task CreateAsync(Order newOrder) =>
        //    await _orderCollection.InsertOneAsync(newOrder);

        public async Task CreateAsync(Order newOrder)
        {
            if (newOrder.OrderItems == null || !newOrder.OrderItems.Any())
            {
                throw new InvalidOperationException("Order must contain at least one item.");
            }

            // Calculate total price based on order items
            newOrder.TotalPrice = newOrder.OrderItems.Sum(item => item.Price * item.Quantity);
            newOrder.OrderDate = DateTime.UtcNow;

            await _orderCollection.InsertOneAsync(newOrder);
        }


        // Update specific fields of an order (Partial Update)
        public async Task UpdateAsync(string id, Order updatedOrder)
        {
            // Retrieve the existing order from the database.
            var order = await _orderCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

            if (order == null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            // Ensure that the order can be updated only if it’s in the "Processing" status.
            if (order.Status == "Shipped" || order.Status == "Cancelled" || order.Status == "Delivered" || order.IsCancelled ==true)
            {
                throw new InvalidOperationException("Order cannot be updated as it is not in 'Processing' status or is already cancelled.");
            }

            // Update only the fields that are provided in the updatedOrder.
            order.OrderItems = updatedOrder.OrderItems ?? order.OrderItems;
            order.OrderDate = updatedOrder.OrderDate ?? order.OrderDate;
            order.DeliveryDate = updatedOrder.DeliveryDate ?? order.DeliveryDate;
            order.CustomerId = !string.IsNullOrEmpty(updatedOrder.CustomerId) ? updatedOrder.CustomerId : order.CustomerId;
            order.Status = !string.IsNullOrEmpty(updatedOrder.Status) ? updatedOrder.Status : order.Status;

            // Recalculate the total price if order items have been updated.
            if (updatedOrder.OrderItems != null)
            {
                order.TotalPrice = CalculateTotalPrice(order.OrderItems);
            }

            // Save the updated order back to the database.
            await _orderCollection.ReplaceOneAsync(x => x.Id == id, order);
        }

        // Helper method to calculate the total price based on the order items.
        private decimal CalculateTotalPrice(List<OrderItem> orderItems)
        {
            return orderItems.Sum(item => item.Price * item.Quantity);
        }


        // Cancel an order
        public async Task CancelOrderAsync(string id)
        {
            var order = await _orderCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

            // Only allow cancellation if the order is in "Processing" status and not already cancelled
            if (order == null || order.Status == "Shipped" || order.Status == "Cancelled" || order.Status == "Delivered" || order.IsCancelled == true)
            {
                throw new InvalidOperationException("Order cannot be cancelled as it is not in 'Processing' status or is already cancelled.");
            }

            var update = Builders<Order>.Update
                .Set(o => o.IsCancelled, true)
                .Set(o => o.Status, "Cancelled");

            await _orderCollection.UpdateOneAsync(x => x.Id == id, update);
        }

        // Remove order
        public async Task RemoveAsync(string id) =>
            await _orderCollection.DeleteOneAsync(x => x.Id == id);
    }
}
