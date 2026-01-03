using MongoDB.Driver;
using KopiAku.Models;
using KopiAku.DTOs;
using HotChocolate.Authorization;

namespace KopiAku.GraphQL.Contact
{
    [ExtendObjectType(typeof(Mutation))]
    public class ContactContentMutations
    {
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<ContactContent> UpdateOrCreateContactContentAsync(
            UpdateContactContentInput input,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<ContactContent>("contact-contents");

            // Get the most recent contact content (if any exists)
            var existing = await collection.Find(Builders<ContactContent>.Filter.Empty)
                                          .Sort(Builders<ContactContent>.Sort.Descending(c => c.UpdatedAt))
                                          .FirstOrDefaultAsync();

            if (existing != null)
            {
                // Update existing contact content
                var updateBuilder = Builders<ContactContent>.Update;
                var updates = new List<UpdateDefinition<ContactContent>>();

                if (!string.IsNullOrEmpty(input.OperationalHours))
                {
                    updates.Add(updateBuilder.Set(c => c.OperationalHours, input.OperationalHours));
                }

                if (!string.IsNullOrEmpty(input.Address))
                {
                    updates.Add(updateBuilder.Set(c => c.Address, input.Address));
                }

                if (!string.IsNullOrEmpty(input.Whatsapp))
                {
                    updates.Add(updateBuilder.Set(c => c.Whatsapp, input.Whatsapp));
                }

                if (!string.IsNullOrEmpty(input.Instagram))
                {
                    updates.Add(updateBuilder.Set(c => c.Instagram, input.Instagram));
                }

                if (!string.IsNullOrEmpty(input.GoogleMaps))
                {
                    updates.Add(updateBuilder.Set(c => c.GoogleMaps, input.GoogleMaps));
                }

                updates.Add(updateBuilder.Set(c => c.UpdatedAt, DateTime.UtcNow));

                if (updates.Any())
                {
                    var filter = Builders<ContactContent>.Filter.Eq(c => c.Id, existing.Id);
                    var update = updateBuilder.Combine(updates);
                    await collection.UpdateOneAsync(filter, update);
                }

                return await collection.Find(Builders<ContactContent>.Filter.Eq(c => c.Id, existing.Id)).FirstOrDefaultAsync();
            }
            else
            {
                // Create new contact content if none exists
                var contactContent = new ContactContent
                {
                    OperationalHours = input.OperationalHours ?? string.Empty,
                    Address = input.Address ?? string.Empty,
                    Whatsapp = input.Whatsapp ?? string.Empty,
                    Instagram = input.Instagram ?? string.Empty,
                    GoogleMaps = input.GoogleMaps ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await collection.InsertOneAsync(contactContent);
                return contactContent;
            }
        }
    }
}
