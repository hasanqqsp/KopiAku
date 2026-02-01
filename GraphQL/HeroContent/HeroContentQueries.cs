using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;

namespace KopiAku.GraphQL.HeroContent
{
    [ExtendObjectType(typeof(Query))]
    public class HeroContentQueries
    {
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<Models.HeroContent> GetHeroContents([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Models.HeroContent>("hero-contents");
            return collection.AsExecutable();
        }

        public async Task<Models.HeroContent?> GetActiveHeroContentAsync([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Models.HeroContent>("hero-contents");
            var filter = Builders<Models.HeroContent>.Filter.Empty;
            var sort = Builders<Models.HeroContent>.Sort.Descending(h => h.UpdatedAt);

            return await collection.Find(filter).Sort(sort).FirstOrDefaultAsync();
        }
    }
}
