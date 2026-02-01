using MongoDB.Driver;
using KopiAku.Models;
using KopiAku.DTOs;
using HotChocolate.Authorization;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using KopiAku.Settings;

namespace KopiAku.GraphQL.Carousel
{
    [ExtendObjectType(typeof(Mutation))]
    public class CarouselContentMutations
    {
        private readonly string _bucketName = "kopiaku-bucket";

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<CarouselContent> AddCarouselContentAsync(
            CarouselContentInput input,
            [Service] IMongoDatabase database,
            [Service] IAmazonS3 s3Client,
            [Service] IOptions<B2Settings> b2Settings)
        {
            var collection = database.GetCollection<CarouselContent>("carousel-contents");

            // Upload image to S3
            var imageUrl = await UploadImageToS3Async(input.Image, s3Client, b2Settings.Value);

            var carouselContent = new CarouselContent
            {
                AltText = input.AltText,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await collection.InsertOneAsync(carouselContent);
            return carouselContent;
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<bool> DeleteCarouselContentAsync(
            string id,
            [Service] IMongoDatabase database,
            [Service] IAmazonS3 s3Client,
            [Service] IOptions<B2Settings> b2Settings)
        {
            var collection = database.GetCollection<CarouselContent>("carousel-contents");
            var filter = Builders<CarouselContent>.Filter.Eq(c => c.Id, id);

            var existing = await collection.Find(filter).FirstOrDefaultAsync();
            if (existing == null)
                return false;

            // Delete image from S3 if it exists
            if (!string.IsNullOrEmpty(existing.ImageUrl))
            {
                await DeleteImageFromS3Async(existing.ImageUrl, s3Client, b2Settings.Value);
            }

            var result = await collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        private async Task<string> UploadImageToS3Async(IFile file, IAmazonS3 s3Client, B2Settings b2Settings)
        {
            var fileName = $"carousel/{Guid.NewGuid()}-{file.Name}";

            using var stream = file.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = stream,
                ContentType = file.ContentType
            };

            await s3Client.PutObjectAsync(request);

            return $"https://storage.czn.my.id/{_bucketName}/{fileName}";
        }

        private async Task DeleteImageFromS3Async(string imageUrl, IAmazonS3 s3Client, B2Settings b2Settings)
        {
            try
            {
                // Extract key from URL
                var uri = new Uri(imageUrl);
                var key = uri.AbsolutePath.TrimStart('/').Replace($"{_bucketName}/", "");

                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await s3Client.DeleteObjectAsync(request);
            }
            catch (Exception)
            {
                // Log error but don't throw - we don't want to fail the operation if S3 delete fails
            }
        }
    }
}
