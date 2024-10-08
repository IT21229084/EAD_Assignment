using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ECommerceAPI.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("fullName")]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("role")]
        public string Role { get; set; } = "Customer";  // Roles: Admin, Customer, Vendor, etc.

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        // Vendor-specific fields
        [BsonElement("vendorProfile")]
        public VendorProfile? VendorProfile { get; set; }

        // Method to add a ranking to the vendor profile
        public void AddRanking(int newRanking)
        {
            if (VendorProfile == null)
                VendorProfile = new VendorProfile(); // Initialize if null

            if (newRanking < 1 || newRanking > 5)
                throw new ArgumentOutOfRangeException("Ranking must be between 1 and 5.");

            VendorProfile.AddRanking(newRanking);
        }

        // Method to add a comment to the vendor profile
        public void AddComment(string customerId, string comment)
        {
            if (VendorProfile == null)
                VendorProfile = new VendorProfile(); // Initialize if null

            VendorProfile.AddComment(new VendorComment
            {
                CustomerId = customerId,
                Comment = comment,
                Date = DateTime.Now
            });
        }
    }

    public class VendorProfile
    {
        [BsonElement("businessName")]
        public string BusinessName { get; set; } = string.Empty;

        [BsonElement("rankings")]
        public List<int> Rankings { get; set; } = new List<int>();

        [BsonElement("averageRanking")]
        public double AverageRanking { get; set; } = 0;

        [BsonElement("comments")]
        public List<VendorComment> Comments { get; set; } = new List<VendorComment>();

        // Method to add a ranking and recalculate the average
        public void AddRanking(int newRanking)
        {
            Rankings.Add(newRanking);
            AverageRanking = Rankings.Count > 0
                ? Rankings.Average()
                : 0;
        }
        public void AddComment(VendorComment comment)
        {
            Comments.Add(comment);
        }

        // Method to delete a comment by customer ID
        public void DeleteComment(string customerId)
        {
            Comments.RemoveAll(c => c.CustomerId == customerId);
        }


    }

    public class VendorComment
    {
        [BsonElement("customerId")]
        public string CustomerId { get; set; } = string.Empty;

        [BsonElement("comment")]
        public string Comment { get; set; } = string.Empty;

        [BsonElement("date")]
        public DateTime Date { get; set; } = DateTime.Now;

    }
}
