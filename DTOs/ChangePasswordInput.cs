namespace KopiAku.DTOs
{
    public class ChangePasswordInput
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}