using Microsoft.EntityFrameworkCore;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.DataAccess.Repositories.Interfaces;
using PharmacyStock.DTO.SupplierDTO;
using PharmacyStock.Entities.Models;

namespace PharmacyStock.Business.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SupplierService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<SupplierListDto>> GetAllAsync()
        {
            return await _unitOfWork.Suppliers
                .GetAll()
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new SupplierListDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    Phone = x.Phone
                })
                .ToListAsync();
        }

        public async Task<SupplierListDto?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Suppliers
                .GetAll()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new SupplierListDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    Phone = x.Phone
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SupplierExistsAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var normalizedName = name.Trim().ToLower();

            return await _unitOfWork.Suppliers
                .AnyAsync(x => x.Name.ToLower() == normalizedName);
        }

        public async Task CreateAsync(SupplierCreateDto dto)
        {
            var supplier = new Supplier
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim(),
                Phone = dto.Phone?.Trim()
            };

            await _unitOfWork.Suppliers.AddAsync(supplier);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(SupplierUpdateDto dto)
        {
            var supplier =
                await _unitOfWork.Suppliers.GetByIdAsync(dto.Id);

            if (supplier == null)
                return;

            supplier.Name = dto.Name.Trim();
            supplier.Email = dto.Email.Trim();
            supplier.Phone = dto.Phone?.Trim();

            _unitOfWork.Suppliers.Update(supplier);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var supplier =
                await _unitOfWork.Suppliers.GetByIdAsync(id);

            if (supplier == null)
                return;

            _unitOfWork.Suppliers.Delete(supplier);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}