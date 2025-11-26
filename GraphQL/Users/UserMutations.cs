using MongoDB.Driver;
using KopiAku.Models;
using KopiAku.Services;
using KopiAku.DTOs;
using Amazon.S3;
using Amazon.S3.Model;
using HotChocolate.Authorization;

namespace KopiAku.GraphQL.Users
{
    [ExtendObjectType(typeof(Mutation))]
    public class UserMutations(IAmazonS3 s3Client)
    {
        private readonly IAmazonS3 _s3Client = s3Client;
        private readonly string _bucketName = "kopiaku-bucket";

        [AllowAnonymous]
        public async Task<LoginResponse> LoginAsync(
            LoginInput input,
            [Service] IMongoDatabase database,
            [Service] JWTService jwtService)
        {
            var collection = database.GetCollection<User>("users");

            var user = await collection.Find(u => u.Username == input.Username).FirstOrDefaultAsync();
            if (user == null || !BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
            {
                throw new GraphQLException("Invalid username or password.");
            }

            // In a real application, generate a JWT or similar token here
            var token = jwtService.GenerateToken(user);

            return new LoginResponse
            {
                Token = token,
                Id = user.Id,
                Name = user.Name,
                Username = user.Username,
                ProfilePictureUrl = user.ProfilePictureUrl,
                IsActive = user.IsActive,
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
                IsActive = false,
                ProfilePictureUrl = "https://ui-avatars.com/api/?size=256&font-size=0.01&background=0D8ABC",
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
                Contact = newUser.Contact,
                IsActive = newUser.IsActive,
                ProfilePictureUrl = newUser.ProfilePictureUrl
            };
        }

        [Authorize]
        public async Task<UpdateUserProfileResponse> UpdateUserProfileAsync(
            string userId,
            RegisterInput input,
            [GraphQLType(typeof(UploadType))] IFile? profilePicture,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<User>("users");
            var user = await collection.Find(u => u.Id == userId).FirstOrDefaultAsync() ?? throw new GraphQLException(
                    ErrorBuilder.New()
                    .SetMessage("User not found.")
                    .SetCode("USER_NOT_FOUND")
                    .Build());

            if (profilePicture != null)
            {
                // Upload new profile picture to S3
                var imageKey = $"{userId}/{Guid.NewGuid()}_{profilePicture.Name}";
                var putRequest = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = imageKey,
                        InputStream = profilePicture.OpenReadStream(),
                        ContentType = profilePicture.ContentType
                    };
                    await _s3Client.PutObjectAsync(putRequest);
                user.ProfilePictureUrl = $"https://storage.czn.my.id/{_bucketName}/{imageKey}";
            }

            user.Name = input.Name ?? user.Name;
            user.Username = input.Username ?? user.Username;
            user.Email = input.Email ?? user.Email;
            user.Contact = input.Contact ?? user.Contact;

            await collection.ReplaceOneAsync(u => u.Id == userId, user);
            return new UpdateUserProfileResponse
            {
                Id = user.Id,
                Name = user.Name,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Contact = user.Contact,
                IsActive = user.IsActive,
                ProfilePictureUrl = user.ProfilePictureUrl
            };
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

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<User> SetUserActiveStatusAsync(
            string userId,
            bool isActive,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<User>("users");

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Set(u => u.IsActive, isActive);

            var result = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            });

            return result;
        }
    }
}