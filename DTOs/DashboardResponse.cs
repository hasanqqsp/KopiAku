namespace KopiAku.DTOs
{
    public class DashboardResponse
    {
        public decimal SalesToday { get; set; }
        public int SalesTodayCount { get; set; }
        public decimal SalesThisMonth { get; set; }
        public int SalesThisMonthCount { get; set; }
        public List<StockStatus> StockStatus { get; set; } = new();
        public List<DailySales> TimeSeriesLast30Days { get; set; } = new();
    }

    public class StockStatus
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
        public string Unit { get; set; } = null!;
    }

    public class DailySales
    {
        public DateTime Date { get; set; }
        public decimal TotalSales { get; set; }
    }
}