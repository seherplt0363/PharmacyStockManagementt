using Microsoft.EntityFrameworkCore;
using pharmacystock.Models;
using pharmacystock.Services.Interfaces;
using PharmacyStock.Data;

namespace pharmacystock.Services.Implementations
{
    public class SupplierService : ISupplierService
    {
        private readonly ApplicationDbContext _context;

        public SupplierService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Supplier>> GetAllAsync()
        {
            return await _context.Suppliers.OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _context.Suppliers.FindAsync(id);
        }
    }
}
