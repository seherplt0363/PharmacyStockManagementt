using Microsoft.EntityFrameworkCore;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.DataAccess.Repositories.Interfaces;
using PharmacyStock.DTO.BrandDTO;
using PharmacyStock.Entities.Models;

namespace PharmacyStock.Business.Services
{
    public class BrandService : IBrandService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BrandService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<BrandListDto>> GetAllAsync(string? search = null)
        {
            var query = _unitOfWork.Brands
                .GetAll()
                .Include(x => x.Products)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.Name.Contains(search));
            }

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new BrandListDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProductCount = x.Products.Count
                })
                .ToListAsync();
        }

        public async Task<BrandListDto?> GetByIdAsync(int id)
        {
            var brand = await _unitOfWork.Brands
                .GetAll()
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (brand == null)
                return null;

            return new BrandListDto
            {
                Id = brand.Id,
                Name = brand.Name,
                ProductCount = brand.Products.Count
            };
        }

        public async Task<bool> BrandExistsAsync(string brandName)
        {
            if (string.IsNullOrWhiteSpace(brandName))
                return false;

            var normalizedName =
                brandName.Trim().ToLower();

            return await _unitOfWork.Brands
                .AnyAsync(x =>
                    x.Name.ToLower() == normalizedName);
        }

        public async Task CreateAsync(BrandCreateDto dto)
        {
            var brand = new Brand
            {
                Name = dto.Name.Trim()
            };

            await _unitOfWork.Brands.AddAsync(brand);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(BrandUpdateDto dto)
        {
            var brand = await _unitOfWork.Brands
                .GetByIdAsync(dto.Id);

            if (brand == null)
                return;

            brand.Name = dto.Name.Trim();

            _unitOfWork.Brands.Update(brand);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var brand = await _unitOfWork.Brands
                .GetByIdAsync(id);

            if (brand == null)
                return;

            _unitOfWork.Brands.Delete(brand);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}