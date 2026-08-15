using PharmacyStock.DTO.StockTransactionDTO;

namespace PharmacyStock.Business.Interfaces
{
    public interface IStockTransactionService
    {
        Task<List<StockTransactionListDto>> GetAllAsync();

        Task<List<StockTransactionListDto>> GetRecentAsync();

        Task<StockTransactionListDto?> GetByIdAsync(int id);

        Task<bool> CreateAsync(StockTransactionCreateDto dto);

        Task<bool> UpdateAsync(StockTransactionUpdateDto dto);

        Task<bool> DeleteAsync(int id);
    }
}