using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Business.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace pharmacystock.Controllers
{
    public class ABCAnalysisController : Controller
    {
        private readonly IABCAnalysisService _abcAnalysisService;

        public ABCAnalysisController(IABCAnalysisService abcAnalysisService)
        {
            _abcAnalysisService = abcAnalysisService;
        }

        // =====================================================
        // ABC ANALİZ SAYFASI
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await _abcAnalysisService.GetAnalysisAsync();

            ViewBag.TotalAnalysisValue =
                model.Sum(x => x.AnnualValue);

            return View(model);
        }


        // =====================================================
        // EXCEL EXPORT
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            var model = await _abcAnalysisService.GetAnalysisAsync();

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("ABC Stok Analizi");

            // -------------------------------------------------
            // BAŞLIK
            // -------------------------------------------------
            worksheet.Cell(1, 1).Value = "ABC STOK ANALİZİ";

            worksheet.Range(1, 1, 1, 6).Merge();

            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // -------------------------------------------------
            // TABLO BAŞLIKLARI
            // -------------------------------------------------
            worksheet.Cell(3, 1).Value = "Ürün Adı";
            worksheet.Cell(3, 2).Value = "Çıkış Adedi (Satış)";
            worksheet.Cell(3, 3).Value = "Yıllık Değer (₺)";
            worksheet.Cell(3, 4).Value = "Ciro Payı (%)";
            worksheet.Cell(3, 5).Value = "Kümülatif Pay (%)";
            worksheet.Cell(3, 6).Value = "ABC Sınıfı";

            var headerRange = worksheet.Range(3, 1, 3, 6);

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // -------------------------------------------------
            // VERİLER
            // -------------------------------------------------
            var row = 4;

            foreach (var item in model)
            {
                worksheet.Cell(row, 1).Value = item.Name;
                worksheet.Cell(row, 2).Value = item.TotalStockOut;
                worksheet.Cell(row, 3).Value = item.AnnualValue;
                worksheet.Cell(row, 4).Value = item.Percentage;
                worksheet.Cell(row, 5).Value = item.CumulativePercentage;
                worksheet.Cell(row, 6).Value = item.ABCClass;

                row++;
            }

            // -------------------------------------------------
            // FORMATLAMA
            // -------------------------------------------------
            if (row > 4)
            {
                worksheet.Range(4, 3, row - 1, 3)
                    .Style.NumberFormat.Format = "#,##0.00 ₺";

                worksheet.Range(4, 4, row - 1, 5)
                    .Style.NumberFormat.Format = "0.00";
            }

            worksheet.Columns().AdjustToContents();

            worksheet.SheetView.FreezeRows(3);

            // -------------------------------------------------
            // DOSYAYI OLUŞTUR
            // -------------------------------------------------
            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var content = stream.ToArray();

            var fileName =
                $"ABC_Stok_Analizi_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // =====================================================
        // PDF EXPORT
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> ExportPdf()
        {
            var model = await _abcAnalysisService.GetAnalysisAsync();

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    // ==========================================
                    // BAŞLIK
                    // ==========================================
                    page.Header()
                        .PaddingBottom(15)
                        .Column(column =>
                        {
                            column.Item()
                                .Text("ABC STOK ANALİZİ")
                                .Bold()
                                .FontSize(20);

                            column.Item()
                                .PaddingTop(5)
                                .Text("Pareto (80/15/5) Dağılımı ve Stok Değer Analizi")
                                .FontSize(10);
                        });

                    // ==========================================
                    // TABLO
                    // ==========================================
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("Ürün Adı");
                            header.Cell().Element(HeaderCell).Text("Çıkış Adedi");
                            header.Cell().Element(HeaderCell).Text("Yıllık Değer");
                            header.Cell().Element(HeaderCell).Text("Ciro Payı");
                            header.Cell().Element(HeaderCell).Text("Kümülatif Pay");
                            header.Cell().Element(HeaderCell).Text("Sınıf");
                        });

                        foreach (var item in model)
                        {
                            table.Cell()
                                .Element(DataCell)
                                .Text(item.Name ?? "-");

                            table.Cell()
                                .Element(DataCell)
                                .Text(item.TotalStockOut.ToString());

                            table.Cell()
                                .Element(DataCell)
                                .Text($"{item.AnnualValue:N2} TL");

                            table.Cell()
                                .Element(DataCell)
                                .Text($"%{item.Percentage:N2}");

                            table.Cell()
                                .Element(DataCell)
                                .Text($"%{item.CumulativePercentage:N2}");

                            table.Cell()
                                .Element(DataCell)
                                .Text(item.ABCClass ?? "-");
                        }
                    });

                    // ==========================================
                    // ALT BİLGİ
                    // ==========================================
                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Pharmacy Stock Management | ");
                            text.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                        });
                });
            }).GeneratePdf();

            var fileName =
                $"ABC_Stok_Analizi_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            return File(
                pdfBytes,
                "application/pdf",
                fileName);
        }


        // =====================================================
        // PDF TABLO STİLLERİ
        // =====================================================
        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten3)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Medium)
                .Padding(6)
                .AlignMiddle();
        }

        private static IContainer DataCell(IContainer container)
        {
            return container
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(6)
                .AlignMiddle();
        }




    }



}