using pharmacystock.Models;

namespace pharmacystock.Services.Interfaces
{
    public interface IBrandService
    {
        Task<List<Brand>> GetAllAsync(string? search);

        Task<Brand?> GetByIdAsync(int id);

        Task<bool> BrandExistsAsync(string brandName);

        Task CreateAsync(Brand brand);

        Task UpdateAsync(Brand brand);

        Task DeleteAsync(int id);
    }
}