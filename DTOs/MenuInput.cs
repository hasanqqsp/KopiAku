using KopiAku.Models;

namespace KopiAku.DTOs
{
    public class CreateMenuInput
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Category { get; set; } = null!;
        public decimal Price { get; set; }
        public List<RecipeIngredient> Ingredients { get; set; } = new();
    }

    public class UpdateMenuInput
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public decimal? Price { get; set; }
        public List<RecipeIngredient>? Ingredients { get; set; }
    }
}