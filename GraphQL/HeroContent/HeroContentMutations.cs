using MongoDB.Driver;
using KopiAku.Models;
using KopiAku.DTOs;
using HotChocolate.Authorization;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using KopiAku.Settings;

namespace KopiAku.GraphQL.HeroContent
{
    [ExtendObjectType(typeof(Mutation))]
    public class HeroContentMutations
    {
        private readonly string _bucketName = "kopiaku-bucket";

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<Models.HeroContent> UpdateOrCreateHeroContentAsync(
            UpdateHeroContentInput input,
            IFile? backgroundImage,
            [Service] IMongoDatabase database,
            [Service] IAmazonS3 s3Client,
            [Service] IOptions<B2Settings> b2Settings)
        {
            var collection = database.GetCollection<Models.HeroContent>("hero-contents");

            // Get the most recent hero content (if any exists)
            var existing = await collection.Find(Builders<Models.HeroContent>.Filter.Empty)
                                          .Sort(Builders<Models.HeroContent>.Sort.Descending(h => h.UpdatedAt))
                                          .FirstOrDefaultAsync();

            if (existing != null)
            {
                // Update existing hero content
                var updateBuilder = Builders<Models.HeroContent>.Update;
                var updates = new List<UpdateDefinition<Models.HeroContent>>();

                if (!string.IsNullOrEmpty(input.Title))
                {
                    updates.Add(updateBuilder.Set(h => h.Title, input.Title));
                }

                if (!string.IsNullOrEmpty(input.Description))
                {
                    updates.Add(updateBuilder.Set(h => h.Description, input.Description));
                }

                if (backgroundImage != null)
                {
                    var backgroundImageUrl = await UploadImageToS3Async(backgroundImage, s3Client, b2Settings.Value);
                    updates.Add(updateBuilder.Set(h => h.BackgroundImageUrl, backgroundImageUrl));

                    // Delete old image from S3 if it exists
                    if (!string.IsNullOrEmpty(existing.BackgroundImageUrl))
                    {
                        await DeleteImageFromS3Async(existing.BackgroundImageUrl, s3Client, b2Settings.Value);
                    }
                }

                updates.Add(updateBuilder.Set(h => h.UpdatedAt, DateTime.UtcNow));

                if (updates.Any())
                {
                    var filter = Builders<Models.HeroContent>.Filter.Eq(h => h.Id, existing.Id);
                    var update = updateBuilder.Combine(updates);
                    await collection.UpdateOneAsync(filter, update);
                }

                return await collection.Find(Builders<Models.HeroContent>.Filter.Eq(h => h.Id, existing.Id)).FirstOrDefaultAsync();
            }
            else
            {
                // Create new hero content if none exists
                string backgroundImageUrl = string.Empty;
                if (backgroundImage != null)
                {
                    backgroundImageUrl = await UploadImageToS3Async(backgroundImage, s3Client, b2Settings.Value);
                }

                var heroContent = new Models.HeroContent
                {
                    Title = input.Title ?? string.Empty,
                    Description = input.Description ?? string.Empty,
                    BackgroundImageUrl = backgroundImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await collection.InsertOneAsync(heroContent);
                return heroContent;
            }
        }

        private async Task<string> UploadImageToS3Async(IFile file, IAmazonS3 s3Client, B2Settings b2Settings)
        {
            var fileName = $"hero-content/{Guid.NewGuid()}-{file.Name}";

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
