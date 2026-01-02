using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;
using Amazon.S3;
using Amazon.S3.Model;
using System.Security.Claims;

namespace KopiAku.GraphQL.Presences
{
    [ExtendObjectType(typeof(Mutation))]
    public class PresenceMutations(IAmazonS3 s3Client)
    {
        private readonly IAmazonS3 _s3Client = s3Client;
        private readonly string _bucketName = "kopiaku-bucket";

        // Check-In
        [Authorize]
        public async Task<Presence> CheckInAsync(
            [Service] IMongoDatabase database,
            [GraphQLType(typeof(UploadType))] IFile image,
            ClaimsPrincipal claimsPrincipal)
        {
            var collection = database.GetCollection<Presence>("presences");
            var userId = (claimsPrincipal.FindFirst("sub")?.Value
                     ?? claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value) ?? throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("User ID not found in claims.")
                    .SetCode("USER_ID_NOT_FOUND")
                    .Build());

            // Upload image to S3
            try
            {
                var imageKey = $"{userId}/{Guid.NewGuid()}_{image.Name}";
                using (var stream = image.OpenReadStream())
                {
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = imageKey,
                        InputStream = stream,
                        ContentType = image.ContentType
                    };
                    await _s3Client.PutObjectAsync(putRequest);
                }

                var presence = new Presence
                {
                    UserId = userId,
                    CheckInTime = DateTime.UtcNow,
                    Validated = false,
                    ImageUrl = $"https://storage.czn.my.id/{_bucketName}/{imageKey}"
                };

                await collection.InsertOneAsync(presence);
                return presence;
            }
            catch (Exception)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Failed to upload image.")
                    .SetCode("IMAGE_UPLOAD_FAILED")
                    .Build());
            }
            
        }

        // Check-Out
        [Authorize]
        public async Task<Presence> CheckOutAsync(
            [Service] IMongoDatabase database,
            ClaimsPrincipal claimsPrincipal)
        {
            var collection = database.GetCollection<Presence>("presences");
            var userId = (claimsPrincipal.FindFirst("sub")?.Value
                     ?? claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value) ?? throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("User ID not found in claims.")
                    .SetCode("USER_ID_NOT_FOUND")
                    .Build());

            var filter = Builders<Presence>.Filter.Eq(p => p.UserId, userId) &
                         Builders<Presence>.Filter.Eq(p => p.CheckOutTime, default(DateTime));
                        
            var presence = await collection.Find(filter).SortByDescending(p => p.CheckInTime).FirstOrDefaultAsync() ?? throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("No active presence found.")
                    .SetCode("NO_ACTIVE_PRESENCE")
                    .Build());
            presence.CheckOutTime = DateTime.UtcNow;
            await collection.ReplaceOneAsync(p => p.Id == presence.Id, presence);
            return presence;
        }

        // Validate Presence
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<Presence> ValidatePresenceAsync(
            [Service] IMongoDatabase database,
            string presenceId)
        {
            var collection = database.GetCollection<Presence>("presences");
            var update = Builders<Presence>.Update.Set(p => p.Validated, true);
            var filter = Builders<Presence>.Filter.Eq(p => p.Id, presenceId);
            var result = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Presence>
            {
                ReturnDocument = ReturnDocument.After
            });
            return result;
        }
    }
}