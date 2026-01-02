using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;

namespace KopiAku.GraphQL.Recipes
{
    [ExtendObjectType(typeof(Mutation))]
    public class RecipeMutations
    {
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<Recipe> CreateRecipeAsync(
            string menuId,
            List<RecipeIngredient> ingredients,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Recipe>("recipes");
            var menuCollection = database.GetCollection<Menu>("menus");
            var stockCollection = database.GetCollection<Stock>("stocks");

            // Verify that the menu exists
            var menu = await menuCollection.Find(m => m.Id == menuId).FirstOrDefaultAsync() ?? throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Menu not found")
                    .SetCode("MENU_NOT_FOUND")
                    .Build());

            // Verify that all ingredients exist in stock
            foreach (var ingredient in ingredients)
            {
                var stockItem = await stockCollection.Find(s => s.Id == ingredient.StockId).FirstOrDefaultAsync() ?? throw new GraphQLException(ErrorBuilder.New()
                        .SetMessage($"Ingredient not found in stock")
                        .SetCode("INGREDIENT_NOT_FOUND")
                        .Build());
            }

            // Check if recipe already exists for the menu
            var existingRecipe = await collection.Find(r => r.MenuId == menuId).FirstOrDefaultAsync();
            if (existingRecipe != null)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Recipe already exists for this menu.")
                    .SetCode("RECIPE_ALREADY_EXISTS")
                    .Build());
            }

            // Create and insert the new recipe
            var newRecipe = new Recipe
            {
                MenuId = menuId,
                Ingredients = ingredients
            };

            // Insert the new recipe into the database
            await collection.InsertOneAsync(newRecipe);

            // Update menu availability
            bool isAvailable = true;
            foreach (var ingredient in ingredients)
            {
                var stockItem = await stockCollection.Find(s => s.Id == ingredient.StockId).FirstOrDefaultAsync();
                if (stockItem.Quantity < ingredient.Quantity)
                {
                    isAvailable = false;
                    break;
                }
            }
            menu.IsAvailable = isAvailable;
            await menuCollection.ReplaceOneAsync(m => m.Id == menuId, menu);

            return newRecipe;
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<Recipe> UpdateRecipeAsync(
            string recipeId,
            List<RecipeIngredient> ingredients,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Recipe>("recipes");
            var stockCollection = database.GetCollection<Stock>("stocks");
            var menuCollection = database.GetCollection<Menu>("menus");

            // Verify that the recipe exists
            var recipe = await collection.Find(r => r.Id == recipeId).FirstOrDefaultAsync() ?? throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Recipe not found")
                    .SetCode("RECIPE_NOT_FOUND")
                    .Build());

            // Verify that all ingredients exist in stock
            foreach (var ingredient in ingredients)
            {
                var stockItem = await stockCollection.Find(s => s.Id == ingredient.StockId).FirstOrDefaultAsync() ?? throw new GraphQLException(ErrorBuilder.New()
                        .SetMessage($"Ingredient not found in stock")
                        .SetCode("INGREDIENT_NOT_FOUND")
                        .Build());
            }

            // Update the recipe
            recipe.Ingredients = ingredients;
            await collection.ReplaceOneAsync(r => r.Id == recipeId, recipe);

            // Update menu availability
            var menu = await menuCollection.Find(m => m.Id == recipe.MenuId).FirstOrDefaultAsync();
            if (menu != null)
            {
                menu.IsAvailable = ingredients.All(ingredient =>
                {
                    var stockItem = stockCollection.Find(s => s.Id == ingredient.StockId).FirstOrDefaultAsync().Result;
                    return stockItem != null && stockItem.Quantity >= ingredient.Quantity;
                });
                await menuCollection.ReplaceOneAsync(m => m.Id == menu.Id, menu);
            }

            return recipe;
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<bool> DeleteRecipeAsync(
            string recipeId,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Recipe>("recipes");
            var result = await collection.DeleteOneAsync(r => r.Id == recipeId);
            return result.DeletedCount > 0;
        }
    }     
}