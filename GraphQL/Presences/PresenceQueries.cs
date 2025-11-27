using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace KopiAku.GraphQL.Presences
{
    [ExtendObjectType(typeof(Query))]
    public class PresenceQueries
    {
        [Authorize(Roles = new[] { "Admin" })]
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<Presence> GetPresences([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Presence>("presences");
            return collection.AsExecutable();
        }

        [Authorize]
        public async Task<Presence?> GetMyPresenceAsync(
            [Service] IMongoDatabase database,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal.FindFirst("sub")?.Value
                ?? claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var collection = database.GetCollection<Presence>("presences");
            return await collection.Find(p => p.UserId == userId).FirstOrDefaultAsync();
        }
    }
}