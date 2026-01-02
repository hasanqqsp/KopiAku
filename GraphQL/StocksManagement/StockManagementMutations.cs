using MongoDB.Driver;
using KopiAku.Models;
using KopiAku.DTOs;
using HotChocolate.Authorization;
using System.Collections.Generic;
using System;

namespace KopiAku.GraphQL.StocksManagement
{
    [ExtendObjectType(typeof(Mutation))]
    public class StockManagementMutations
    {
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<Stock> AddStockAsync(
            string itemName,
            int quantity,
            string unit,
            int notificationThreshold,
            [Service] IMongoDatabase database)
        {
            var menuCollection = database.GetCollection<Menu>("menus");
            var stockCollection = database.GetCollection<Stock>("stocks");
            var recipeCollection = database.GetCollection<Recipe>("recipes");
            var stockLogCollection = database.GetCollection<StockLog>("stock-logs");

            var newStock = new Stock
            {
                ItemName = itemName,
                Quantity = quantity,
                Unit = unit,
                NotificationThreshold = notificationThreshold
            };
            await stockCollection.InsertOneAsync(newStock);

            // Log the initial stock addition
            var stockLog = new StockLog
            {
                StockId = newStock.Id,
                Type = "in",
                Quantity = quantity,
                BeforeQuantity = 0,
                AfterQuantity = quantity,
                Reason = "Initial stock addition",
                Timestamp = DateTime.UtcNow
            };
            await stockLogCollection.InsertOneAsync(stockLog);

            // Update menu availability based on new stock
            var recipes = await recipeCollection.Find(_ => true).ToListAsync();
            foreach (var recipe in recipes)
            {
                bool isAvailable = true;
                foreach (var ingredient in recipe.Ingredients)
                {
                    var stockItem = await stockCollection.Find(s => s.Id == ingredient.StockId).FirstOrDefaultAsync();
                    if (stockItem == null || stockItem.Quantity < ingredient.Quantity)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                var menu = await menuCollection.Find(m => m.Id == recipe.MenuId).FirstOrDefaultAsync();
                
                if (menu != null)
                {
                    menu.IsAvailable = isAvailable;
                    await menuCollection.ReplaceOneAsync(m => m.Id == menu.Id, menu);
                }
            }

            return newStock;
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<StockLog> StockInAsync(
            string stockId,
            int quantity,
            string reason,
            [Service] IMongoDatabase database)
        {
            var stockCollection = database.GetCollection<Stock>("stocks");
            var stockLogCollection = database.GetCollection<StockLog>("stock-logs");
            var menuCollection = database.GetCollection<Menu>("menus");
            var recipeCollection = database.GetCollection<Recipe>("recipes");

            var stock = await stockCollection.Find(s => s.Id == stockId).FirstOrDefaultAsync() ?? throw new GraphQLException(new Error("Stock not found", "STOCK_NOT_FOUND"));
            var beforeQuantity = stock.Quantity;
            stock.Quantity += quantity;

            await stockCollection.ReplaceOneAsync(s => s.Id == stockId, stock);

            var stockLog = new StockLog
            {
                StockId = stockId,
                Type = "in",
                Quantity = quantity,
                BeforeQuantity = beforeQuantity,
                AfterQuantity = stock.Quantity,
                Reason = reason,
                Timestamp = DateTime.UtcNow
            };

            await stockLogCollection.InsertOneAsync(stockLog);

            // Update menu availability based on updated stock
            var recipes = await recipeCollection.Find(_ => true).ToListAsync();
            foreach (var recipe in recipes)
            {
                bool isAvailable = true;
                foreach (var ingredient in recipe.Ingredients)
                {
                    var stockItem = await stockCollection.Find(s => s.Id == ingredient.StockId).FirstOrDefaultAsync();
                    if (stockItem == null || stockItem.Quantity < ingredient.Quantity)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                var menu = await menuCollection.Find(m => m.Id == recipe.MenuId).FirstOrDefaultAsync();

                if (menu != null)
                {
                    menu.IsAvailable = isAvailable;
                    await menuCollection.ReplaceOneAsync(m => m.Id == menu.Id, menu);
                }
            }

            return stockLog;
        }

        // Stock out mutation
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<StockLog> StockOutAsync(
            string stockId,
            int quantity,
            string reason,
            [Service] IMongoDatabase database)
        {
            var stockCollection = database.GetCollection<Stock>("stocks");
            var stockLogCollection = database.GetCollection<StockLog>("stock-logs");
            var menuCollection = database.GetCollection<Menu>("menus");
            var recipeCollection = database.GetCollection<Recipe>("recipes");

            var stock = await stockCollection.Find(s => s.Id == stockId).FirstOrDefaultAsync() ?? throw new GraphQLException(new Error("Stock not found", "STOCK_NOT_FOUND"));

            if (stock.Quantity < quantity)
            {
                throw new GraphQLException(new Error("Insufficient stock quantity", "INSUFFICIENT_STOCK"));
            }

            var beforeQuantity = stock.Quantity;
            stock.Quantity -= quantity;

            await stockCollection.ReplaceOneAsync(s => s.Id == stockId, stock);

            var stockLog = new StockLog
            {
                StockId = stockId,
                Type = "out",
                Quantity = quantity,
                BeforeQuantity = beforeQuantity,
                AfterQuantity = stock.Quantity,
                Reason = reason,
                Timestamp = DateTime.UtcNow
            };

            await stockLogCollection.InsertOneAsync(stockLog);

            // Update menu availability based on updated stock
            var recipes = await recipeCollection.Find(_ => true).ToListAsync();
            foreach (var recipe in recipes)
            {
                bool isAvailable = true;
                foreach (var ingredient in recipe.Ingredients)
                {
                    var stockItem = await stockCollection.Find(s => s.Id == ingredient.StockId).FirstOrDefaultAsync();
                    if (stockItem == null || stockItem.Quantity < ingredient.Quantity)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                var menu = await menuCollection.Find(m => m.Id == recipe.MenuId).FirstOrDefaultAsync();

                if (menu != null)
                {
                    menu.IsAvailable = isAvailable;
                    await menuCollection.ReplaceOneAsync(m => m.Id == menu.Id, menu);
                }
            }

            return stockLog;
        }

        // Delete a stock item
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<bool> DeleteStockAsync(string stockId, [Service] IMongoDatabase database)
        {
            var stockCollection = database.GetCollection<Stock>("stocks");
            var stockLogCollection = database.GetCollection<StockLog>("stock-logs");
            var menuCollection = database.GetCollection<Menu>("menus");
            var recipeCollection = database.GetCollection<Recipe>("recipes");

            var stock = await stockCollection.Find(s => s.Id == stockId).FirstOrDefaultAsync() ?? throw new GraphQLException(new Error("Stock not found", "STOCK_NOT_FOUND"));

            // Check if the stock is used in any recipe
            var recipesUsingStock = await recipeCollection.Find(r => r.Ingredients.Any(i => i.StockId == stockId)).ToListAsync();
            if (recipesUsingStock.Count > 0)
            {
                throw new GraphQLException(new Error("Cannot delete stock item as it is used in one or more recipes", "STOCK_IN_USE"));
            }

            await stockCollection.DeleteOneAsync(s => s.Id == stockId);
            await stockLogCollection.DeleteManyAsync(sl => sl.StockId == stockId);

            return true;
        }

        // Update stock details
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<Stock> UpdateStockAsync(
            string stockId,
            string? itemName,
            int? quantity,
            string? unit,
            int? notificationThreshold,
            [Service] IMongoDatabase database)
        {
            var stockCollection = database.GetCollection<Stock>("stocks");
            var menuCollection = database.GetCollection<Menu>("menus");
            var recipeCollection = database.GetCollection<Recipe>("recipes");

            var stock = await stockCollection.Find(s => s.Id == stockId).FirstOrDefaultAsync() ?? throw new GraphQLException(new Error("Stock not found", "STOCK_NOT_FOUND"));

            if (itemName != null)
                stock.ItemName = itemName;
            if (quantity.HasValue)
                stock.Quantity = quantity.Value;
            if (unit != null)
                stock.Unit = unit;
            if (notificationThreshold.HasValue)
                stock.NotificationThreshold = notificationThreshold.Value;

            // Update menu availability based on updated stock
            var recipes = await recipeCollection.Find(_ => true).ToListAsync();
            foreach (var recipe in recipes)
            {
                bool isAvailable = true;
                foreach (var ingredient in recipe.Ingredients)
                {
                    var stockItem = await stockCollection.Find(s => s.Id == ingredient.StockId).FirstOrDefaultAsync();
                    if (stockItem == null || stockItem.Quantity < ingredient.Quantity)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                var menu = await menuCollection.Find(m => m.Id == recipe.MenuId).FirstOrDefaultAsync();

                if (menu != null)
                {
                    menu.IsAvailable = isAvailable;
                    await menuCollection.ReplaceOneAsync(m => m.Id == menu.Id, menu);
                }
            }

            await stockCollection.ReplaceOneAsync(s => s.Id == stockId, stock);
            return stock;
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<List<Stock>> BatchUpdateStocksAsync(
            List<StockBatchUpdateInput> updates,
            [Service] IMongoDatabase database)
        {
            var stockCollection = database.GetCollection<Stock>("stocks");
            var menuCollection = database.GetCollection<Menu>("menus");
            var recipeCollection = database.GetCollection<Recipe>("recipes");
            var stockLogCollection = database.GetCollection<StockLog>("stock-logs");

            var updatedStocks = new List<Stock>();

            foreach (var update in updates)
            {
                var stock = await stockCollection.Find(s => s.Id == update.StockId).FirstOrDefaultAsync();
                if (stock == null)
                {
                    throw new GraphQLException(new Error($"Stock with ID {update.StockId} not found", "STOCK_NOT_FOUND"));
                }

                var beforeQuantity = stock.Quantity;
                stock.Quantity = update.Quantity;

                await stockCollection.ReplaceOneAsync(s => s.Id == update.StockId, stock);
                updatedStocks.Add(stock);

                // Log the stock update
                var stockLog = new StockLog
                {
                    StockId = update.StockId,
                    Type = update.Quantity > beforeQuantity ? "in" : "out",
                    Quantity = Math.Abs(update.Quantity - beforeQuantity),
                    BeforeQuantity = beforeQuantity,
                    AfterQuantity = update.Quantity,
                    Reason = "Batch stock update",
                    Timestamp = DateTime.UtcNow
                };
                await stockLogCollection.InsertOneAsync(stockLog);
            }

            // Update menu availability based on updated stocks
            var recipes = await recipeCollection.Find(_ => true).ToListAsync();
            foreach (var recipe in recipes)
            {
                bool isAvailable = true;
                foreach (var ingredient in recipe.Ingredients)
                {
                    var stock = await stockCollection.Find(s => s.Id == ingredient.StockId).FirstOrDefaultAsync();
                    if (stock == null || stock.Quantity < ingredient.Quantity)
                    {
                        isAvailable = false;
                        break;
                    }
                }
                var update = Builders<Menu>.Update.Set(m => m.IsAvailable, isAvailable);
                await menuCollection.UpdateOneAsync(m => m.Id == recipe.MenuId, update);
            }

            return updatedStocks;
        }
    }
}