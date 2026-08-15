using Microsoft.EntityFrameworkCore;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.DataAccess.Repositories.Interfaces;
using PharmacyStock.DTO.CategoryDTO;
using PharmacyStock.Entities.Models;

namespace PharmacyStock.Business.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CategoryListDto>> GetAllAsync(string? search = null)
        {
            var query = _unitOfWork.Categories
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
                .Select(x => new CategoryListDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ProductCount = x.Products.Count
                })
                .ToListAsync();
        }

        public async Task<CategoryListDto?> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.Categories
                .GetAll()
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
                return null;

            return new CategoryListDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductCount = category.Products.Count
            };
        }

        public async Task<bool> CategoryExistsAsync(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return false;

            var normalizedName =
                categoryName.Trim().ToLower();

            return await _unitOfWork.Categories
                .AnyAsync(x =>
                    x.Name.ToLower() == normalizedName);
        }

        public async Task CreateAsync(CategoryCreateDto dto)
        {
            var category = new Category
            {
                Name = dto.Name.Trim(),
                Description = dto.Description
            };

            await _unitOfWork.Categories.AddAsync(category);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(CategoryUpdateDto dto)
        {
            var category = await _unitOfWork.Categories
                .GetByIdAsync(dto.Id);

            if (category == null)
                return;

            category.Name = dto.Name.Trim();
            category.Description = dto.Description;

            _unitOfWork.Categories.Update(category);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _unitOfWork.Categories
                .GetByIdAsync(id);

            if (category == null)
                return;

            _unitOfWork.Categories.Delete(category);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}