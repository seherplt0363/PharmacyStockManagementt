using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Business.Interfaces;

namespace pharmacystock.Controllers
{
    public class OrderDraftController : Controller
    {
        private readonly IOrderDraftService _orderDraftService;
        private readonly ISupplierService _supplierService;

        public OrderDraftController(
            IOrderDraftService orderDraftService,
            ISupplierService supplierService)
        {
            _orderDraftService = orderDraftService;
            _supplierService = supplierService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model =
                await _orderDraftService.GetOrderDraftsAsync();

            ViewBag.Suppliers =
                await _supplierService.GetAllAsync();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Yetkili")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToSupplier(int supplierId)
        {
            try
            {
                var result =
                    await _orderDraftService
                        .SendDraftToSupplierAsync(supplierId);

                if (!result)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Sipariş taslağı gönderilemedi veya tedarikçi bulunamadı."
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "Sipariş taslağı başarıyla oluşturuldu ve tedarikçiye e-posta ile gönderildi."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"E-posta gönderilirken bir hata oluştu: {ex.Message}"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            var file =
                await _orderDraftService.ExportExcelAsync();

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Siparis_Taslagi_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf()
        {
            var file =
                await _orderDraftService.ExportPdfAsync();

            return File(
                file,
                "application/pdf",
                $"Siparis_Taslagi_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}