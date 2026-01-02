using KopiAku.Models;

namespace KopiAku.DTOs
{
    public class RegisterResponse
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? Contact { get; set; }
        public bool IsActive { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public Presence? Presence { get; set; }
    }
}