using MongoDB.Driver;
using MongoDB.Bson;
using KopiAku.Models;
using KopiAku.DTOs;
using HotChocolate.Authorization;
using System.Security.Claims;
using System.Linq;

namespace KopiAku.GraphQL
{
    [ExtendObjectType(typeof(Query))]
    public class DashboardQueries
    {
    
        public async Task<DashboardResponse> GetDashboardAsync([Service] IMongoDatabase database)
        {
            var response = new DashboardResponse();

            // Calculate UTC+7 dates
            var utcNow = DateTime.UtcNow;
            var offset = TimeSpan.FromHours(7);
            var nowInTz = utcNow + offset;
            var today = nowInTz.Date;
            var thisMonth = new DateTime(nowInTz.Year, nowInTz.Month, 1);
            var todayStart = today - offset;
            var todayEnd = todayStart + TimeSpan.FromDays(1);
            var monthStart = thisMonth - offset;
            var monthEnd = monthStart.AddMonths(1);

            // Sales today
            var transactionCollection = database.GetCollection<Transaction>("transactions");
            var salesTodayPipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "transactionDate", new BsonDocument { { "$gte", todayStart }, { "$lt", todayEnd } } },
                    
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "total", new BsonDocument("$sum", "$totalAmount") },
                    { "count", new BsonDocument("$sum", 1) }
                })
            };
            var salesTodayResult = await transactionCollection.AggregateAsync<BsonDocument>(salesTodayPipeline);
            var salesTodayDoc = await salesTodayResult.FirstOrDefaultAsync();
            response.SalesToday = salesTodayDoc?["total"].AsDecimal ?? 0;
            response.SalesTodayCount = salesTodayDoc?["count"].AsInt32 ?? 0;

            // Sales this month
            var salesMonthPipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "transactionDate", new BsonDocument { { "$gte", monthStart }, { "$lt", monthEnd } } },
                    
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "total", new BsonDocument("$sum", "$totalAmount") },
                    { "count", new BsonDocument("$sum", 1) }
                })
            };
            var salesMonthResult = await transactionCollection.AggregateAsync<BsonDocument>(salesMonthPipeline);
            var salesMonthDoc = await salesMonthResult.FirstOrDefaultAsync();
            response.SalesThisMonth = salesMonthDoc?["total"].AsDecimal ?? 0;
            response.SalesThisMonthCount = salesMonthDoc?["count"].AsInt32 ?? 0;

            // Stock status
            var stockCollection = database.GetCollection<Stock>("stocks");
            var lowStocks = await stockCollection.Find(s => s.Quantity < s.NotificationThreshold).ToListAsync();
            response.StockStatus = lowStocks.Select(s => new StockStatus
            {
                Id = s.Id,
                Name = s.ItemName,
                Quantity = s.Quantity,
                Unit = s.Unit
            }).ToList();

            // Time series last 30 days
            var thirtyDaysAgo = nowInTz.AddDays(-30).Date - offset;
            var timeSeriesPipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "transactionDate", new BsonDocument { { "$gte", thirtyDaysAgo } } },
                    
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument("$dateToString", new BsonDocument { { "format", "%Y-%m-%d" }, { "date", "$transactionDate" } }) },
                    { "total", new BsonDocument("$sum", "$totalAmount") }
                }),
                new BsonDocument("$sort", new BsonDocument("_id", 1))
            };
            var timeSeriesResult = await transactionCollection.AggregateAsync<BsonDocument>(timeSeriesPipeline);
            var timeSeriesDocs = await timeSeriesResult.ToListAsync();
            var dateDict = timeSeriesDocs.ToDictionary(doc => DateTime.Parse(doc["_id"].AsString), doc => doc["total"].AsDecimal);
            var startDate = nowInTz.AddDays(-29).Date;
            var endDate = nowInTz.Date;
            response.TimeSeriesLast30Days = new List<DailySales>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                response.TimeSeriesLast30Days.Add(new DailySales
                {
                    Date = date,
                    TotalSales = dateDict.TryGetValue(date, out var total) ? total : 0
                });
            }

            return response;
        }
    }
}