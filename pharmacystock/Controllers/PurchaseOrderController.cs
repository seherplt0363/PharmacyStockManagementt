using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.Entities.Enum;

namespace pharmacystock.Controllers
{
    public class PurchaseOrderController : Controller
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrderController(
            IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }


        // =====================================================
        // TÜM SİPARİŞLER
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders =
                await _purchaseOrderService.GetAllOrdersAsync();

            return View(orders);
        }


        // =====================================================
        // SİPARİŞ DURUMU GÜNCELLE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id,
            OrderStatus status)
        {
            var result =
                await _purchaseOrderService
                    .UpdateOrderStatusAsync(id, status);


            if (!result)
            {
                TempData["ErrorMessage"] =
                    "Sipariş durumu güncellenemedi.";

                return RedirectToAction(nameof(Index));
            }


            TempData["SuccessMessage"] =
                status == OrderStatus.Delivered
                    ? "Sipariş teslim alındı ve ürün stokları otomatik güncellendi."
                    : "Sipariş durumu başarıyla güncellendi.";


            return RedirectToAction(nameof(Index));
        }
    }
}