using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;


namespace KopiAku.GraphQL.Users
{
    [ExtendObjectType(typeof(Query))]
    public class UserQueries
    {
        [Authorize(Roles = new[] { "Admin" })]
        [UsePaging(IncludeTotalCount = true)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<User> GetUsers([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<User>("users");
            return collection.AsExecutable();
        }
    }
}