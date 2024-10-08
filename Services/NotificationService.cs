using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ECommerceAPI.Models;
using ECommerceAPI.Data;

namespace ECommerceAPI.Services
{
    public class NotificationService
    {
        private readonly IMongoCollection<Notification> _notificationCollection;

        public NotificationService(IOptions<DatabaseSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.Connection);
            var mongoDb = mongoClient.GetDatabase("EAD");
            _notificationCollection = mongoDb.GetCollection<Notification>("Notifications");
        }

        // Create a new notification
        public async Task CreateNotificationAsync(Notification notification) =>
            await _notificationCollection.InsertOneAsync(notification);

        // Get notifications for a specific vendor
        public async Task<List<Notification>> GetNotificationsByVendorAsync(string vendorId) =>
            await _notificationCollection.Find(n => n.VendorId == vendorId).ToListAsync();

        // Mark a notification as read
        public async Task MarkAsReadAsync(string id)
        {
            var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
            await _notificationCollection.UpdateOneAsync(n => n.Id == id, update);
        }
        public async Task SendNotificationAsync(string userId, string message)
        {
            // Here you would integrate with an external service to send the notification
            // (e.g., an email service, push notification service, SMS gateway, etc.)

            // Simulating notification sending with a delay
            await Task.Delay(500);
            Console.WriteLine($"Notification sent to User {userId}: {message}");
        }

        // Delete a notification
        public async Task DeleteNotificationAsync(string id) =>
            await _notificationCollection.DeleteOneAsync(n => n.Id == id);
    }
}
