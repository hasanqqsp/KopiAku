using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KopiAku.Models
{
    public class ContentManagement
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("content")]
        public string Content { get; set; } = null!;
    }
}