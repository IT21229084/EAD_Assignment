using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ECommerceAPI.Models;
using ECommerceAPI.Data;

namespace ECommerceAPI.Services
{
    public class InventoryService
    {
        private readonly IMongoCollection<Inventory> _inventoryCollection;
        private readonly OrderService _orderService; // Assuming there is an OrderService to manage orders
        private readonly NotificationService _notificationService; // For sending notifications to vendors

        public InventoryService(IOptions<DatabaseSettings> settings, OrderService orderService, NotificationService notificationService)
        {
            var mongoClient = new MongoClient(settings.Value.Connection);
            var mongoDb = mongoClient.GetDatabase("EAD");
            _inventoryCollection = mongoDb.GetCollection<Inventory>("Inventory");
            _orderService = orderService;
            _notificationService = notificationService;
        }

        // Get all inventory records
        public async Task<List<Inventory>> GetAsync() =>
            await _inventoryCollection.Find(_ => true).ToListAsync();

        // Get inventory by Id
        public async Task<Inventory?> GetAsync(string id) =>
            await _inventoryCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        // Create a new inventory entry
        public async Task CreateAsync(Inventory newInventory)
        {
            await _inventoryCollection.InsertOneAsync(newInventory);
            // Automatically adjust stock levels based on creation.
            await NotifyIfLowStock(newInventory);
        }

        // Update an inventory entry by Id
        public async Task UpdateAsync(string id, Inventory updatedInventory)
        {
            await _inventoryCollection.ReplaceOneAsync(x => x.Id == id, updatedInventory);
            await NotifyIfLowStock(updatedInventory);
        }

        // Remove an inventory entry with pending order check
        public async Task RemoveAsync(string id)
        {
            var inventory = await GetAsync(id);
            if (inventory == null) throw new InvalidOperationException("Inventory not found.");

            // Check if there are any pending orders containing this product
            bool hasPendingOrders = await _orderService.HasPendingOrdersForProductAsync(inventory.ProductId);
            if (hasPendingOrders)
                throw new InvalidOperationException("Cannot remove inventory with pending orders.");

            await _inventoryCollection.DeleteOneAsync(x => x.Id == id);
        }

        // Update stock quantity
        public async Task UpdateStockAsync(string id, int stockQuantity)
        {
            var update = Builders<Inventory>.Update
                .Set(p => p.StockQuantity, stockQuantity)
                .Set(p => p.LastUpdated, DateTime.UtcNow);

            await _inventoryCollection.UpdateOneAsync(p => p.Id == id, update);
            var updatedInventory = await GetAsync(id);
            if (updatedInventory != null)
            {
                await NotifyIfLowStock(updatedInventory);
            }
        }

        // Get low-stock inventory for alerting
        public async Task<List<Inventory>> GetLowStockItemsAsync() =>
            await _inventoryCollection.Find(x => x.StockQuantity < x.LowStockThreshold && x.IsLowStockAlertEnabled).ToListAsync();

        // Enable or disable low stock alert
        public async Task UpdateLowStockAlertAsync(string id, bool enableAlert)
        {
            var update = Builders<Inventory>.Update.Set(p => p.IsLowStockAlertEnabled, enableAlert);
            await _inventoryCollection.UpdateOneAsync(p => p.Id == id, update);
        }

        // Notify vendor if stock is low
        private async Task NotifyIfLowStock(Inventory inventory)
        {
            if (inventory.IsLowStockAlertEnabled && inventory.StockQuantity < inventory.LowStockThreshold)
            {
                var notification = new Notification
                {
                    VendorId = inventory.VendorId,
                    Message = $"Stock is low for product {inventory.ProductId}. Only {inventory.StockQuantity} items left.",
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationService.CreateNotificationAsync(notification);
            }
        }

    }
}
