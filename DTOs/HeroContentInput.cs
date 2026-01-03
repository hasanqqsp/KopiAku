namespace KopiAku.DTOs
{
    public class HeroContentInput
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
    }

    public class UpdateHeroContentInput
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
