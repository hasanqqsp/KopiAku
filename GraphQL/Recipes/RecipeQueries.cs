using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;

namespace KopiAku.GraphQL.Recipes
{
    [ExtendObjectType(typeof(Query))]
    public class RecipeQueries
    {
        [Authorize(Roles = new[] { "Admin" })]
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<Recipe> GetRecipes([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Recipe>("recipes");
            return collection.AsExecutable();
        }
    }
}