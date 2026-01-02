using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;
using MongoDB.Driver;
using System.Security.Claims;
using HotChocolate;
using KopiAku.DTOs;

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

        [Authorize]
        public async Task<Transaction?> GetTransactionByIdAsync(string id, [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Transaction>("transactions");
            return await collection.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        [Authorize]
        [UsePaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        [UseSorting]
        public IExecutable<Transaction> GetTransactionsByUserId(string userId, [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Transaction>("transactions");
            return collection.Find(t => t.UserId == userId).AsExecutable();
        }

        [Authorize]
        public async Task<KopiAku.DTOs.TransactionStatusResponse> GetTransactionsByStatus(string status, List<string> qrisOrderIds, [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Transaction>("transactions");
            
            // Find transactions with the specified status 
    
            var transactions = await collection.Find(t => t.Status == status).ToListAsync();

            
            // Find all distinct qrisOrderIds that exist in the database from the parameter list
            var existingFilter = Builders<Transaction>.Filter.In(t => t.QrisOrderId, qrisOrderIds.Where(id => id != null));
            var existingIds = await collection.Distinct<string>("qrisOrderId", existingFilter).ToListAsync();
            
            return new KopiAku.DTOs.TransactionStatusResponse
            {
                Transactions = transactions,
                ExistingQrisOrderIds = existingIds.Where(id => id != null).ToList()!
            };
        }
    }

    [ExtendObjectType(typeof(Transaction))]
    public class TransactionResolvers
    {
        public async Task<User?> GetUser([Parent] Transaction transaction, [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<User>("users");
            return await collection.Find(u => u.Id == transaction.UserId).FirstOrDefaultAsync();
        }
    }

    [ExtendObjectType(typeof(TransactionMenuItem))]
    public class TransactionMenuItemResolvers
    {
        public async Task<decimal> GetUnitPrice([Parent] TransactionMenuItem item, [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Menu>("menus");
            var menu = await collection.Find(m => m.Id == item.MenuId).FirstOrDefaultAsync();
            return menu?.Price ?? 0;
        }

        public async Task<string?> GetName([Parent] TransactionMenuItem item, [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<Menu>("menus");
            var menu = await collection.Find(m => m.Id == item.MenuId).FirstOrDefaultAsync();
            return menu?.Name;
        }
    }
}