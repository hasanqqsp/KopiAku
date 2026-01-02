using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;
using Amazon.S3;
using Amazon.S3.Model;
using System.Security.Claims;
using KopiAku.DTOs;
using System.Linq;

namespace KopiAku.GraphQL.Menus
{
    [ExtendObjectType(typeof(Mutation))]
    public class MenuMutations(IAmazonS3 s3Client)
    {
        private readonly IAmazonS3 _s3Client = s3Client;
        private readonly string _bucketName = "kopiaku-bucket";
        
        // Create Menu
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<Menu> CreateMenuAsync(CreateMenuInput menu,[Service] IMongoDatabase database, [GraphQLType(typeof(UploadType))] IFile image)
        {
            var menuCollection = database.GetCollection<Menu>("menus");
            var recipeCollection = database.GetCollection<Recipe>("recipes");
            var stockCollection = database.GetCollection<Stock>("stocks");
            
            // Upload image to S3
            try
            {
                var imageKey = $"menus/{Guid.NewGuid()}_{image.Name}";
                using var stream = image.OpenReadStream();
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = imageKey,
                    InputStream = stream,
                    ContentType = image.ContentType
                };
                await _s3Client.PutObjectAsync(putRequest);
                var newMenu = new Menu
                {
                    Name = menu.Name,
                    Description = menu.Description,
                    Category = menu.Category,
                    Price = menu.Price,
                    ImageUrl = $"https://storage.czn.my.id/{_bucketName}/{imageKey}",
                    IsAvailable = false
                };
                await menuCollection.InsertOneAsync(newMenu);

                // Create recipe if ingredients are provided
                if (menu.Ingredients.Any())
                {
                    var recipe = new Recipe
                    {
                        MenuId = newMenu.Id,
                        Ingredients = menu.Ingredients
                    };
                    await recipeCollection.InsertOneAsync(recipe);

                    // Check availability based on stock
                    bool isAvailable = true;
                    foreach (var ingredient in menu.Ingredients)
                    {
                        var stock = await stockCollection.Find(s => s.Id == ingredient.StockId).FirstOrDefaultAsync();
                        if (stock == null || stock.Quantity < ingredient.Quantity)
                        {
                            isAvailable = false;
                            break;
                        }
                    }
                    newMenu.IsAvailable = isAvailable;
                    await menuCollection.ReplaceOneAsync(m => m.Id == newMenu.Id, newMenu);
                }

                return newMenu;
            }
            catch (Exception)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Failed to upload image.")
                    .SetCode("IMAGE_UPLOAD_FAILED")
                    .Build());
            }
        }

        // Update Menu
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<Menu> UpdateMenuAsync(string id, UpdateMenuInput menu, [Service] IMongoDatabase database, [GraphQLType(typeof(UploadType))] IFile? image = null)
        {
            var menuCollection = database.GetCollection<Menu>("menus");
            var recipeCollection = database.GetCollection<Recipe>("recipes");
            var stockCollection = database.GetCollection<Stock>("stocks");

            var existingMenu = await menuCollection.Find(m => m.Id == id).FirstOrDefaultAsync();
            if (existingMenu == null)
            {
                throw new GraphQLException(new Error("Menu not found", "MENU_NOT_FOUND"));
            }

            // Update image if provided
            if (image != null)
            {
                try
                {
                    var imageKey = $"menus/{Guid.NewGuid()}_{image.Name}";
                    using var stream = image.OpenReadStream();
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = imageKey,
                        InputStream = stream,
                        ContentType = image.ContentType
                    };
                    await _s3Client.PutObjectAsync(putRequest);
                    existingMenu.ImageUrl = $"https://storage.czn.my.id/{_bucketName}/{imageKey}";
                }
                catch (Exception)
                {
                    throw new GraphQLException(ErrorBuilder.New()
                        .SetMessage("Failed to upload image.")
                        .SetCode("IMAGE_UPLOAD_FAILED")
                        .Build());
                }
            }

            // Update fields
            if (menu.Name != null)
                existingMenu.Name = menu.Name;
            if (menu.Description != null)
                existingMenu.Description = menu.Description;
            if (menu.Category != null)
                existingMenu.Category = menu.Category;
            if (menu.Price.HasValue)
                existingMenu.Price = menu.Price.Value;

            // Update recipe if ingredients provided
            if (menu.Ingredients != null)
            {
                var existingRecipe = await recipeCollection.Find(r => r.MenuId == id).FirstOrDefaultAsync();
                if (existingRecipe != null)
                {
                    existingRecipe.Ingredients = menu.Ingredients;
                    await recipeCollection.ReplaceOneAsync(r => r.Id == existingRecipe.Id, existingRecipe);
                }
                else if (menu.Ingredients.Any())
                {
                    var newRecipe = new Recipe
                    {
                        MenuId = id,
                        Ingredients = menu.Ingredients
                    };
                    await recipeCollection.InsertOneAsync(newRecipe);
                }

                // Check availability
                bool isAvailable = true;
                if (menu.Ingredients.Any())
                {
                    foreach (var ingredient in menu.Ingredients)
                    {
                        var stock = await stockCollection.Find(s => s.Id == ingredient.StockId).FirstOrDefaultAsync();
                        if (stock == null || stock.Quantity < ingredient.Quantity)
                        {
                            isAvailable = false;
                            break;
                        }
                    }
                }
                existingMenu.IsAvailable = isAvailable;
            }

            await menuCollection.ReplaceOneAsync(m => m.Id == id, existingMenu);
            return existingMenu;
        }

        // Delete Menu
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<bool> DeleteMenuAsync(string id, [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Menu>("menus");
            var result = await collection.DeleteOneAsync(m => m.Id == id);
            return result.DeletedCount > 0;
        }
    }
}