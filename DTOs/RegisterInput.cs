namespace KopiAku.DTOs
{
    public class RegisterInput
    {
        public string Name { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Contact { get; set; }
    }
}