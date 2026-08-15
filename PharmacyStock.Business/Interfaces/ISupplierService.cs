using PharmacyStock.DTO.SupplierDTO;

namespace PharmacyStock.Business.Interfaces
{
    public interface ISupplierService
    {
        Task<List<SupplierListDto>> GetAllAsync();

        Task<SupplierListDto?> GetByIdAsync(int id);

        Task<bool> SupplierExistsAsync(string name);

        Task CreateAsync(SupplierCreateDto dto);

        Task UpdateAsync(SupplierUpdateDto dto);

        Task DeleteAsync(int id);
    }
}