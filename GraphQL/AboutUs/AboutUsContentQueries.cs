using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;

namespace KopiAku.GraphQL.AboutUs
{
    [ExtendObjectType(typeof(Query))]
    public class AboutUsContentQueries
    {
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<AboutUsContent> GetAboutUsContents([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<AboutUsContent>("about-us-contents");
            return collection.AsExecutable();
        }

        public async Task<AboutUsContent?> GetActiveAboutUsContentAsync([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<AboutUsContent>("about-us-contents");
            var filter = Builders<AboutUsContent>.Filter.Empty;
            var sort = Builders<AboutUsContent>.Sort.Descending(a => a.UpdatedAt);

            return await collection.Find(filter).Sort(sort).FirstOrDefaultAsync();
        }
    }
}
