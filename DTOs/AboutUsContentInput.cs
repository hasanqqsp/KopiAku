namespace KopiAku.DTOs
{
    public class AboutUsContentInput
    {
        public string Paragraph1 { get; set; } = null!;
        public string Paragraph2 { get; set; } = null!;
        public string Vision { get; set; } = null!;
        public List<string> Mission { get; set; } = new List<string>();
        public string Background { get; set; } = null!;
    }

    public class UpdateAboutUsContentInput
    {
        public string? Paragraph1 { get; set; }
        public string? Paragraph2 { get; set; }
        public string? Vision { get; set; }
        public List<string>? Mission { get; set; }
        public string? Background { get; set; }
    }
}
