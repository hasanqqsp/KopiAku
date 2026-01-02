using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;
using HotChocolate;

namespace KopiAku.GraphQL.Menus
{
    [ExtendObjectType(typeof(Query))]
    public class MenuQueries
    {
        // Get Menus
        [Authorize]
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseSorting]
        public IExecutable<Menu> GetMenus([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Menu>("menus");
            return collection.AsExecutable();
        }
    }

    [ExtendObjectType(typeof(Menu))]
    public class MenuResolvers
    {
        public async Task<List<Recipe>> GetRecipes([Parent] Menu menu, [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Recipe>("recipes");
            return await collection.Find(r => r.MenuId == menu.Id).ToListAsync();
        }
    }

    [ExtendObjectType(typeof(RecipeIngredient))]
    public class RecipeIngredientResolvers
    {
        public async Task<Stock?> GetStock([Parent] RecipeIngredient ingredient, [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Stock>("stocks");
            return await collection.Find(s => s.Id == ingredient.StockId).FirstOrDefaultAsync();
        }
    }
}