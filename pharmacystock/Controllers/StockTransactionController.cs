using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.DTO.StockTransactionDTO;

namespace pharmacystock.Controllers
{
    public class StockTransactionController : Controller
    {
        private readonly IStockTransactionService _stockTransactionService;
        private readonly IProductService _productService;

        public StockTransactionController(
            IStockTransactionService stockTransactionService,
            IProductService productService)
        {
            _stockTransactionService = stockTransactionService;
            _productService = productService;
        }


        // =====================================================
        // TÜM STOK HAREKETLERİ
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var transactions =
                await _stockTransactionService.GetAllAsync();

            return View(transactions);
        }


        // =====================================================
        // SON 50 STOK HAREKETİ
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Recent()
        {
            var transactions =
                await _stockTransactionService.GetRecentAsync();

            ViewData["IsRecent"] = true;

            return View("Index", transactions);
        }


        // =====================================================
        // CREATE - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadProductsAsync();

            return View(new StockTransactionCreateDto());
        }


        // =====================================================
        // CREATE - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            StockTransactionCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadProductsAsync(dto.ProductId);

                return View(dto);
            }

            var result =
                await _stockTransactionService.CreateAsync(dto);

            if (!result)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Stok hareketi oluşturulamadı. Yeterli stok bulunmuyor veya ürün mevcut değil.");

                await LoadProductsAsync(dto.ProductId);

                return View(dto);
            }

            TempData["Success"] =
                "Stok hareketi başarıyla oluşturuldu.";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // DETAILS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
                return NotFound();

            var transaction =
                await _stockTransactionService.GetByIdAsync(id.Value);

            if (transaction == null)
                return NotFound();

            return View(transaction);
        }


        // =====================================================
        // EDIT - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
                return NotFound();

            var transaction =
                await _stockTransactionService.GetByIdAsync(id.Value);

            if (transaction == null)
                return NotFound();

            var dto = new StockTransactionUpdateDto
            {
                Id = transaction.Id,
                ProductId = transaction.ProductId,
                Type = transaction.Type,
                Quantity = transaction.Quantity,
                TransactionDate = transaction.TransactionDate,
                SerialNumbers = transaction.SerialNumbers,
                Notes = transaction.Notes,
                PerformedBy = transaction.PerformedBy
            };

            await LoadProductsAsync(dto.ProductId);

            return View(dto);
        }


        // =====================================================
        // EDIT - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            StockTransactionUpdateDto dto)
        {
            if (id != dto.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadProductsAsync(dto.ProductId);

                return View(dto);
            }

            var result =
                await _stockTransactionService.UpdateAsync(dto);

            if (!result)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Stok hareketi güncellenemedi. Stok miktarını ve ürün bilgilerini kontrol edin.");

                await LoadProductsAsync(dto.ProductId);

                return View(dto);
            }

            TempData["Success"] =
                "Stok hareketi başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // DELETE - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
                return NotFound();

            var transaction =
                await _stockTransactionService.GetByIdAsync(id.Value);

            if (transaction == null)
                return NotFound();

            return View(transaction);
        }


        // =====================================================
        // DELETE - POST
        // =====================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result =
                await _stockTransactionService.DeleteAsync(id);

            if (!result)
            {
                TempData["Error"] =
                    "Bu stok hareketi silinemedi. Stok bütünlüğü korunamadığı için işlem iptal edildi.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] =
                "Stok hareketi başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // PRODUCT DROPDOWN
        // Controller artık DbContext kullanmıyor.
        // ProductService üzerinden ürünleri alıyor.
        // =====================================================

        private async Task LoadProductsAsync(int? productId = null)
        {
            var products =
                await _productService.GetAllAsync(null);

            ViewBag.Products = new SelectList(
                products,
                "Id",
                "Name",
                productId);
        }
    }
}