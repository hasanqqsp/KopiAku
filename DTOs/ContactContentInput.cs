namespace KopiAku.DTOs
{
    public class ContactContentInput
    {
        public string OperationalHours { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Whatsapp { get; set; } = null!;
        public string Instagram { get; set; } = null!;
        public string GoogleMaps { get; set; } = null!;
    }

    public class UpdateContactContentInput
    {
        public string? OperationalHours { get; set; }
        public string? Address { get; set; }
        public string? Whatsapp { get; set; }
        public string? Instagram { get; set; }
        public string? GoogleMaps { get; set; }
    }
}
