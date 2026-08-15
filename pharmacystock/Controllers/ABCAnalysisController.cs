using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Business.Interfaces;

namespace pharmacystock.Controllers
{
    public class ABCAnalysisController : Controller
    {
        private readonly IABCAnalysisService _abcAnalysisService;

        public ABCAnalysisController(
            IABCAnalysisService abcAnalysisService)
        {
            _abcAnalysisService = abcAnalysisService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model =
                await _abcAnalysisService.GetAnalysisAsync();


            ViewBag.TotalAnalysisValue =
                model.Sum(x => x.AnnualValue);


            return View(model);
        }
    }
}