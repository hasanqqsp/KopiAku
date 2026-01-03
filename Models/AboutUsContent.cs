using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KopiAku.Models
{
    public class AboutUsContent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("paragraph1")]
        public string Paragraph1 { get; set; } = null!;

        [BsonElement("paragraph2")]
        public string Paragraph2 { get; set; } = null!;

        [BsonElement("vision")]
        public string Vision { get; set; } = null!;

        [BsonElement("mission")]
        public List<string> Mission { get; set; } = new List<string>();

        [BsonElement("background")]
        public string Background { get; set; } = null!;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
