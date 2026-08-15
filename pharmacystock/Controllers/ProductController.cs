using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.DTO.ProductDTO;

namespace pharmacystock.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
        }


        // =====================================================
        // ÜRÜN LİSTESİ
        // =====================================================

        public async Task<IActionResult> Index(string? search)
        {
            var products =
                await _productService.GetAllAsync(search);

            return View(products);
        }


        // =====================================================
        // YENİ ÜRÜN - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();

            return View(new ProductCreateDto());
        }


        // =====================================================
        // YENİ ÜRÜN - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(
                    dto.CategoryId,
                    dto.BrandId);

                return View(dto);
            }


            if (await _productService.ProductExistsAsync(dto.Name))
            {
                ModelState.AddModelError(
                    nameof(dto.Name),
                    "Bu ürün zaten kayıtlı.");

                await LoadDropdownsAsync(
                    dto.CategoryId,
                    dto.BrandId);

                return View(dto);
            }


            await _productService.CreateAsync(dto);


            TempData["Success"] =
                "Ürün başarıyla oluşturuldu.";


            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // DETAY
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
                return NotFound();


            var product =
                await _productService.GetByIdAsync(id.Value);


            if (product == null)
                return NotFound();


            return View(product);
        }


        // =====================================================
        // DÜZENLE - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
                return NotFound();


            var product =
                await _productService.GetByIdAsync(id.Value);


            if (product == null)
                return NotFound();


            var dto = new ProductUpdateDto
            {
                Id = product.Id,
                Name = product.Name,
                Barcode = product.Barcode,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                Price = product.Price,
                Description = product.Description,
                ExpirationDate = product.ExpirationDate,
                MinimumStock = product.MinimumStock
            };


            await LoadDropdownsAsync(
                dto.CategoryId,
                dto.BrandId);


            return View(dto);
        }


        // =====================================================
        // DÜZENLE - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ProductUpdateDto dto)
        {
            if (id != dto.Id)
                return NotFound();


            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(
                    dto.CategoryId,
                    dto.BrandId);

                return View(dto);
            }


            await _productService.UpdateAsync(dto);


            TempData["Success"] =
                "Ürün başarıyla güncellendi.";


            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // SİL - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
                return NotFound();


            var product =
                await _productService.GetByIdAsync(id.Value);


            if (product == null)
                return NotFound();


            return View(product);
        }


        // =====================================================
        // SİL - POST
        // =====================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteAsync(id);


            TempData["Success"] =
                "Ürün başarıyla silindi.";


            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // KRİTİK STOK
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> CriticalStock()
        {
            var products =
                await _productService.GetCriticalStockAsync();

            return View(products);
        }


        // =====================================================
        // TÜKENEN ÜRÜNLER
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> OutOfStock()
        {
            var products =
                await _productService.GetOutOfStockAsync();

            return View(products);
        }


        // =====================================================
        // SKT YAKLAŞAN
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> ExpiringSoon()
        {
            var products =
                await _productService.GetExpiringSoonAsync();

            return View(products);
        }


        // =====================================================
        // SKT GEÇMİŞ
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Expired()
        {
            var products =
                await _productService.GetExpiredAsync();

            return View(products);
        }


        // =====================================================
        // DROPDOWN
        // Geçiş aşamasında DataAccess kullanılıyor.
        // Category / Brand servisleri taşındığında kaldıracağız.
        // =====================================================

        private async Task LoadDropdownsAsync(
            int? categoryId = null,
            int? brandId = null)
        {
            var categories = await _categoryService.GetAllAsync();
            var brands = await _brandService.GetAllAsync();


            ViewBag.Categories =
                new SelectList(
                    categories,
                    "Id",
                    "Name",
                    categoryId);


            ViewBag.Brands =
                new SelectList(
                    brands,
                    "Id",
                    "Name",
                    brandId);
        }
    }
}