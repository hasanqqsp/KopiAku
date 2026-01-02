using HotChocolate;

namespace KopiAku.DTOs
{
    [GraphQLName("BatchUpdateInput")]
    public class StockBatchUpdateInput
    {
        public string StockId { get; set; } = null!;
        public int Quantity { get; set; }
    }
}