using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;
using KopiAku.DTOs;
using System.Security.Claims;


namespace KopiAku.GraphQL.Users
{
    [ExtendObjectType(typeof(Query))]
    public class UserQueries
    {
        [Authorize(Roles = new[] { "Admin" })]
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<User> GetUsers([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<User>("users");
            return collection.AsExecutable();
        }

        [Authorize]
        public async Task<RegisterResponse> GetMyProfileAsync(
            [Service] IMongoDatabase database,
            ClaimsPrincipal claimsPrincipal)
        {
            var collection = database.GetCollection<User>("users");
            var userId = (claimsPrincipal.FindFirst("sub")?.Value
                     ?? claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value) ?? throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("User ID not found in claims.")
                    .SetCode("USER_ID_NOT_FOUND")
                    .Build());

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var user = await collection.Find(filter).FirstOrDefaultAsync() ?? throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("User not found.")
                    .SetCode("USER_NOT_FOUND")
                    .Build());
                    
            return new RegisterResponse
            {
                Id = user.Id,
                Name = user.Name,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Contact = user.Contact,
                IsActive = user.IsActive,
                ProfilePictureUrl = user.ProfilePictureUrl
            };
        }
    }
}