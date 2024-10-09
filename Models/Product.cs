using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerceAPI.Models
{
    [BsonIgnoreExtraElements]
    public class Product
    {
        // Unique Product ID
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        // The name of the product
        [BsonElement("name")]
        public string? Name { get; set; }

        // Product description
        [BsonElement("description")]
        public string? Description { get; set; }

        // Price of the product
        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]  // To store decimal values accurately in MongoDB
        public decimal Price { get; set; }

        // Number of items in stock
        [BsonElement("stock_quantity")]
        public int StockQuantity { get; set; }

        // Vendor who created the product
        [BsonElement("vendor_id")]
        [BsonRepresentation(BsonType.ObjectId)]  // Reference to the Vendor who manages the product
        public string? VendorId { get; set; } = string.Empty;

        // Category of the product
        [BsonElement("category")]
        public string? Category { get; set; }

        [BsonElement("ImageUrl")]
        public string? ImageUrl { get; set; } // URL for the product image

        // Whether the product is active and available for customers
        [BsonElement("is_active")]
        public bool IsActive { get; set; } = true;

        // Rating given by customers, initially 0
        [BsonElement("rating")]
        public double? Rating { get; set; } = 0.0;

        // Date when the product was created
        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Date when the product was last updated
        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Constructor to update the CreatedAt and UpdatedAt timestamps
        public Product()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
