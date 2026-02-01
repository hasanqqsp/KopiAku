using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;

namespace KopiAku.GraphQL.Carousel
{
    [ExtendObjectType(typeof(Query))]
    public class CarouselContentQueries
    {
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<CarouselContent> GetCarouselContents([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<CarouselContent>("carousel-contents");
            return collection.AsExecutable();
        }

        public async Task<List<CarouselContent>> GetAllCarouselContentsAsync([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<CarouselContent>("carousel-contents");
            var sort = Builders<CarouselContent>.Sort.Descending(c => c.CreatedAt);

            return await collection.Find(Builders<CarouselContent>.Filter.Empty)
                                  .Sort(sort)
                                  .ToListAsync();
        }
    }
}
