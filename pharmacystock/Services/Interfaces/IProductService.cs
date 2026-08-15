using pharmacystock.Models;

namespace pharmacystock.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetAllAsync(string? search);

        Task<Product?> GetByIdAsync(int id);

        Task CreateAsync(Product product);

        Task UpdateAsync(Product product);

        Task DeleteAsync(int id);

        Task<bool> ProductExistsAsync(string productName);

        Task<List<Product>> GetCriticalStockAsync();

        Task<List<Product>> GetOutOfStockAsync();

        Task<List<Product>> GetExpiringSoonAsync();

        Task<List<Product>> GetExpiredAsync();
    }
}