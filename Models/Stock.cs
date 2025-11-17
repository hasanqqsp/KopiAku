using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KopiAku.Models
{
    public class Stock
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("itemName")]
        public string ItemName { get; set; } = null!;

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("unit")]
        public string Unit { get; set; } = null!;

        [BsonElement("notificationThreshold")]
        public int NotificationThreshold { get; set; }
    }
}