using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;

namespace KopiAku.GraphQL.Menus
{
    [ExtendObjectType(typeof(Query))]
    public class MenuQueries
    {
        // Get Menus
        [Authorize]
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<Menu> GetMenus([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Menu>("menus");
            return collection.AsExecutable();
        }
    }
}