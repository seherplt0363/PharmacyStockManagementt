using Microsoft.EntityFrameworkCore;
using pharmacystock.Models;
using pharmacystock.Services.Interfaces;
using PharmacyStock.Data;

namespace pharmacystock.Services.Implementations
{
    public class StockTransactionService : IStockTransactionService
    {
        private readonly ApplicationDbContext _context;

        public StockTransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tüm stok hareketleri
        public async Task<List<StockTransaction>> GetAllAsync()
        {
            return await _context.StockTransactions
                .Include(x => x.Product)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();
        }

        // Son 50 stok hareketi
        public async Task<List<StockTransaction>> GetRecentAsync()
        {
            return await _context.StockTransactions
                .Include(x => x.Product)
                .OrderByDescending(x => x.TransactionDate)
                .Take(50)
                .ToListAsync();
        }

        // Stok hareketi detay
        public async Task<StockTransaction?> GetByIdAsync(int id)
        {
            return await _context.StockTransactions
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // Yeni stok hareketi
        public async Task<bool> CreateAsync(StockTransaction transaction)
        {
            if (transaction.Quantity <= 0)
                return false;

            var product = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == transaction.ProductId);

            if (product == null)
                return false;

            // STOK GİRİŞİ
            if (transaction.Type == TransactionType.In)
            {
                product.CurrentStock += transaction.Quantity;
            }
            // STOK ÇIKIŞI
            else
            {
                // Yeterli stok yoksa işlem yapılmaz
                if (product.CurrentStock < transaction.Quantity)
                    return false;

                product.CurrentStock -= transaction.Quantity;
            }

            // Güvenlik kontrolü
            if (product.CurrentStock < 0)
                return false;

            _context.StockTransactions.Add(transaction);

            await _context.SaveChangesAsync();

            return true;
        }

        // Stok hareketi düzenleme
        public async Task<bool> UpdateAsync(StockTransaction transaction)
        {
            if (transaction.Quantity <= 0)
                return false;

            // Veritabanındaki eski hareket
            var oldTransaction = await _context.StockTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == transaction.Id);

            if (oldTransaction == null)
                return false;

            // Eski ürün
            var oldProduct = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == oldTransaction.ProductId);

            // Yeni ürün
            var newProduct = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == transaction.ProductId);

            if (oldProduct == null || newProduct == null)
                return false;

            /*
             * =====================================================
             * 1. ESKİ HAREKETİN STOK ETKİSİNİ GERİ AL
             * =====================================================
             */

            if (oldTransaction.Type == TransactionType.In)
            {
                // Eski işlem giriş ise stoktan çıkar
                if (oldProduct.CurrentStock < oldTransaction.Quantity)
                    return false;

                oldProduct.CurrentStock -= oldTransaction.Quantity;
            }
            else
            {
                // Eski işlem çıkış ise stoğa geri ekle
                oldProduct.CurrentStock += oldTransaction.Quantity;
            }

            /*
             * =====================================================
             * 2. YENİ HAREKETİ UYGULA
             * =====================================================
             */

            if (transaction.Type == TransactionType.In)
            {
                // Yeni işlem giriş
                newProduct.CurrentStock += transaction.Quantity;
            }
            else
            {
                // Yeni işlem çıkış
                if (newProduct.CurrentStock < transaction.Quantity)
                    return false;

                newProduct.CurrentStock -= transaction.Quantity;
            }

            /*
             * =====================================================
             * 3. NEGATİF STOK KONTROLÜ
             * =====================================================
             */

            if (oldProduct.CurrentStock < 0 ||
                newProduct.CurrentStock < 0)
            {
                return false;
            }

            /*
             * =====================================================
             * 4. STOK HAREKETİNİ GÜNCELLE
             * =====================================================
             */

            _context.StockTransactions.Update(transaction);

            await _context.SaveChangesAsync();

            return true;
        }

        // Stok hareketi silme
        public async Task<bool> DeleteAsync(int id)
        {
            var transaction = await _context.StockTransactions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (transaction == null)
                return false;

            var product = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == transaction.ProductId);

            if (product == null)
                return false;

            /*
             * =====================================================
             * SİLİNEN HAREKETİN STOK ETKİSİNİ GERİ AL
             * =====================================================
             */

            if (transaction.Type == TransactionType.In)
            {
                // Giriş siliniyorsa stok azalır
                if (product.CurrentStock < transaction.Quantity)
                    return false;

                product.CurrentStock -= transaction.Quantity;
            }
            else
            {
                // Çıkış siliniyorsa stok tekrar artar
                product.CurrentStock += transaction.Quantity;
            }

            /*
             * Negatif stok kontrolü
             */
            if (product.CurrentStock < 0)
                return false;

            _context.StockTransactions.Remove(transaction);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}