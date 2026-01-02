using KopiAku.Models;

namespace KopiAku.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ProfilePictureUrl { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; } = null!;
        public Presence? Presence { get; set; }
    }
}