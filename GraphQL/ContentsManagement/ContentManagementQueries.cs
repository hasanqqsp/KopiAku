using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;

namespace KopiAku.GraphQL.ContentsManagement
{
    [ExtendObjectType(typeof(Query))]
    public class ContentManagementQueries
    {
        [Authorize(Roles = new[] { "Admin" })]
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<ContentManagement> GetContents([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<ContentManagement>("contents-management");
            return collection.AsExecutable();
        }
    }
}