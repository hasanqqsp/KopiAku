using MongoDB.Driver;
using KopiAku.Models;
using KopiAku.DTOs;
using HotChocolate.Authorization;

namespace KopiAku.GraphQL.Users
{
    [ExtendObjectType(typeof(Mutation))]
    public class UserMutations
    {
        [AllowAnonymous]
        public async Task<LoginResponse> LoginAsync(
            LoginInput input,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<User>("users");

            var user = await collection.Find(u => u.Username == input.Username).FirstOrDefaultAsync();
            if (user == null || !BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
            {
                throw new GraphQLException("Invalid username or password.");
            }

            // In a real application, generate a JWT or similar token here
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            return new LoginResponse
            {
                Token = token,
                Id = user.Id,
                Name = user.Name,
                Username = user.Username,
                Email = user.Email
            };
        }

        [AllowAnonymous]
        public async Task<RegisterResponse> RegisterAsync(
            RegisterInput input,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<User>("users");

            var existingUser = await collection.Find(u => u.Username == input.Username || u.Email == input.Email || u.Name == input.Name).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                throw new GraphQLException("Username, email, or name already exists.");
            }

            var newUser = new User
            {
                Name = input.Name,
                Username = input.Username,
                Email = input.Email,
                Role = "User",
                Contact = input.Contact,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password)
            };

            await collection.InsertOneAsync(newUser);
            return new RegisterResponse
            {
                Id = newUser.Id,
                Name = newUser.Name,
                Username = newUser.Username,
                Email = newUser.Email,
                Role = newUser.Role,
                Contact = newUser.Contact
            };
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<User> UpdateUserEmailAsync(
            string userId,
            string newEmail,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<User>("users");

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Set(u => u.Email, newEmail);

            var result = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            });

            return result;
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<bool> DeleteUserAsync(
            string userId,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<User>("users");

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var result = await collection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        [Authorize]
        public async Task<User> ChangeUserPasswordAsync(
            string userId,
            string newPassword,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<User>("users");

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Set(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword(newPassword));

            var result = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            });

            return result;
        }
    }
}