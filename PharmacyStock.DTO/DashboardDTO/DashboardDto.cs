using System.Collections.Generic;

namespace PharmacyStock.DTO.DashboardDTO
{
    public class DashboardDto
    {
        // =====================================================
        // İSTATİSTİK KARTLARI
        // =====================================================

        public int TotalProducts { get; set; }

        public int TotalBrands { get; set; }

        public int TotalCategories { get; set; }

        public int TotalStock { get; set; }


        // =====================================================
        // DEPO ÖZETİ
        // =====================================================

        public int TotalStockIn { get; set; }

        public int TotalStockOut { get; set; }


        // =====================================================
        // SAYAÇLAR
        // =====================================================

        public int CriticalStockCount { get; set; }

        public int OutOfStockCount { get; set; }

        public int ExpiringSoonCount { get; set; }

        public int ExpiredCount { get; set; }


        // =====================================================
        // ÜRÜN LİSTELERİ
        // =====================================================

        public List<DashboardProductDto> LowStockProducts { get; set; }
            = new();

        public List<DashboardProductDto> OutOfStockProducts { get; set; }
            = new();

        public List<DashboardProductDto> ExpiringProducts { get; set; }
            = new();

        public List<DashboardProductDto> ExpiredProducts { get; set; }
            = new();

        public List<DashboardProductDto> NewProducts { get; set; }
            = new();


        // =====================================================
        // SON HAREKETLER
        // =====================================================

        public List<DashboardTransactionDto> RecentTransactions { get; set; }
            = new();


        // =====================================================
        // EN ÇOK İŞLEM GÖREN ÜRÜNLER
        // =====================================================

        public List<string> TopProductNames { get; set; }
            = new();

        public List<int> TopProductTransactionCounts { get; set; }
            = new();


        // =====================================================
        // SON 7 GÜN GRAFİĞİ
        // =====================================================

        public List<string> Last7Days { get; set; }
            = new();

        public List<int> StockInData { get; set; }
            = new();

        public List<int> StockOutData { get; set; }
            = new();
    }
}