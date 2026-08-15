using PharmacyStock.DTO.BrandDTO;

namespace PharmacyStock.Business.Interfaces
{
    public interface IBrandService
    {
        Task<List<BrandListDto>> GetAllAsync(string? search = null);

        Task<BrandListDto?> GetByIdAsync(int id);

        Task<bool> BrandExistsAsync(string brandName);

        Task CreateAsync(BrandCreateDto dto);

        Task UpdateAsync(BrandUpdateDto dto);

        Task DeleteAsync(int id);
    }
}