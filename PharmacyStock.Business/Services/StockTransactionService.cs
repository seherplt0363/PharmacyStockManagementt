using Microsoft.EntityFrameworkCore;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.DataAccess.Repositories.Interfaces;
using PharmacyStock.DTO.StockTransactionDTO;
using PharmacyStock.Entities.Enum;
using PharmacyStock.Entities.Models;

namespace PharmacyStock.Business.Services
{
    public class StockTransactionService : IStockTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StockTransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        // =====================================================
        // TÜM STOK HAREKETLERİ
        // =====================================================

        public async Task<List<StockTransactionListDto>> GetAllAsync()
        {
            return await _unitOfWork.StockTransactions
                .GetAll()
                .AsNoTracking()
                .Include(x => x.Product)
                .OrderByDescending(x => x.TransactionDate)
                .Select(x => new StockTransactionListDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,

                    ProductName = x.Product != null
                        ? x.Product.Name
                        : string.Empty,

                    Type = x.Type,
                    Quantity = x.Quantity,
                    TransactionDate = x.TransactionDate,
                    SerialNumbers = x.SerialNumbers,
                    Notes = x.Notes,
                    PerformedBy = x.PerformedBy
                })
                .ToListAsync();
        }


        // =====================================================
        // SON 50 STOK HAREKETİ
        // =====================================================

        public async Task<List<StockTransactionListDto>> GetRecentAsync()
        {
            return await _unitOfWork.StockTransactions
                .GetAll()
                .AsNoTracking()
                .Include(x => x.Product)
                .OrderByDescending(x => x.TransactionDate)
                .Take(50)
                .Select(x => new StockTransactionListDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,

                    ProductName = x.Product != null
                        ? x.Product.Name
                        : string.Empty,

                    Type = x.Type,
                    Quantity = x.Quantity,
                    TransactionDate = x.TransactionDate,
                    SerialNumbers = x.SerialNumbers,
                    Notes = x.Notes,
                    PerformedBy = x.PerformedBy
                })
                .ToListAsync();
        }


        // =====================================================
        // DETAY
        // =====================================================

        public async Task<StockTransactionListDto?> GetByIdAsync(int id)
        {
            return await _unitOfWork.StockTransactions
                .GetAll()
                .AsNoTracking()
                .Include(x => x.Product)
                .Where(x => x.Id == id)
                .Select(x => new StockTransactionListDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,

                    ProductName = x.Product != null
                        ? x.Product.Name
                        : string.Empty,

                    Type = x.Type,
                    Quantity = x.Quantity,
                    TransactionDate = x.TransactionDate,
                    SerialNumbers = x.SerialNumbers,
                    Notes = x.Notes,
                    PerformedBy = x.PerformedBy
                })
                .FirstOrDefaultAsync();
        }


        // =====================================================
        // YENİ STOK HAREKETİ
        // =====================================================

        public async Task<bool> CreateAsync(
            StockTransactionCreateDto dto)
        {
            if (dto.Quantity <= 0)
                return false;

            var product =
                await _unitOfWork.Products.GetByIdAsync(dto.ProductId);

            if (product == null)
                return false;


            // Çıkış işleminde önceden stok kontrolü
            if (dto.Type == TransactionType.Out &&
                product.CurrentStock < dto.Quantity)
            {
                return false;
            }


            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (dto.Type == TransactionType.In)
                {
                    product.CurrentStock += dto.Quantity;
                }
                else
                {
                    product.CurrentStock -= dto.Quantity;
                }


                if (product.CurrentStock < 0)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }


                var transaction = new StockTransaction
                {
                    ProductId = dto.ProductId,
                    Type = dto.Type,
                    Quantity = dto.Quantity,
                    TransactionDate = dto.TransactionDate,
                    SerialNumbers = dto.SerialNumbers,
                    Notes = dto.Notes,
                    PerformedBy = dto.PerformedBy
                };


                await _unitOfWork.StockTransactions
                    .AddAsync(transaction);


                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }


        // =====================================================
        // STOK HAREKETİ GÜNCELLE
        // =====================================================

        public async Task<bool> UpdateAsync(
            StockTransactionUpdateDto dto)
        {
            if (dto.Quantity <= 0)
                return false;


            // Eski hareketin değiştirilmemiş halini alıyoruz.
            var oldTransaction =
                await _unitOfWork.StockTransactions
                    .GetAll()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == dto.Id);


            if (oldTransaction == null)
                return false;


            var oldProduct =
                await _unitOfWork.Products
                    .GetByIdAsync(oldTransaction.ProductId);


            if (oldProduct == null)
                return false;


            Product? newProduct;

            // Aynı ürün üzerinde işlem yapılıyorsa aynı entity'yi kullan.
            if (oldTransaction.ProductId == dto.ProductId)
            {
                newProduct = oldProduct;
            }
            else
            {
                newProduct =
                    await _unitOfWork.Products
                        .GetByIdAsync(dto.ProductId);
            }


            if (newProduct == null)
                return false;


            /*
             * Önce hesaplamayı bellekte yapıyoruz.
             * Geçersiz durumda veritabanındaki entity'leri
             * değiştirmeden işlemi reddediyoruz.
             */


            if (oldTransaction.ProductId == dto.ProductId)
            {
                var calculatedStock = oldProduct.CurrentStock;


                // Eski etkinin geri alınması
                if (oldTransaction.Type == TransactionType.In)
                {
                    calculatedStock -= oldTransaction.Quantity;
                }
                else
                {
                    calculatedStock += oldTransaction.Quantity;
                }


                if (calculatedStock < 0)
                    return false;


                // Yeni etkinin uygulanması
                if (dto.Type == TransactionType.In)
                {
                    calculatedStock += dto.Quantity;
                }
                else
                {
                    if (calculatedStock < dto.Quantity)
                        return false;

                    calculatedStock -= dto.Quantity;
                }


                if (calculatedStock < 0)
                    return false;


                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    oldProduct.CurrentStock = calculatedStock;

                    return await UpdateTransactionRecordAsync(dto);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }


            /*
             * Ürün değişmişse iki ayrı stok etkisi var.
             */


            var calculatedOldProductStock =
                oldProduct.CurrentStock;

            var calculatedNewProductStock =
                newProduct.CurrentStock;


            // Eski hareketin etkisini geri al
            if (oldTransaction.Type == TransactionType.In)
            {
                calculatedOldProductStock -=
                    oldTransaction.Quantity;
            }
            else
            {
                calculatedOldProductStock +=
                    oldTransaction.Quantity;
            }


            if (calculatedOldProductStock < 0)
                return false;


            // Yeni hareketi yeni ürüne uygula
            if (dto.Type == TransactionType.In)
            {
                calculatedNewProductStock += dto.Quantity;
            }
            else
            {
                if (calculatedNewProductStock < dto.Quantity)
                    return false;

                calculatedNewProductStock -= dto.Quantity;
            }


            if (calculatedNewProductStock < 0)
                return false;


            await _unitOfWork.BeginTransactionAsync();

            try
            {
                oldProduct.CurrentStock =
                    calculatedOldProductStock;

                newProduct.CurrentStock =
                    calculatedNewProductStock;


                return await UpdateTransactionRecordAsync(dto);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }


        // =====================================================
        // STOK HAREKETİ SİL
        // =====================================================

        public async Task<bool> DeleteAsync(int id)
        {
            var transaction =
                await _unitOfWork.StockTransactions
                    .GetByIdAsync(id);


            if (transaction == null)
                return false;


            var product =
                await _unitOfWork.Products
                    .GetByIdAsync(transaction.ProductId);


            if (product == null)
                return false;


            var calculatedStock = product.CurrentStock;


            // Silinen hareketin stok etkisini geri al
            if (transaction.Type == TransactionType.In)
            {
                calculatedStock -= transaction.Quantity;
            }
            else
            {
                calculatedStock += transaction.Quantity;
            }


            if (calculatedStock < 0)
                return false;


            await _unitOfWork.BeginTransactionAsync();

            try
            {
                product.CurrentStock = calculatedStock;

                _unitOfWork.StockTransactions.Delete(transaction);

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }


        // =====================================================
        // PRIVATE - TRANSACTION UPDATE
        // =====================================================

        private async Task<bool> UpdateTransactionRecordAsync(
            StockTransactionUpdateDto dto)
        {
            var transaction =
                await _unitOfWork.StockTransactions
                    .GetByIdAsync(dto.Id);


            if (transaction == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }


            transaction.ProductId = dto.ProductId;
            transaction.Type = dto.Type;
            transaction.Quantity = dto.Quantity;
            transaction.TransactionDate = dto.TransactionDate;
            transaction.SerialNumbers = dto.SerialNumbers;
            transaction.Notes = dto.Notes;
            transaction.PerformedBy = dto.PerformedBy;


            _unitOfWork.StockTransactions.Update(transaction);


            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
    }
}