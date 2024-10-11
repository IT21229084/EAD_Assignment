using MongoDB.Driver;
using Microsoft.Extensions.Options;
using ECommerceAPI.Models;
using ECommerceAPI.Data;
using BCrypt.Net;

namespace ECommerceAPI.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;

        public UserService(IOptions<DatabaseSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.Connection);
            var mongoDb = mongoClient.GetDatabase("EAD");
            _userCollection = mongoDb.GetCollection<User>("user");

            //var database = mongoClient.GetDatabase(settings.DatabaseName);
            //_vehicle = database.GetCollection<Vehicle>(settings.VehicleCollectionName);
        }

        // Get all users
        public async Task<List<User>> GetAsync() =>
            await _userCollection.Find(_ => true).ToListAsync();

        // Get a single user by Id
        public async Task<User?> GetAsync(string id) =>
            await _userCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        // Get a single user by Username
        public async Task<User?> GetByUsernameAsync(string username) =>
            await _userCollection.Find(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();
        public async Task<User?> GetByeEmailAsync(string Email) =>
           await _userCollection.Find(x => x.Email.ToLower() == Email.ToLower()).FirstOrDefaultAsync();


        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        // Create a new user
        public async Task CreateAsync(User newUser)
        {
            newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.PasswordHash);
            await _userCollection.InsertOneAsync(newUser);
        }

        // Update an existing user
        public async Task UpdateAsync(string id, User updatedUser)
        {
            // Retrieve the existing user document
            var existingUser = await GetAsync(id);
            if (existingUser == null)
            {
                throw new Exception("User not found.");
            }

            // Preserve non-null fields from `updatedUser` or use existing fields if null
            var updateDefinition = Builders<User>.Update
                .Set(u => u.Username, !string.IsNullOrEmpty(updatedUser.Username) ? updatedUser.Username : existingUser.Username)
                .Set(u => u.Email, !string.IsNullOrEmpty(updatedUser.Email) ? updatedUser.Email : existingUser.Email)
                .Set(u => u.FullName, !string.IsNullOrEmpty(updatedUser.FullName) ? updatedUser.FullName : existingUser.FullName)
                .Set(u => u.Role, !string.IsNullOrEmpty(updatedUser.Role) ? updatedUser.Role : existingUser.Role)
                .Set(u => u.PasswordHash, !string.IsNullOrEmpty(updatedUser.PasswordHash) ? BCrypt.Net.BCrypt.HashPassword(updatedUser.PasswordHash) : existingUser.PasswordHash);

            await _userCollection.UpdateOneAsync(u => u.Id == id, updateDefinition);
        }


        // Delete a user by Id
        public async Task RemoveAsync(string id) =>
            await _userCollection.DeleteOneAsync(x => x.Id == id);

        // Authenticate user by username and password
        public async Task<User?> AuthenticateAsync(string Email, string password)
        {
            var user = await GetByeEmailAsync(Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }
            return user;
        }

        // Change user role
        public async Task UpdateRoleAsync(string id, string role)
        {
            var update = Builders<User>.Update.Set(u => u.Role, role);
            await _userCollection.UpdateOneAsync(u => u.Id == id, update);
        }

        // Activate or Deactivate a user account
        public async Task UpdateStatusAsync(string id, bool isActive)
        {
            var update = Builders<User>.Update.Set(u => u.IsActive, isActive);
            await _userCollection.UpdateOneAsync(u => u.Id == id, update);
        }
    }
}
