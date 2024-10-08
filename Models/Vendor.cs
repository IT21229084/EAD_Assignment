using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerceAPI.Models
{
    [BsonIgnoreExtraElements]
    public class Vendor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("rankings")]
        public List<int> Rankings { get; set; } = new List<int>();

        [BsonElement("averageRanking")]
        public double AverageRanking { get; set; } = 0;

        [BsonElement("comments")]
        public List<VendorComments> Comments { get; set; } = new List<VendorComments>();
    }

    public class VendorComments
    {
        [BsonElement("customerId")]
        public string CustomerId { get; set; } = string.Empty;

        [BsonElement("comment")]
        public string Comment { get; set; } = string.Empty;

        [BsonElement("date")]
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
