namespace pharmacystock.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalBrands { get; set; }
        public int TotalCategories { get; set; }
        public int TotalStock { get; set; }
        public List<Product> LowStoctProducts { get; set; } = new();

        public List<StockTransaction> RecentTransactions { get; set; } = new();

        public List<string> Last7Days { get; set; } = new();

        public List<int> StockInData { get; set; } = new();

        public List<int> StockOutData { get; set; } = new();

        public List<string> TopProductNames { get; set; } = new();

        public List<int> TopProductTransactionCounts { get; set; } = new();

        public List<Product> OutOfStockProducts { get; set; } = new();

        public List<Product> ExpiringProducts { get; set; } = new();

        public List<Product> ExpiredProducts { get; set; } = new();

        // Dashboard Kartları
        public int TotalStockIn { get; set; }

        public int TotalStockOut { get; set; }

        // Son Eklenen Ürünler
        public List<Product> NewProducts { get; set; } = new();

        // Sistem Uyarıları
        public int CriticalStockCount { get; set; }

        public int OutOfStockCount { get; set; }

        public int ExpiringSoonCount { get; set; }
    }



}