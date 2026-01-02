using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KopiAku.Models
{
    public class Menu
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("category")]
        public string Category { get; set; } = null!;

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("imageUrl")]
        public string ImageUrl { get; set; } = null!;

        [BsonElement("isAvailable")]
        public bool IsAvailable { get; set; }
    }
}