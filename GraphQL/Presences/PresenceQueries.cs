using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;

namespace KopiAku.GraphQL.Users
{
    [ExtendObjectType(typeof(Query))]
    public class PresenceQueries
    {
        [Authorize]
        [UsePaging(IncludeTotalCount = true)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<Presence> GetPresences([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Presence>("presences");
            return collection.AsExecutable();
        }
    }
}