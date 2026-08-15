using pharmacystock.Models;

namespace pharmacystock.Services.Interfaces
{
    public interface IStockTransactionService
    {
        Task<List<StockTransaction>> GetAllAsync();

        Task<List<StockTransaction>> GetRecentAsync();

        Task<StockTransaction?> GetByIdAsync(int id);

        Task<bool> CreateAsync(StockTransaction transaction);

        Task<bool> UpdateAsync(StockTransaction transaction);

        Task<bool> DeleteAsync(int id);
    }
}