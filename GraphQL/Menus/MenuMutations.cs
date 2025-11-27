using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;
using Amazon.S3;
using Amazon.S3.Model;
using System.Security.Claims;
using KopiAku.DTOs;

namespace KopiAku.GraphQL.Menus
{
    [ExtendObjectType(typeof(Mutation))]
    public class MenuMutations(IAmazonS3 s3Client)
    {
        private readonly IAmazonS3 _s3Client = s3Client;
        private readonly string _bucketName = "kopiaku-bucket";
        
        // Create Menu
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<Menu> CreateMenuAsync(MenuInput menu,[Service] IMongoDatabase database, [GraphQLType(typeof(UploadType))] IFile image)
        {
            var collection = database.GetCollection<Menu>("menus");
            
            // Upload image to S3
            try
            {
                var imageKey = $"menus/{Guid.NewGuid()}_{image.Name}";
                using var stream = image.OpenReadStream();
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = imageKey,
                    InputStream = stream,
                    ContentType = image.ContentType
                };
                await _s3Client.PutObjectAsync(putRequest);
                var newMenu = new Menu
                {
                    Name = menu.Name,
                    Description = menu.Description,
                    Category = menu.Category,
                    Price = menu.Price,
                    ImageUrl = $"https://storage.czn.my.id/{_bucketName}/{imageKey}",
                    IsAvailable = false
                };
                await collection.InsertOneAsync(newMenu);
                return newMenu;
            }
            catch (Exception)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Failed to upload image.")
                    .SetCode("IMAGE_UPLOAD_FAILED")
                    .Build());
            }
        }

        // Delete Menu
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<bool> DeleteMenuAsync(string id, [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Menu>("menus");
            var result = await collection.DeleteOneAsync(m => m.Id == id);
            return result.DeletedCount > 0;
        }
    }
}