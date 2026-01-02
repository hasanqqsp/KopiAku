using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KopiAku.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("username")]
        public string Username { get; set; } = null!;

        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("role")]
        public string Role { get; set; } = null!;

        [BsonElement("contact")]
        public string? Contact { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = false;

        [BsonElement("profilePictureUrl")]
        public string? ProfilePictureUrl { get; set; }

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = null!;
    }
}