using MongoDB.Driver;
using ECommerceAPI.Models;
using Microsoft.Extensions.Options;
using ECommerceAPI.Data;

namespace ECommerceAPI.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly NotificationService _notificationService;
        public OrderService(IOptions<DatabaseSettings> settings , NotificationService notificationService)
        {
            var mongoClient = new MongoClient(settings.Value.Connection);
            var mongoDb = mongoClient.GetDatabase("EAD");
            _orderCollection = mongoDb.GetCollection<Order>("Order");
            _notificationService = notificationService;
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

        // Get all orders for a specific customer by CustomerId
        public async Task<List<Order>> GetOrdersByCustomerIdAsync(string customerId)
        {
            return await _orderCollection.Find(order => order.CustomerId == customerId).ToListAsync();
        }
        public async Task<bool> HasPendingOrdersForProductAsync(string productId)
        {
            // Build a filter to check for orders that have a status of "Processing"
            // and contain at least one order item with the specified productId.
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.Eq(o => o.Status, "Processing"), // Check order status
                Builders<Order>.Filter.ElemMatch(o => o.OrderItems, item => item.ProductId == productId) // Check if any item has the given productId
            );

            // Count documents that match the filter
            var count = await _orderCollection.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task UpdateAsync(string id, Order updatedOrder)
        {
            // Retrieve the existing order from the database.
            var order = await _orderCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

            if (order == null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            // Ensure that the order can be updated only if it’s in the "Processing" status.
            if (order.Status == "Shipped" || order.Status == "Cancelled" || order.Status == "Delivered" || order.IsCancelled == true)
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
        public async Task CancelOrderAsync(string id, string note)
        {
            // Retrieve the existing order by its ID.
            var order = await _orderCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

            // Only allow cancellation if the order is in "Processing" status and not already cancelled.
            if (order == null || order.Status == "Shipped" || order.Status == "Cancelled" || order.Status == "Delivered" || order.IsCancelled == true)
            {
                throw new InvalidOperationException("Order cannot be cancelled as it is not in 'Processing' status or is already cancelled.");
            }

            // Create the update definition to set cancellation details.
            var update = Builders<Order>.Update
                .Set(o => o.IsCancelled, true)
                .Set(o => o.Status, "Cancelled")
                .Set(o => o.CancellationNote, note) // Store the cancellation note.
                .Set(o => o.CancellationDate, DateTime.UtcNow); // Optional: Record the date of cancellation.

            // Apply the update to the order in the database.
            await _orderCollection.UpdateOneAsync(x => x.Id == id, update);

            // Notify the customer about the order cancellation.
            await _notificationService.SendNotificationAsync(order.CustomerId,
                $"Your order {id} has been cancelled. Note: {note}");
        }

        //public async Task MarkOrderAsDeliveredAsync(string orderId, string userId, string role)
        //{
        //    // Retrieve the order from the database.
        //    var order = await GetAsync(orderId);
        //    if (order == null)
        //    {
        //        throw new InvalidOperationException("Order not found.");
        //    }

        //    // Update delivery status based on the user's role.
        //    //if (role == "CSR" || role == "Admin")
        //    //{
        //    //    // CSR or Admin can mark the entire order as delivered.
        //    //    order.Status = "Delivered";
        //    //    order.DeliveryDate = DateTime.UtcNow; // Record the delivery date.
        //    //}
        //    //else if (role == "Vendor")
        //    //{
        //        // Vendor can mark their item as delivered.
        //        // Check if the vendor's item is in the order
        //        var itemDelivered = false;
        //        foreach (var item in order.OrderItems)
        //        {
        //            if (item.VendorId == userId)
        //            {
        //                itemDelivered = true; // Indicate that this vendor's item was marked as delivered
        //                                      // If this vendor's item is the last item, mark the order as delivered.
        //                if (order.OrderItems.All(i => i.VendorId != userId || (i.VendorId == userId && item.IsDelivered)))
        //                {
        //                    order.Status = "Delivered";
        //                    order.DeliveryDate = DateTime.UtcNow; // Set delivery date if all items from the vendor are delivered.
        //                }
        //                else
        //                {
        //                    order.Status = "Partially Delivered"; // Some items are delivered, but not all.
        //                }
        //            }
        //        }

        //        if (!itemDelivered)
        //        {
        //            throw new InvalidOperationException("This vendor has no items in the order.");
        //        }
        //    //}
        //    //else
        //    //{
        //    //    throw new UnauthorizedAccessException("You do not have permission to update this order.");
        //    //}

        //    // Update the last modified date.


        //    // Save the updated order back to the database.
        //    await _orderCollection.ReplaceOneAsync(o => o.Id == orderId, order);

        //    // Prepare the notification message based on the updated order status.
        //    var notificationMessage = order.Status == "Delivered"
        //        ? $"Your order {orderId} has been fully delivered."
        //        : $"Part of your order {orderId} has been delivered by one of the vendors.";

        //    // Notify the customer about the delivery status.
        //    await _notificationService.SendNotificationAsync(order.CustomerId, notificationMessage);
        //}




        // Remove order


        public async Task MarkOrderAsDeliveredAsync(string orderId, string userId)
        {
            // Retrieve the order from the database.
            var order = await GetAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            // Mark all items in the order as delivered.
            foreach (var item in order.OrderItems)
            {
                // Assuming you want to set IsDelivered to true for each item.
                item.IsDelivered = true;
                item.DeliveredDate = DateTime.UtcNow;
            }

            // Update the order status and delivery date.
            order.Status = "Delivered";
            order.DeliveryDate = DateTime.UtcNow; // Record the delivery date.

            // Update the last modified date (if needed).
           

            // Save the updated order back to the database.
            await _orderCollection.ReplaceOneAsync(o => o.Id == orderId, order);

            // Prepare the notification message based on the updated order status.
            var notificationMessage = $"Your order {orderId} has been fully delivered.";

            // Notify the customer about the delivery status.
            await _notificationService.SendNotificationAsync(order.CustomerId, notificationMessage);
        }

        public async Task RemoveAsync(string id) =>
            await _orderCollection.DeleteOneAsync(x => x.Id == id);
    }
}
