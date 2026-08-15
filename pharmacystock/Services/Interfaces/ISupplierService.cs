using pharmacystock.Models;

namespace pharmacystock.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<List<Supplier>> GetAllAsync();
        Task<Supplier?> GetByIdAsync(int id);
    }
}
