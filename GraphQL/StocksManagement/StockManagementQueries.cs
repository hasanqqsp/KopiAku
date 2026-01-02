using MongoDB.Driver;
using MongoDB.Bson;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;

namespace KopiAku.GraphQL.StocksManagement
{
    [ExtendObjectType(typeof(Query))]
    public class StockManagementQueries
    {
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<Stock> GetStocks([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Stock>("stocks");
            return collection.AsExecutable();
        }

        [Authorize(Roles = new[] { "Admin" })]
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<StockLog> GetStockLogs([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<StockLog>("stock-logs");
            return collection.AsExecutable();
        }
    }

    [ExtendObjectType(typeof(Stock))]
    public class StockResolvers
    {
        public string Name([Parent] Stock stock) => stock.ItemName;

        public int CurrentStock([Parent] Stock stock) => stock.Quantity;

        public async Task<int> UsedToday([Parent] Stock stock, [Service] IMongoDatabase database)
        {
            var utcNow = DateTime.UtcNow;
            var offset = TimeSpan.FromHours(7);
            var nowInTz = utcNow + offset;
            var today = nowInTz.Date;
            var todayStart = today - offset;
            var todayEnd = todayStart + TimeSpan.FromDays(1);

            var logCollection = database.GetCollection<StockLog>("stock-logs");
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "stockId", stock.Id },
                    { "type", "out" },
                    { "timestamp", new BsonDocument { { "$gte", todayStart }, { "$lt", todayEnd } } }
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "total", new BsonDocument("$sum", "$quantity") }
                })
            };
            var result = await logCollection.AggregateAsync<BsonDocument>(pipeline);
            var doc = await result.FirstOrDefaultAsync();
            return doc?["total"].AsInt32 ?? 0;
        }

        public async Task<int> UsedThisMonth([Parent] Stock stock, [Service] IMongoDatabase database)
        {
            var utcNow = DateTime.UtcNow;
            var offset = TimeSpan.FromHours(7);
            var nowInTz = utcNow + offset;
            var thisMonth = new DateTime(nowInTz.Year, nowInTz.Month, 1);
            var monthStart = thisMonth - offset;
            var monthEnd = monthStart.AddMonths(1);

            var logCollection = database.GetCollection<StockLog>("stock-logs");
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "stockId", stock.Id },
                    { "type", "out" },
                    { "timestamp", new BsonDocument { { "$gte", monthStart }, { "$lt", monthEnd } } }
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "total", new BsonDocument("$sum", "$quantity") }
                })
            };
            var result = await logCollection.AggregateAsync<BsonDocument>(pipeline);
            var doc = await result.FirstOrDefaultAsync();
            return doc?["total"].AsInt32 ?? 0;
        }

        public string Status([Parent] Stock stock)
        {
            if (stock.Quantity <= 0)
                return "Out Of Stock";
            else if (stock.Quantity < stock.NotificationThreshold)
                return "Low";
            else
                return "Normal";
        }
    }

    [ExtendObjectType(typeof(StockLog))]
    public class StockLogResolvers
    {
        public async Task<Stock?> GetStock([Parent] StockLog stockLog, [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Stock>("stocks");
            return await collection.Find(s => s.Id == stockLog.StockId).FirstOrDefaultAsync();
        }
    }
}