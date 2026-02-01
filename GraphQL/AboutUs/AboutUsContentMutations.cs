using MongoDB.Driver;
using KopiAku.Models;
using KopiAku.DTOs;
using HotChocolate.Authorization;

namespace KopiAku.GraphQL.AboutUs
{
    [ExtendObjectType(typeof(Mutation))]
    public class AboutUsContentMutations
    {

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<AboutUsContent> UpdateOrCreateAboutUsContentAsync(
            UpdateAboutUsContentInput input,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<AboutUsContent>("about-us-contents");

            // Get the most recent about us content (if any exists)
            var existing = await collection.Find(Builders<AboutUsContent>.Filter.Empty)
                                          .Sort(Builders<AboutUsContent>.Sort.Descending(a => a.UpdatedAt))
                                          .FirstOrDefaultAsync();

            if (existing != null)
            {
                // Update existing about us content
                var updateBuilder = Builders<AboutUsContent>.Update;
                var updates = new List<UpdateDefinition<AboutUsContent>>();

                if (!string.IsNullOrEmpty(input.Paragraph1))
                {
                    updates.Add(updateBuilder.Set(a => a.Paragraph1, input.Paragraph1));
                }

                if (!string.IsNullOrEmpty(input.Paragraph2))
                {
                    updates.Add(updateBuilder.Set(a => a.Paragraph2, input.Paragraph2));
                }

                if (!string.IsNullOrEmpty(input.Vision))
                {
                    updates.Add(updateBuilder.Set(a => a.Vision, input.Vision));
                }

                if (input.Mission != null && input.Mission.Any())
                {
                    updates.Add(updateBuilder.Set(a => a.Mission, input.Mission));
                }

                if (!string.IsNullOrEmpty(input.Background))
                {
                    updates.Add(updateBuilder.Set(a => a.Background, input.Background));
                }

                updates.Add(updateBuilder.Set(a => a.UpdatedAt, DateTime.UtcNow));

                if (updates.Any())
                {
                    var filter = Builders<AboutUsContent>.Filter.Eq(a => a.Id, existing.Id);
                    var update = updateBuilder.Combine(updates);
                    await collection.UpdateOneAsync(filter, update);
                }

                return await collection.Find(Builders<AboutUsContent>.Filter.Eq(a => a.Id, existing.Id)).FirstOrDefaultAsync();
            }
            else
            {
                // Create new about us content if none exists
                var aboutUsContent = new AboutUsContent
                {
                    Paragraph1 = input.Paragraph1 ?? string.Empty,
                    Paragraph2 = input.Paragraph2 ?? string.Empty,
                    Vision = input.Vision ?? string.Empty,
                    Mission = input.Mission ?? new List<string>(),
                    Background = input.Background ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await collection.InsertOneAsync(aboutUsContent);
                return aboutUsContent;
            }
        }
    }
}
