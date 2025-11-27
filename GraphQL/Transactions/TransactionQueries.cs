using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;
using MongoDB.Driver;

namespace KopiAku.GraphQL.Transactions
{
    [ExtendObjectType(typeof(Query))]
    public class TransactionQueries
    {
        [Authorize]
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseFiltering]
        [UseSorting]
        public IExecutable<Transaction> GetTransactions([Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Transaction>("transactions");
            return collection.AsExecutable();
        }
    }
}