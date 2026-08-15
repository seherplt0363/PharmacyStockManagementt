using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.DTO.BrandDTO;

namespace pharmacystock.Controllers
{
    public class BrandController : Controller
    {
        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }


        // =====================================================
        // LİSTE
        // =====================================================

        public async Task<IActionResult> Index(string? searchString)
        {
            var brands =
                await _brandService.GetAllAsync(searchString);

            return View(brands);
        }


        // =====================================================
        // CREATE - GET
        // =====================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new BrandCreateDto());
        }


        // =====================================================
        // CREATE - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandCreateDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);


            if (await _brandService.BrandExistsAsync(dto.Name))
            {
                ModelState.AddModelError(
                    nameof(dto.Name),
                    "Bu marka zaten kayıtlıdır.");

                return View(dto);
            }


            await _brandService.CreateAsync(dto);


            TempData["Success"] =
                "Marka başarıyla oluşturuldu.";


            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // EDIT - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
                return NotFound();


            var brand =
                await _brandService.GetByIdAsync(id.Value);


            if (brand == null)
                return NotFound();


            var dto = new BrandUpdateDto
            {
                Id = brand.Id,
                Name = brand.Name
            };


            return View(dto);
        }


        // =====================================================
        // EDIT - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            BrandUpdateDto dto)
        {
            if (id != dto.Id)
                return NotFound();


            if (!ModelState.IsValid)
                return View(dto);


            await _brandService.UpdateAsync(dto);


            TempData["Success"] =
                "Marka başarıyla güncellendi.";


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


            var brand =
                await _brandService.GetByIdAsync(id.Value);


            if (brand == null)
                return NotFound();


            return View(brand);
        }


        // =====================================================
        // DELETE - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
                return NotFound();


            var brand =
                await _brandService.GetByIdAsync(id.Value);


            if (brand == null)
                return NotFound();


            return View(brand);
        }


        // =====================================================
        // DELETE - POST
        // =====================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _brandService.DeleteAsync(id);


            TempData["Success"] =
                "Marka başarıyla silindi.";


            return RedirectToAction(nameof(Index));
        }
    }
}