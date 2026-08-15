using PharmacyStock.DTO.CategoryDTO;

namespace PharmacyStock.Business.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryListDto>> GetAllAsync(string? search = null);

        Task<CategoryListDto?> GetByIdAsync(int id);

        Task<bool> CategoryExistsAsync(string categoryName);

        Task CreateAsync(CategoryCreateDto dto);

        Task UpdateAsync(CategoryUpdateDto dto);

        Task DeleteAsync(int id);
    }
}