using Microsoft.EntityFrameworkCore;
using pharmacystock.Models;
using pharmacystock.Services.Interfaces;
using PharmacyStock.Data;

namespace pharmacystock.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync(string? search)
        {
            var products = _context.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(x =>
                    x.Name.Contains(search) ||
                    x.Barcode.Contains(search));
            }

            return await products.ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> ProductExistsAsync(string productName)
        {
            return await _context.Products
                .AnyAsync(x =>
                    x.Name.ToLower() == productName.ToLower());
        }

        public async Task CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == product.Id);

            if (existingProduct == null)
                return;

            existingProduct.Name = product.Name;
            existingProduct.Barcode = product.Barcode;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.BrandId = product.BrandId;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.ExpirationDate = product.ExpirationDate;
            existingProduct.MinimumStock = product.MinimumStock;

            // CurrentStock burada değiştirilmez.
            // Stok miktarı yalnızca StockTransaction üzerinden yönetilir.

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products
                .FindAsync(id);

            if (product == null)
                return;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Product>> GetCriticalStockAsync()
        {
            return await _context.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Where(x =>
                    x.CurrentStock <= x.MinimumStock &&
                    x.CurrentStock > 0)
                .ToListAsync();
        }

        public async Task<List<Product>> GetOutOfStockAsync()
        {
            return await _context.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Where(x => x.CurrentStock == 0)
                .ToListAsync();
        }

        public async Task<List<Product>> GetExpiringSoonAsync()
        {
            var today = DateTime.Today;
            var in90Days = today.AddDays(90);

            return await _context.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Where(x =>
                    x.ExpirationDate != DateTime.MinValue &&
                    x.ExpirationDate.Date >= today &&
                    x.ExpirationDate.Date <= in90Days)
                .OrderBy(x => x.ExpirationDate)
                .ToListAsync();
        }

        public async Task<List<Product>> GetExpiredAsync()
        {
            var today = DateTime.Today;

            return await _context.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Where(x =>
                    x.ExpirationDate != DateTime.MinValue &&
                    x.ExpirationDate.Date < today)
                .OrderBy(x => x.ExpirationDate)
                .ToListAsync();
        }
    }
}