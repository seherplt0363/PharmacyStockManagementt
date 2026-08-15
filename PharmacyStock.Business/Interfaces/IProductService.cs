using PharmacyStock.DTO.ProductDTO;

namespace PharmacyStock.Business.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductListDto>> GetAllAsync(string? search);

        Task<ProductListDto?> GetByIdAsync(int id);

        Task<bool> ProductExistsAsync(string productName);

        Task CreateAsync(ProductCreateDto dto);

        Task UpdateAsync(ProductUpdateDto dto);

        Task DeleteAsync(int id);

        Task<List<ProductListDto>> GetCriticalStockAsync();

        Task<List<ProductListDto>> GetOutOfStockAsync();

        Task<List<ProductListDto>> GetExpiringSoonAsync();

        Task<List<ProductListDto>> GetExpiredAsync();
    }
}