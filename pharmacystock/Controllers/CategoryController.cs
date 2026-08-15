using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.DTO.CategoryDTO;

namespace pharmacystock.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }


        // =====================================================
        // LİSTE
        // =====================================================

        public async Task<IActionResult> Index(string? searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var categories =
                await _categoryService.GetAllAsync(searchString);

            return View(categories);
        }


        // =====================================================
        // CREATE - GET
        // =====================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryCreateDto());
        }


        // =====================================================
        // CREATE - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);


            if (await _categoryService.CategoryExistsAsync(dto.Name))
            {
                ModelState.AddModelError(
                    nameof(dto.Name),
                    "Bu kategori zaten mevcut.");

                return View(dto);
            }


            await _categoryService.CreateAsync(dto);


            TempData["Success"] =
                "Kategori başarıyla oluşturuldu.";


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


            var category =
                await _categoryService.GetByIdAsync(id.Value);


            if (category == null)
                return NotFound();


            var dto = new CategoryUpdateDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
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
            CategoryUpdateDto dto)
        {
            if (id != dto.Id)
                return NotFound();


            if (!ModelState.IsValid)
                return View(dto);


            await _categoryService.UpdateAsync(dto);


            TempData["Success"] =
                "Kategori başarıyla güncellendi.";


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


            var category =
                await _categoryService.GetByIdAsync(id.Value);


            if (category == null)
                return NotFound();


            return View(category);
        }


        // =====================================================
        // DELETE - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
                return NotFound();


            var category =
                await _categoryService.GetByIdAsync(id.Value);


            if (category == null)
                return NotFound();


            return View(category);
        }


        // =====================================================
        // DELETE - POST
        // =====================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryService.DeleteAsync(id);


            TempData["Success"] =
                "Kategori başarıyla silindi.";


            return RedirectToAction(nameof(Index));
        }
    }
}