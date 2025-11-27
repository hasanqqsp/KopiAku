using MongoDB.Driver;
using KopiAku.Models;
using HotChocolate.Data;
using HotChocolate.Authorization;

namespace KopiAku.GraphQL.ContentsManagement
{
    [ExtendObjectType(typeof(Mutation))]
    public class ContentManagementMutations
    {
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<ContentManagement> CreateContentManagementAsync(
            ContentManagement input,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<ContentManagement>("contents-management");

            await collection.InsertOneAsync(input);
            return input;
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<ContentManagement> UpdateContentManagementAsync(
            string id,
            ContentManagement input,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<ContentManagement>("contents-management");

            var filter = Builders<ContentManagement>.Filter.Eq(c => c.Id, id);
            await collection.ReplaceOneAsync(filter, input);
            return input;
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<bool> DeleteContentManagementAsync(
            string id,
            [Service] IMongoDatabase database)
        {
            var collection = database.GetCollection<ContentManagement>("contents-management");

            var filter = Builders<ContentManagement>.Filter.Eq(c => c.Id, id);
            var result = await collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
    }
}