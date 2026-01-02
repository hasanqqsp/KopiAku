using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KopiAku.Models
{
    public class Presence
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = null!;

        [BsonElement("imageUrl")]
        public string ImageUrl { get; set; } = null!;

        [BsonElement("checkInTime")]
        public DateTime CheckInTime { get; set; }

        [BsonElement("checkOutTime")]
        public DateTime CheckOutTime { get; set; }

        [BsonElement("validated")]
        public bool Validated { get; set; }
    }
}