using Microsoft.EntityFrameworkCore;
using pharmacystock.Models;
using pharmacystock.Services.Interfaces;
using PharmacyStock.Data;

namespace pharmacystock.Services.Implementations
{
    public class BrandService : IBrandService
    {
        private readonly ApplicationDbContext _context;

        public BrandService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Brand>> GetAllAsync(string? search)
        {
            var brands = _context.Brands.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                brands = brands.Where(x => x.Name.Contains(search));
            }

            return await brands.ToListAsync();
        }

        public async Task<Brand?> GetByIdAsync(int id)
        {
            return await _context.Brands.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> BrandExistsAsync(string brandName)
        {
            return await _context.Brands
                .AnyAsync(x => x.Name.ToLower() == brandName.ToLower());
        }

        public async Task CreateAsync(Brand brand)
        {
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Brand brand)
        {
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);

            if (brand != null)
            {
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
            }
        }
    }
}