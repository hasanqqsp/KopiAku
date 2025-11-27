using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;

namespace KopiAku.GraphQL.StocksManagement
{
    [ExtendObjectType(typeof(Query))]
    public class StockManagementQueries
    {
        [Authorize(Roles = new[] { "Admin" })]
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
}