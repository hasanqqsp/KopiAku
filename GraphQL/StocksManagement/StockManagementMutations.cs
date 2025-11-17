using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate;
using HotChocolate.Authorization;
using System.Security.Claims;

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
            var collection = database.GetCollection<Stock>("stocks");

            var newStock = new Stock
            {
                ItemName = itemName,
                Quantity = quantity,
                Unit = unit,
                NotificationThreshold = notificationThreshold
            };

            await collection.InsertOneAsync(newStock);
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
            var stock = await stockCollection.Find(s => s.Id == stockId).FirstOrDefaultAsync() ?? throw new GraphQLException(new Error("Stock not found", "STOCK_NOT_FOUND"));
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
            return stockLog;
        }

        // Delete a stock item
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<bool> DeleteStockAsync(string stockId, [Service] IMongoDatabase database)
        {
            var stockCollection = database.GetCollection<Stock>("stocks");
            var stockLogCollection = database.GetCollection<StockLog>("stock-logs");

            var stock = await stockCollection.Find(s => s.Id == stockId).FirstOrDefaultAsync() ?? throw new GraphQLException(new Error("Stock not found", "STOCK_NOT_FOUND"));
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

            var stock = await stockCollection.Find(s => s.Id == stockId).FirstOrDefaultAsync() ?? throw new GraphQLException(new Error("Stock not found", "STOCK_NOT_FOUND"));

            if (itemName != null)
                stock.ItemName = itemName;
            if (quantity.HasValue)
                stock.Quantity = quantity.Value;
            if (unit != null)
                stock.Unit = unit;
            if (notificationThreshold.HasValue)
                stock.NotificationThreshold = notificationThreshold.Value;

            await stockCollection.ReplaceOneAsync(s => s.Id == stockId, stock);
            return stock;
        }
    }
}