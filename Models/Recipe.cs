using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KopiAku.Models
{
    public class Recipe
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("menuId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MenuId { get; set; } = null!;

        [BsonElement("ingredients")]
        public List<RecipeIngredient> Ingredients { get; set; } = new();

    }

    public class RecipeIngredient
    {
        [BsonElement("stockId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string StockId { get; set; } = null!;

        [BsonElement("quantity")]
        public decimal Quantity { get; set; }
    }
}