using Microsoft.EntityFrameworkCore;
using pharmacystock.Models;
using pharmacystock.Models.ViewModels;
using pharmacystock.Services.Interfaces;
using PharmacyStock.Data;

namespace pharmacystock.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var today = DateTime.Today;
            var in90Days = today.AddDays(90);

            // Kritik stok
            var lowStockProducts = await _context.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Where(x => x.CurrentStock <= x.MinimumStock && x.CurrentStock > 0)
                .ToListAsync();

            // Stokta olmayanlar
            var outOfStockProducts = await _context.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Where(x => x.CurrentStock == 0)
                .ToListAsync();

            // SKT yaklaşanlar
            var expiringProducts = await _context.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Where(x => x.ExpirationDate >= today &&
                            x.ExpirationDate <= in90Days)
                .ToListAsync();

            // Süresi geçmişler
            var expiredProducts = await _context.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Where(x => x.ExpirationDate < today)
                .ToListAsync();

            // En çok işlem gören ürünler
            var topProducts = await _context.StockTransactions
                .Include(x => x.Product)
                .Where(x => x.Product != null)
                .GroupBy(x => x.Product!.Name)
                .Select(g => new
                {
                    ProductName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                // İstatistik Kartları
                TotalProducts = await _context.Products.CountAsync(),

                TotalBrands = await _context.Brands.CountAsync(),

                TotalCategories = await _context.Categories.CountAsync(),

                TotalStock = await _context.Products.SumAsync(x => x.CurrentStock),

                // Son Hareketler
                RecentTransactions = await _context.StockTransactions
                    .Include(x => x.Product)
                    .OrderByDescending(x => x.TransactionDate)
                    .Take(5)
                    .ToListAsync(),

                // Depo Özeti
                TotalStockIn = await _context.StockTransactions
                    .Where(x => x.Type == TransactionType.In)
                    .SumAsync(x => (int?)x.Quantity) ?? 0,

                TotalStockOut = await _context.StockTransactions
                    .Where(x => x.Type == TransactionType.Out)
                    .SumAsync(x => (int?)x.Quantity) ?? 0,

                // Son Eklenen Ürünler
                NewProducts = await _context.Products
                    .Include(x => x.Brand)
                    .Include(x => x.Category)
                    .OrderByDescending(x => x.Id)
                    .Take(5)
                    .ToListAsync(),

                // Listeler
                LowStoctProducts = lowStockProducts,

                OutOfStockProducts = outOfStockProducts,

                ExpiringProducts = expiringProducts,

                ExpiredProducts = expiredProducts,

                // Sayaçlar
                CriticalStockCount = lowStockProducts.Count,

                OutOfStockCount = outOfStockProducts.Count,

                ExpiringSoonCount = expiringProducts.Count,

                // Grafik
                TopProductNames = topProducts
                    .Select(x => x.ProductName)
                    .ToList(),

                TopProductTransactionCounts = topProducts
                    .Select(x => x.Count)
                    .ToList()
            };

            // ==========================
            // Son 7 Günlük Grafik
            // ==========================

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i))
                .ToList();

            model.Last7Days = last7Days
                .Select(x => x.ToString("dd.MM"))
                .ToList();

            var firstDay = last7Days.First();

            var transactions = await _context.StockTransactions
                .Where(x => x.TransactionDate >= firstDay)
                .ToListAsync();
            foreach (var day in last7Days)
            {
                var stockIn = transactions
                    .Where(x => x.TransactionDate.Date == day.Date &&
                                x.Type == TransactionType.In)
                    .Sum(x => x.Quantity);

                var stockOut = transactions
                    .Where(x => x.TransactionDate.Date == day.Date &&
                                x.Type == TransactionType.Out)
                    .Sum(x => x.Quantity);

                model.StockInData.Add(stockIn);
                model.StockOutData.Add(stockOut);
            }

            return model;
        }
    }
}
