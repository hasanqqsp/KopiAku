using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;

namespace KopiAku.GraphQL.Promotion
{
    [ExtendObjectType(typeof(Query))]
    public class PromotionContentQueries
    {
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<PromotionContent> GetPromotionContents([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<PromotionContent>("promotion-contents");
            return collection.AsExecutable();
        }

        public async Task<PromotionContent?> GetActivePromotionContentAsync([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<PromotionContent>("promotion-contents");
            var filter = Builders<PromotionContent>.Filter.Empty;
            var sort = Builders<PromotionContent>.Sort.Descending(p => p.UpdatedAt);

            return await collection.Find(filter).Sort(sort).FirstOrDefaultAsync();
        }
    }
}
