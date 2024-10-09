using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerceAPI.Models
{
    [BsonIgnoreExtraElements]
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("customerId")]
        public string CustomerId { get; set; } = string.Empty;  // Link to customer

        [BsonElement("orderItems")]
        public List<OrderItem> OrderItems { get; set; } = new(); // List of products ordered

        [BsonElement("totalPrice")]
        public decimal TotalPrice { get; set; } 

        [BsonElement("status")]
        public string? Status { get; set; } = "Processing"; // e.g., Processing, Shipped, Delivered, Cancelled

        [BsonElement("cancellationNote")]
        public string? CancellationNote { get; set; }

        [BsonElement("CancellationDate")]
        public DateTime? CancellationDate { get; set; }

        [BsonElement("orderDate")]
        public DateTime? OrderDate { get; set; } = DateTime.UtcNow;

        [BsonElement("deliveryDate")]
        public DateTime? DeliveryDate { get; set; }

        [BsonElement("cancelled")]
        public bool IsCancelled { get; set; } = false;
    }

    public class OrderItem
    {
        [BsonElement("productId")]
        public string ProductId { get; set; } = string.Empty;  // Link to Product

        [BsonElement("productName")]
        public string ProductName { get; set; } = string.Empty;

        [BsonElement("vendorId")]
        public string VendorId { get; set; } = string.Empty;

        [BsonElement("quantity")]
        public int Quantity { get; set; } = 1;

        [BsonElement("price")]
        public decimal Price { get; set; } = 0M;

        [BsonElement("IsDelivered")]
        public bool IsDelivered { get; set; } = false;

        [BsonElement("DeliveredDate")]
        public DateTime? DeliveredDate { get; set; }
    }
}
