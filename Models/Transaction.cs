using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KopiAku.Models
{
    public class Transaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = null!;

        [BsonElement("menuItems")]
        public List<TransactionMenuItem> MenuItems { get; set; } = new();

        [BsonElement("totalAmount")]
        public decimal TotalAmount { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = null!;

        [BsonElement("transactionDate")]
        public DateTime TransactionDate { get; set; }

        [BsonElement("qrisTransactionTime")]
        public DateTime? QrisTransactionTime { get; set; }

        [BsonElement("qrisOrderId")]
        public string? QrisOrderId { get; set; }

        [BsonElement("netAmount")]
        public decimal? NetAmount { get; set; }
    }
    public class TransactionMenuItem
    {
        [BsonElement("menuId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MenuId { get; set; } = null!;

        [BsonElement("quantity")]
        public int Quantity { get; set; }
    }
}