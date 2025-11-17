using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KopiAku.Models
{
    public class StockLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("stockId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string StockId { get; set; } = null!;

        [BsonElement("type")]
        public string Type { get; set; } = null!; // "in" or "out"

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("beforeQuantity")]
        public int BeforeQuantity { get; set; }

        [BsonElement("afterQuantity")]
        public int AfterQuantity { get; set; }

        [BsonElement("reason")]
        public string Reason { get; set; } = null!;

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}