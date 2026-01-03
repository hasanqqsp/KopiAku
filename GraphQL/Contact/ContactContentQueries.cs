using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;

namespace KopiAku.GraphQL.Contact
{
    [ExtendObjectType(typeof(Query))]
    public class ContactContentQueries
    {
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<ContactContent> GetContactContents([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<ContactContent>("contact-contents");
            return collection.AsExecutable();
        }

        public async Task<ContactContent?> GetActiveContactContentAsync([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<ContactContent>("contact-contents");
            var filter = Builders<ContactContent>.Filter.Empty;
            var sort = Builders<ContactContent>.Sort.Descending(c => c.UpdatedAt);

            return await collection.Find(filter).Sort(sort).FirstOrDefaultAsync();
        }
    }
}
