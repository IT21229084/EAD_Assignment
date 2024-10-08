using MongoDB.Driver;
using ECommerceAPI.Models;
using Microsoft.Extensions.Options;
using ECommerceAPI.Data;

namespace ECommerceAPI.Services
{
    public class VendorService
    {
        private readonly IMongoCollection<User> _userCollection;

        public VendorService(IOptions<DatabaseSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.Connection);
            var mongoDb = mongoClient.GetDatabase("EAD");
            _userCollection = mongoDb.GetCollection<User>("user"); // Access the User collection
        }

        // Get all vendors (users with VendorProfile)
        public async Task<List<User>> GetAllVendorsAsync()
        {
            return await _userCollection.Find(u => u.VendorProfile != null).ToListAsync();
        }

        // Get a vendor by ID
        public async Task<User> GetVendorByIdAsync(string userId)
        {
            return await _userCollection.Find(u => u.Id == userId && u.VendorProfile != null).FirstOrDefaultAsync();
        }

        // Add ranking and calculate the average
        public async Task AddRankingAsync(string userId, int ranking)
        {
            var user = await GetVendorByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Vendor not found.");

            // Add ranking using the method in the User model
            user.AddRanking(ranking);

            // Update the user in the database
            await _userCollection.ReplaceOneAsync(u => u.Id == userId, user);
        }

        // Add a comment for the vendor
        public async Task AddCommentAsync(string userId, string customerId, string comment)
        {
            var user = await GetVendorByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Vendor not found.");

            // Add comment using the method in the User model
            user.AddComment(customerId, comment);

            // Update the user in the database
            await _userCollection.ReplaceOneAsync(u => u.Id == userId, user);
        }

        // Delete a comment by customer ID
        public async Task DeleteCommentAsync(string userId, string customerId)
        {
            var user = await GetVendorByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Vendor not found.");

            // Delete comment using the method in the VendorProfile
            user.VendorProfile.DeleteComment(customerId);

            // Update the user in the database
            await _userCollection.ReplaceOneAsync(u => u.Id == userId, user);
        }

        // Get vendor with comments and average ranking
        public async Task<User> GetVendorWithFeedbackAsync(string userId)
        {
            return await GetVendorByIdAsync(userId);
        }
    }
}
