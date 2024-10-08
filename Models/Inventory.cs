using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerceAPI.Models
{
    [BsonIgnoreExtraElements]
    public class Inventory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("productId")]
        public string ProductId { get; set; } = string.Empty;

        [BsonElement("vendorId")]
        public string VendorId { get; set; } = string.Empty;

        [BsonElement("stockQuantity")]
        public int StockQuantity { get; set; } = 0;

        [BsonElement("isLowStockAlertEnabled")]
        public bool IsLowStockAlertEnabled { get; set; } = true;

        [BsonElement("lowStockThreshold")]
        public int LowStockThreshold { get; set; } = 10;

        [BsonElement("lastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
