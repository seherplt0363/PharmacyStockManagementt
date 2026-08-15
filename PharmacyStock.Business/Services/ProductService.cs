using Microsoft.EntityFrameworkCore;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.DataAccess.Repositories.Interfaces;
using PharmacyStock.DTO.ProductDTO;
using PharmacyStock.Entities.Models;

namespace PharmacyStock.Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ProductListDto>> GetAllAsync(string? search)
        {
            IQueryable<Product> query = _unitOfWork.Products
                .GetAll()
                .Include(x => x.Category)
                .Include(x => x.Brand);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.Name.Contains(search) ||
                    x.Barcode.Contains(search));
            }

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new ProductListDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Barcode = x.Barcode,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category != null
                        ? x.Category.Name
                        : string.Empty,
                    BrandId = x.BrandId,
                    BrandName = x.Brand != null
                        ? x.Brand.Name
                        : string.Empty,
                    Price = x.Price,
                    CurrentStock = x.CurrentStock,
                    MinimumStock = x.MinimumStock,
                    ExpirationDate = x.ExpirationDate,
                    Description = x.Description
                })
                .ToListAsync();
        }
        public async Task<ProductListDto?> GetByIdAsync(int id)
        {
            var product = await _unitOfWork.Products
                .GetAll()
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
                return null;

            return MapToListDto(product);
        }

        public async Task<bool> ProductExistsAsync(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return false;

            var normalizedName = productName.Trim().ToLower();

            return await _unitOfWork.Products
                .AnyAsync(x => x.Name.ToLower() == normalizedName);
        }

        public async Task CreateAsync(ProductCreateDto dto)
        {
            var product = new Product
            {
                Name = dto.Name.Trim(),
                Barcode = dto.Barcode.Trim(),
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                Price = dto.Price,
                Description = dto.Description,
                ExpirationDate = dto.ExpirationDate,
                MinimumStock = dto.MinimumStock,
                CurrentStock = 0
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductUpdateDto dto)
        {
            var product = await _unitOfWork.Products
                .GetByIdAsync(dto.Id);

            if (product == null)
                return;

            product.Name = dto.Name.Trim();
            product.Barcode = dto.Barcode.Trim();
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;
            product.Price = dto.Price;
            product.Description = dto.Description;
            product.ExpirationDate = dto.ExpirationDate;
            product.MinimumStock = dto.MinimumStock;

            _unitOfWork.Products.Update(product);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _unitOfWork.Products
                .GetByIdAsync(id);

            if (product == null)
                return;

            _unitOfWork.Products.Delete(product);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<ProductListDto>> GetCriticalStockAsync()
        {
            return await _unitOfWork.Products
                .GetAll()
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Where(x =>
                    x.CurrentStock <= x.MinimumStock &&
                    x.CurrentStock > 0)
                .OrderBy(x => x.CurrentStock)
                .Select(x => new ProductListDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Barcode = x.Barcode,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category != null
                        ? x.Category.Name
                        : string.Empty,
                    BrandId = x.BrandId,
                    BrandName = x.Brand != null
                        ? x.Brand.Name
                        : string.Empty,
                    Price = x.Price,
                    CurrentStock = x.CurrentStock,
                    MinimumStock = x.MinimumStock,
                    ExpirationDate = x.ExpirationDate,
                    Description = x.Description
                })
                .ToListAsync();
        }

        public async Task<List<ProductListDto>> GetOutOfStockAsync()
        {
            return await _unitOfWork.Products
                .GetAll()
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Where(x => x.CurrentStock == 0)
                .OrderBy(x => x.Name)
                .Select(x => new ProductListDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Barcode = x.Barcode,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category != null
                        ? x.Category.Name
                        : string.Empty,
                    BrandId = x.BrandId,
                    BrandName = x.Brand != null
                        ? x.Brand.Name
                        : string.Empty,
                    Price = x.Price,
                    CurrentStock = x.CurrentStock,
                    MinimumStock = x.MinimumStock,
                    ExpirationDate = x.ExpirationDate,
                    Description = x.Description
                })
                .ToListAsync();
        }

        public async Task<List<ProductListDto>> GetExpiringSoonAsync()
        {
            var today = DateTime.Today;
            var in90Days = today.AddDays(90);

            return await _unitOfWork.Products
                .GetAll()
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Where(x =>
                    x.ExpirationDate != DateTime.MinValue &&
                    x.ExpirationDate.Date >= today &&
                    x.ExpirationDate.Date <= in90Days)
                .OrderBy(x => x.ExpirationDate)
                .Select(x => new ProductListDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Barcode = x.Barcode,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category != null
                        ? x.Category.Name
                        : string.Empty,
                    BrandId = x.BrandId,
                    BrandName = x.Brand != null
                        ? x.Brand.Name
                        : string.Empty,
                    Price = x.Price,
                    CurrentStock = x.CurrentStock,
                    MinimumStock = x.MinimumStock,
                    ExpirationDate = x.ExpirationDate,
                    Description = x.Description
                })
                .ToListAsync();
        }

        public async Task<List<ProductListDto>> GetExpiredAsync()
        {
            var today = DateTime.Today;

            return await _unitOfWork.Products
                .GetAll()
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Where(x =>
                    x.ExpirationDate != DateTime.MinValue &&
                    x.ExpirationDate.Date < today)
                .OrderBy(x => x.ExpirationDate)
                .Select(x => new ProductListDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Barcode = x.Barcode,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category != null
                        ? x.Category.Name
                        : string.Empty,
                    BrandId = x.BrandId,
                    BrandName = x.Brand != null
                        ? x.Brand.Name
                        : string.Empty,
                    Price = x.Price,
                    CurrentStock = x.CurrentStock,
                    MinimumStock = x.MinimumStock,
                    ExpirationDate = x.ExpirationDate,
                    Description = x.Description
                })
                .ToListAsync();
        }

        private static ProductListDto MapToListDto(Product product)
        {
            return new ProductListDto
            {
                Id = product.Id,
                Name = product.Name,
                Barcode = product.Barcode,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name ?? string.Empty,
                Price = product.Price,
                CurrentStock = product.CurrentStock,
                MinimumStock = product.MinimumStock,
                ExpirationDate = product.ExpirationDate,
                Description = product.Description
            };
        }
    }
}