using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ECommerceAPI.Models;
using ECommerceAPI.Data;

namespace ECommerceAPI.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _productCollection;

        // Constructor that injects the DatabaseSettings
        public ProductService(IOptions<DatabaseSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.Connection);
            var mongoDb = mongoClient.GetDatabase("EAD");
            _productCollection = mongoDb.GetCollection<Product>("Product");
        }

        // Get all products
        public async Task<List<Product>> GetAsync() =>
            await _productCollection.Find(_ => true).ToListAsync();

        // Get a single product by Id
        public async Task<Product?> GetAsync(string id) =>
            await _productCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        // Create a new product
        public async Task CreateAsync(Product newProduct) =>
            await _productCollection.InsertOneAsync(newProduct);

        // Update an existing product
        public async Task UpdateAsync(string id, Product updatedProduct)
        {
            // Retrieve the existing product document
            var existingProduct = await GetAsync(id);
            if (existingProduct == null)
            {
                throw new Exception("Product not found.");
            }

            // Build the update definition with only the fields that are not null or empty
            var updateDefinition = Builders<Product>.Update
                .Set(p => p.Name, !string.IsNullOrEmpty(updatedProduct.Name) ? updatedProduct.Name : existingProduct.Name)
                .Set(p => p.Description, !string.IsNullOrEmpty(updatedProduct.Description) ? updatedProduct.Description : existingProduct.Description)
                .Set(p => p.Category, !string.IsNullOrEmpty(updatedProduct.Category) ? updatedProduct.Category : existingProduct.Category)
                .Set(p => p.Price, updatedProduct.Price > 0 ? updatedProduct.Price : existingProduct.Price)
                .Set(p => p.StockQuantity, updatedProduct.StockQuantity > 0 ? updatedProduct.StockQuantity : existingProduct.StockQuantity)
                .Set(p => p.IsActive, updatedProduct.IsActive);

            // Execute the update operation
            await _productCollection.UpdateOneAsync(p => p.Id == id, updateDefinition);
        }

        public async Task<List<Product>> GetProductsByVendorIdAsync(string vendorId)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.VendorId, vendorId);
            return await _productCollection.Find(filter).ToListAsync();
        }


        // Delete a product by Id
        public async Task RemoveAsync(string id) =>
            await _productCollection.DeleteOneAsync(x => x.Id == id);

        // Activate or deactivate a product listing
        public async Task UpdateProductStatusAsync(string id)
        {
            var product = await _productCollection.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            // Toggle the active status
            product.IsActive = !product.IsActive;

            var update = Builders<Product>.Update.Set(p => p.IsActive, product.IsActive);
            await _productCollection.UpdateOneAsync(p => p.Id == id, update);
        }


        // Update stock quantity
        public async Task UpdateStockAsync(string id, int stockQuantity)
        {
            var update = Builders<Product>.Update.Set(p => p.StockQuantity, stockQuantity);
            await _productCollection.UpdateOneAsync(p => p.Id == id, update);
        }

        // Find products by category (for filtering products by categories)
        public async Task<List<Product>> GetByCategoryAsync(string category) =>
            await _productCollection.Find(x => x.Category == category).ToListAsync();

        // Get low-stock products for inventory alerts
        public async Task<List<Product>> GetLowStockProductsAsync(int threshold) =>
            await _productCollection.Find(x => x.StockQuantity < threshold).ToListAsync();
    }
}
