using Microsoft.EntityFrameworkCore;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.DataAccess.Repositories.Interfaces;
using PharmacyStock.DTO.DashboardDTO;
using PharmacyStock.Entities.Enum;
using PharmacyStock.Entities.Models;

namespace PharmacyStock.Business.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            var today = DateTime.Today;
            var in90Days = today.AddDays(90);


            // =====================================================
            // PRODUCT QUERY
            // =====================================================

            var productQuery = _unitOfWork.Products
                .GetAll()
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Brand);


            // =====================================================
            // KRİTİK STOK
            // =====================================================

            var lowStockProducts = await productQuery
                .Where(x =>
                    x.CurrentStock <= x.MinimumStock &&
                    x.CurrentStock > 0)
                .Select(x => new DashboardProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Barcode = x.Barcode,

                    CategoryName = x.Category != null
                        ? x.Category.Name
                        : string.Empty,

                    BrandName = x.Brand != null
                        ? x.Brand.Name
                        : string.Empty,

                    Price = x.Price,
                    CurrentStock = x.CurrentStock,
                    MinimumStock = x.MinimumStock,
                    ExpirationDate = x.ExpirationDate
                })
                .ToListAsync();


            // =====================================================
            // TÜKENEN ÜRÜNLER
            // =====================================================

            var outOfStockProducts = await productQuery
                .Where(x => x.CurrentStock == 0)
                .Select(x => new DashboardProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Barcode = x.Barcode,

                    CategoryName = x.Category != null
                        ? x.Category.Name
                        : string.Empty,

                    BrandName = x.Brand != null
                        ? x.Brand.Name
                        : string.Empty,

                    Price = x.Price,
                    CurrentStock = x.CurrentStock,
                    MinimumStock = x.MinimumStock,
                    ExpirationDate = x.ExpirationDate
                })
                .ToListAsync();


            // =====================================================
            // SKT YAKLAŞAN
            // =====================================================

            var expiringProducts = await productQuery
                .Where(x =>
                    x.ExpirationDate >= today &&
                    x.ExpirationDate <= in90Days)
                .OrderBy(x => x.ExpirationDate)
                .Select(x => new DashboardProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Barcode = x.Barcode,

                    CategoryName = x.Category != null
                        ? x.Category.Name
                        : string.Empty,

                    BrandName = x.Brand != null
                        ? x.Brand.Name
                        : string.Empty,

                    Price = x.Price,
                    CurrentStock = x.CurrentStock,
                    MinimumStock = x.MinimumStock,
                    ExpirationDate = x.ExpirationDate
                })
                .ToListAsync();


            // =====================================================
            // SKT GEÇMİŞ
            // =====================================================

            var expiredProducts = await productQuery
                .Where(x => x.ExpirationDate < today)
                .OrderBy(x => x.ExpirationDate)
                .Select(x => new DashboardProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Barcode = x.Barcode,

                    CategoryName = x.Category != null
                        ? x.Category.Name
                        : string.Empty,

                    BrandName = x.Brand != null
                        ? x.Brand.Name
                        : string.Empty,

                    Price = x.Price,
                    CurrentStock = x.CurrentStock,
                    MinimumStock = x.MinimumStock,
                    ExpirationDate = x.ExpirationDate
                })
                .ToListAsync();


            // =====================================================
            // SON EKLENEN 5 ÜRÜN
            // =====================================================

            var newProducts = await productQuery
                .OrderByDescending(x => x.Id)
                .Take(5)
                .Select(x => new DashboardProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Barcode = x.Barcode,

                    CategoryName = x.Category != null
                        ? x.Category.Name
                        : string.Empty,

                    BrandName = x.Brand != null
                        ? x.Brand.Name
                        : string.Empty,

                    Price = x.Price,
                    CurrentStock = x.CurrentStock,
                    MinimumStock = x.MinimumStock,
                    ExpirationDate = x.ExpirationDate
                })
                .ToListAsync();


            // =====================================================
            // SON 5 STOK HAREKETİ
            // =====================================================

            var recentTransactions =
                await _unitOfWork.StockTransactions
                    .GetAll()
                    .AsNoTracking()
                    .Include(x => x.Product)
                    .OrderByDescending(x => x.TransactionDate)
                    .Take(5)
                    .Select(x => new DashboardTransactionDto
                    {
                        Id = x.Id,
                        ProductId = x.ProductId,

                        ProductName = x.Product != null
                            ? x.Product.Name
                            : string.Empty,

                        Type = x.Type,
                        Quantity = x.Quantity,
                        TransactionDate = x.TransactionDate,
                        PerformedBy = x.PerformedBy,
                        Notes = x.Notes
                    })
                    .ToListAsync();


            // =====================================================
            // EN ÇOK İŞLEM GÖREN ÜRÜNLER
            // =====================================================

            var topProducts =
                await _unitOfWork.StockTransactions
                    .GetAll()
                    .AsNoTracking()
                    .Include(x => x.Product)
                    .Where(x => x.Product != null)
                    .GroupBy(x => x.Product!.Name)
                    .Select(group => new
                    {
                        ProductName = group.Key,
                        Count = group.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToListAsync();


            // =====================================================
            // TOPLAM DEPO HAREKETLERİ
            // =====================================================

            var totalStockIn =
                await _unitOfWork.StockTransactions
                    .GetAll()
                    .Where(x => x.Type == TransactionType.In)
                    .SumAsync(x => (int?)x.Quantity)
                ?? 0;


            var totalStockOut =
                await _unitOfWork.StockTransactions
                    .GetAll()
                    .Where(x => x.Type == TransactionType.Out)
                    .SumAsync(x => (int?)x.Quantity)
                ?? 0;


            // =====================================================
            // MODEL
            // =====================================================

            var model = new DashboardDto
            {
                TotalProducts =
                    await _unitOfWork.Products
                        .GetAll()
                        .CountAsync(),

                TotalBrands =
                    await _unitOfWork.Brands
                        .GetAll()
                        .CountAsync(),

                TotalCategories =
                    await _unitOfWork.Categories
                        .GetAll()
                        .CountAsync(),

                TotalStock =
                    await _unitOfWork.Products
                        .GetAll()
                        .SumAsync(x => (int?)x.CurrentStock)
                    ?? 0,


                TotalStockIn = totalStockIn,
                TotalStockOut = totalStockOut,


                RecentTransactions = recentTransactions,

                NewProducts = newProducts,

                LowStockProducts = lowStockProducts,

                OutOfStockProducts = outOfStockProducts,

                ExpiringProducts = expiringProducts,

                ExpiredProducts = expiredProducts,


                CriticalStockCount =
                    lowStockProducts.Count,

                OutOfStockCount =
                    outOfStockProducts.Count,

                ExpiringSoonCount =
                    expiringProducts.Count,

                ExpiredCount =
                    expiredProducts.Count,


                TopProductNames =
                    topProducts
                        .Select(x => x.ProductName)
                        .ToList(),

                TopProductTransactionCounts =
                    topProducts
                        .Select(x => x.Count)
                        .ToList()
            };


            // =====================================================
            // SON 7 GÜNLÜK GRAFİK
            // =====================================================

            var last7Days = Enumerable
                .Range(0, 7)
                .Select(i =>
                    today.AddDays(-6 + i))
                .ToList();


            model.Last7Days = last7Days
                .Select(x => x.ToString("dd.MM"))
                .ToList();


            var firstDay =
                last7Days.First();


            var transactions =
                await _unitOfWork.StockTransactions
                    .GetAll()
                    .AsNoTracking()
                    .Where(x =>
                        x.TransactionDate >= firstDay)
                    .ToListAsync();


            foreach (var day in last7Days)
            {
                var stockIn = transactions
                    .Where(x =>
                        x.TransactionDate.Date == day.Date &&
                        x.Type == TransactionType.In)
                    .Sum(x => x.Quantity);


                var stockOut = transactions
                    .Where(x =>
                        x.TransactionDate.Date == day.Date &&
                        x.Type == TransactionType.Out)
                    .Sum(x => x.Quantity);


                model.StockInData.Add(stockIn);

                model.StockOutData.Add(stockOut);
            }


            return model;
        }
    }
}