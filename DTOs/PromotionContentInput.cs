namespace KopiAku.DTOs
{
    public class PromotionContentInput
    {
        public string Title { get; set; } = null!;
        public List<string> Rules { get; set; } = new List<string>();
    }

    public class UpdatePromotionContentInput
    {
        public string? Title { get; set; }
        public List<string>? Rules { get; set; }
    }
}
