using MongoDB.Driver;
using KopiAku.Models;
using KopiAku.DTOs;
using HotChocolate.Authorization;

namespace KopiAku.GraphQL.Promotion
{
    [ExtendObjectType(typeof(Mutation))]
    public class PromotionContentMutations
    {
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<PromotionContent> UpdateOrCreatePromotionContentAsync(
            UpdatePromotionContentInput input,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<PromotionContent>("promotion-contents");

            // Get the most recent promotion content (if any exists)
            var existing = await collection.Find(Builders<PromotionContent>.Filter.Empty)
                                          .Sort(Builders<PromotionContent>.Sort.Descending(p => p.UpdatedAt))
                                          .FirstOrDefaultAsync();

            if (existing != null)
            {
                // Update existing promotion content
                var updateBuilder = Builders<PromotionContent>.Update;
                var updates = new List<UpdateDefinition<PromotionContent>>();

                if (!string.IsNullOrEmpty(input.Title))
                {
                    updates.Add(updateBuilder.Set(p => p.Title, input.Title));
                }

                if (input.Rules != null && input.Rules.Any())
                {
                    updates.Add(updateBuilder.Set(p => p.Rules, input.Rules));
                }

                updates.Add(updateBuilder.Set(p => p.UpdatedAt, DateTime.UtcNow));

                if (updates.Any())
                {
                    var filter = Builders<PromotionContent>.Filter.Eq(p => p.Id, existing.Id);
                    var update = updateBuilder.Combine(updates);
                    await collection.UpdateOneAsync(filter, update);
                }

                return await collection.Find(Builders<PromotionContent>.Filter.Eq(p => p.Id, existing.Id)).FirstOrDefaultAsync();
            }
            else
            {
                // Create new promotion content if none exists
                var promotionContent = new PromotionContent
                {
                    Title = input.Title ?? string.Empty,
                    Rules = input.Rules ?? new List<string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await collection.InsertOneAsync(promotionContent);
                return promotionContent;
            }
        }
    }
}
