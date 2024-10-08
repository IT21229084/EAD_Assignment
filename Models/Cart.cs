using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerceAPI.Models
{
    [BsonIgnoreExtraElements]
    public class CartItem
    {
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
    }

    [BsonIgnoreExtraElements]
    public class Cart
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("customerId")]
        public string CustomerId { get; set; } = string.Empty;

        [BsonElement("items")]
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
