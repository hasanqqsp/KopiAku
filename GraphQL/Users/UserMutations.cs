using MongoDB.Driver;
using KopiAku.Models;
using KopiAku.Services;
using KopiAku.DTOs;
using Amazon.S3;
using Amazon.S3.Model;
using HotChocolate.Authorization;
using System.Security.Claims;
using HotChocolate.Types;

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
            try
            {
                var collection = database.GetCollection<User>("users");

                var user = await collection.Find(u => u.Username.ToLower() == input.Username.ToLower()).FirstOrDefaultAsync();
                if (user == null || !BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
                {
                    throw new GraphQLException("Invalid username or password.");
                }

                if (!user.IsActive)
                {
                    throw new GraphQLException("Account is inactive.");
                }

                // In a real application, generate a JWT or similar token here
                var token = jwtService.GenerateToken(user);

                var presenceCollection = database.GetCollection<Presence>("presences");
                var utcNow = DateTime.UtcNow;
                var offset = TimeSpan.FromHours(7);
                var nowInTz = utcNow + offset;
                var todayStart = nowInTz.Date - offset;
                var todayEnd = todayStart + TimeSpan.FromDays(1);
                var presence = await presenceCollection.Find(p => p.UserId == user.Id && p.CheckInTime >= todayStart && p.CheckInTime < todayEnd && p.CheckOutTime == default(DateTime)).FirstOrDefaultAsync();

                return new LoginResponse
                {
                    Token = token,
                    Id = user.Id,
                    Name = user.Name,
                    Username = user.Username,
                    Nickname = user.Nickname,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    Email = user.Email,
                    Presence = presence
                };
            }
            catch (Exception ex)
            {
                throw new GraphQLException($"Login failed: {ex.Message}");
            }
        }

        [AllowAnonymous]
        public async Task<RegisterResponse> RegisterAsync(
            RegisterInput input,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<User>("users");

            var existingUser = await collection.Find(u => u.Username.ToLower() == input.Username.ToLower() || u.Email == input.Email || u.Name == input.Name).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                throw new GraphQLException("Username, email, or name already exists.");
            }

            var newUser = new User
            {
                Name = input.Name,
                Username = input.Username,
                Nickname = input.Nickname,
                Email = input.Email,
                Role = "User",
                Contact = input.Contact,
                IsActive = false,
                ProfilePictureUrl = $"https://ui-avatars.com/api/?size=256&background=0D8ABC&name={Uri.EscapeDataString(input.Name)}",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password)
            };

            await collection.InsertOneAsync(newUser);
            return new RegisterResponse
            {
                Id = newUser.Id,
                Name = newUser.Name,
                Username = newUser.Username,
                Nickname = newUser.Nickname,
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
            user.Nickname = input.Nickname ?? user.Nickname;
            user.Contact = input.Contact ?? user.Contact;

            await collection.ReplaceOneAsync(u => u.Id == userId, user);
            return new UpdateUserProfileResponse
            {
                Id = user.Id,
                Name = user.Name,
                Username = user.Username,
                Nickname = user.Nickname,
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

        [Authorize]
        public async Task<bool> ChangePasswordAsync(
            ChangePasswordInput input,
            [Service] IMongoDatabase database,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new GraphQLException("Unauthorized");
            }

            var collection = database.GetCollection<User>("users");
            var user = await collection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new GraphQLException("User not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(input.CurrentPassword, user.PasswordHash))
            {
                throw new GraphQLException("Current password is incorrect");
            }

            var newHash = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);
            var update = Builders<User>.Update.Set(u => u.PasswordHash, newHash);
            await collection.UpdateOneAsync(u => u.Id == userId, update);

            return true;
        }

        [Authorize]
        public async Task<UpdateUserProfileResponse> UpdateMyProfileAsync(
            UpdateUserProfileInput input,
            [GraphQLType(typeof(UploadType))] IFile? profilePicture,
            [Service] IMongoDatabase database,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new GraphQLException("Unauthorized");
            }

            var collection = database.GetCollection<User>("users");
            var user = await collection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new GraphQLException("User not found");
            }

            // Check uniqueness for username and email if provided
            if (!string.IsNullOrEmpty(input.Username) && input.Username != user.Username)
            {
                var existingUsername = await collection.Find(u => u.Username.ToLower() == input.Username.ToLower() && u.Id != userId).FirstOrDefaultAsync();
                if (existingUsername != null)
                {
                    throw new GraphQLException("Username already exists");
                }
            }

            if (!string.IsNullOrEmpty(input.Email) && input.Email != user.Email)
            {
                var existingEmail = await collection.Find(u => u.Email == input.Email && u.Id != userId).FirstOrDefaultAsync();
                if (existingEmail != null)
                {
                    throw new GraphQLException("Email already exists");
                }
            }

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
            user.Nickname = input.Nickname ?? user.Nickname;
            user.Email = input.Email ?? user.Email;
            user.Contact = input.Contact ?? user.Contact;

            await collection.ReplaceOneAsync(u => u.Id == userId, user);
            return new UpdateUserProfileResponse
            {
                Id = user.Id,
                Name = user.Name,
                Username = user.Username,
                Nickname = user.Nickname,
                Email = user.Email,
                Role = user.Role,
                Contact = user.Contact,
                IsActive = user.IsActive,
                ProfilePictureUrl = user.ProfilePictureUrl
            };
        }
    }
}