using KopiAku.Models;

namespace KopiAku.DTOs
{
    public class TransactionInput
    {
        public string UserId { get; set; } = null!;
        public List<TransactionMenuItemInput> MenuItems { get; set; } = new();
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = null!;

        public DateTime TransactionDate { get; set; }
    }
    public class TransactionMenuItemInput
    {
        public string MenuId { get; set; } = null!;
        public int Quantity { get; set; }
    }

    public class TransactionStatusResponse
    {
        public List<Transaction> Transactions { get; set; } = new();
        public List<string> ExistingQrisOrderIds { get; set; } = new();
    }

    public class ReconciliationItemInput
    {
        public string? TransactionId { get; set; }
        public string QrisOrderId { get; set; } = null!;
        public DateTime QrisTransactionTime { get; set; }
        public decimal NetAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; } = null!;
    }
}