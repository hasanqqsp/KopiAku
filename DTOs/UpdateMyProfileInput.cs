namespace KopiAku.DTOs
{
    public class UpdateMyProfileInput
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Contact { get; set; }
    }
}