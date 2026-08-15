using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Business.Interfaces;

namespace pharmacystock.Controllers
{
    public class StockTurnoverController : Controller
    {
        private readonly IStockTurnoverService _stockTurnoverService;

        public StockTurnoverController(
            IStockTurnoverService stockTurnoverService)
        {
            _stockTurnoverService = stockTurnoverService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model =
                await _stockTurnoverService.GetAnalysisAsync();

            ViewBag.AverageTurnover = model.Any()
                ? Math.Round(
                    model.Average(x => x.TurnoverRate),
                    1)
                : 0;

            ViewBag.FastCount =
                model.Count(x =>
                    x.Status == "Hızlı Dönen");

            ViewBag.NormalCount =
                model.Count(x =>
                    x.Status == "Normal");

            ViewBag.SlowCount =
                model.Count(x =>
                    x.Status == "Yavaş Dönen");

            ViewBag.DeadCount =
                model.Count(x =>
                    x.Status == "Hareketsiz");

            return View(model);
        }

        [HttpGet]
        public IActionResult SyncStocks()
        {
            TempData["Success"] =
                "Stoklar mevcut stok değerleri üzerinden senkronize edildi.";

            return RedirectToAction(nameof(Index));
        }
    }
}