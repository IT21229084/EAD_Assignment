using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ECommerceAPI.Models;
using ECommerceAPI.Data;

namespace ECommerceAPI.Services
{
    public class InventoryService
    {
        private readonly IMongoCollection<Inventory> _inventoryCollection;

        public InventoryService(IOptions<DatabaseSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.Connection);
            var mongoDb = mongoClient.GetDatabase("EAD");
            _inventoryCollection = mongoDb.GetCollection<Inventory>("Inventory");
        }

        // Get all inventory records
        public async Task<List<Inventory>> GetAsync() =>
            await _inventoryCollection.Find(_ => true).ToListAsync();

        // Get inventory by Id
        public async Task<Inventory?> GetAsync(string id) =>
            await _inventoryCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        // Create a new inventory entry
        public async Task CreateAsync(Inventory newInventory) =>
            await _inventoryCollection.InsertOneAsync(newInventory);

        // Update an inventory entry by Id
        public async Task UpdateAsync(string id, Inventory updatedInventory) =>
            await _inventoryCollection.ReplaceOneAsync(x => x.Id == id, updatedInventory);

        // Remove an inventory entry
        public async Task RemoveAsync(string id) =>
            await _inventoryCollection.DeleteOneAsync(x => x.Id == id);

        // Update stock quantity
        public async Task UpdateStockAsync(string id, int stockQuantity)
        {
            var update = Builders<Inventory>.Update
                .Set(p => p.StockQuantity, stockQuantity)
                .Set(p => p.LastUpdated, DateTime.UtcNow);

            await _inventoryCollection.UpdateOneAsync(p => p.Id == id, update);
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
    }
}
