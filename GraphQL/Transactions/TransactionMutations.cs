using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Authorization;

namespace KopiAku.GraphQL.Transactions
{
    [ExtendObjectType(typeof(Mutation))]
    public class TransactionMutations
    {
        [Authorize]
        public async Task<Transaction> CreateTransactionAsync(
            Transaction transaction,
            [Service] IMongoDatabase database)
        {
            var transactionCollection = database.GetCollection<Transaction>("transactions");
            var menuCollection = database.GetCollection<Menu>("menus");
            var recipeCollection = database.GetCollection<Recipe>("recipes");
            var stockCollection = database.GetCollection<Stock>("stocks");
            var stockLogCollection = database.GetCollection<StockLog>("stock-logs");

            // validate all menu items
            decimal totalAmount = 0;
            var validatedMenuItems = new List<TransactionMenuItem>();
            var stockConsumptions = new List<(string stockId, decimal quantity, string menuName)>();
            
            foreach (var item in transaction.MenuItems)
            {
                var menu = await menuCollection.Find(m => m.Id == item.MenuId).FirstOrDefaultAsync();
                if (menu == null)
                {
                    throw new GraphQLException(new Error($"Menu with ID {item.MenuId} not found", "MENU_NOT_FOUND"));
                }

                if (!menu.IsAvailable)
                {
                    throw new GraphQLException(new Error($"Menu {menu.Name} is not available", "MENU_NOT_AVAILABLE"));
                }

                // Check if we can fulfill the ingredients from stock and prepare consumption data
                var recipe = await recipeCollection.Find(r => r.MenuId == item.MenuId).FirstOrDefaultAsync();
                if (recipe != null)
                {
                    foreach (var ingredient in recipe.Ingredients)
                    {
                        var stockItem = await stockCollection.Find(s => s.Id == ingredient.StockId).FirstOrDefaultAsync();
                        var requiredQuantity = ingredient.Quantity * item.Quantity;
                        
                        if (stockItem == null || stockItem.Quantity < requiredQuantity)
                        {
                            throw new GraphQLException(new Error($"Insufficient stock for ingredient {ingredient.StockId} for menu {menu.Name}", "INSUFFICIENT_STOCK"));
                        }

                        // Add to stock consumption list
                        stockConsumptions.Add((ingredient.StockId, requiredQuantity, menu.Name));
                    }
                }

                totalAmount += menu.Price * item.Quantity;
                validatedMenuItems.Add(new TransactionMenuItem
                {
                    MenuId = item.MenuId,
                    Quantity = item.Quantity,
                });
            }

            // create the transaction
            var newTransaction = new Transaction
            {
                UserId = transaction.UserId,
                MenuItems = validatedMenuItems,
                TotalAmount = totalAmount,
                Status = transaction.Status,
                TransactionDate = DateTime.UtcNow
            };

            await transactionCollection.InsertOneAsync(newTransaction);

             // Now consume the stock and create stock logs
            foreach (var (stockId, consumedQuantity, menuName) in stockConsumptions)
            {
                var stockItem = await stockCollection.Find(s => s.Id == stockId).FirstOrDefaultAsync();
                if (stockItem != null)
                {
                    var beforeQuantity = stockItem.Quantity;
                    stockItem.Quantity -= (int)consumedQuantity; // Convert decimal to int for stock quantity

                    await stockCollection.ReplaceOneAsync(s => s.Id == stockId, stockItem);

                    // Create stock log
                    var stockLog = new StockLog
                    {
                        StockId = stockId,
                        Type = "out",
                        Quantity = (int)consumedQuantity,
                        BeforeQuantity = (int)beforeQuantity,
                        AfterQuantity = stockItem.Quantity,
                        Reason = $"Transaction {newTransaction.Id} - {menuName}",
                        Timestamp = DateTime.UtcNow
                    };

                    await stockLogCollection.InsertOneAsync(stockLog);
                }
            }

            // Update menu availability after stock consumption
            var allRecipes = await recipeCollection.Find(Builders<Recipe>.Filter.Empty).ToListAsync();
            foreach (var recipe in allRecipes)
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

            return newTransaction;
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<Transaction> UpdateTransactionStatusAsync(
            string transactionId,
            string status,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Transaction>("transactions");
            var update = Builders<Transaction>.Update.Set(t => t.Status, status);
            var filter = Builders<Transaction>.Filter.Eq(t => t.Id, transactionId);
            var options = new FindOneAndUpdateOptions<Transaction>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updatedTransaction = await collection.FindOneAndUpdateAsync(filter, update, options);
            if (updatedTransaction == null)
            {
                throw new GraphQLException(new Error("Transaction not found", "TRANSACTION_NOT_FOUND"));
            }

            return updatedTransaction;
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<bool> DeleteTransactionAsync(
            string transactionId,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Transaction>("transactions");
            var result = await collection.DeleteOneAsync(t => t.Id == transactionId);
            return result.DeletedCount > 0;
        }
    }
}