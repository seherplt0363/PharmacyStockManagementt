using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.DataAccess.Repositories.Interfaces;
using PharmacyStock.DTO.OrderDraftDTO;
using PharmacyStock.DTO.PurchaseOrderDTO;
using PharmacyStock.Entities.Enum;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PharmacyStock.Business.Services
{
    public class OrderDraftService : IOrderDraftService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IPurchaseOrderService _purchaseOrderService;

        public OrderDraftService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IPurchaseOrderService purchaseOrderService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _purchaseOrderService = purchaseOrderService;
        }

        // =====================================================
        // SİPARİŞ TASLAĞI / KARAR DESTEK ALGORİTMASI
        // =====================================================

        public async Task<List<OrderDraftDto>> GetOrderDraftsAsync()
        {
            var products = await _unitOfWork.Products
                .GetAll()
                .AsNoTracking()
                .Include(x => x.StockTransactions)
                .ToListAsync();

            var orderDrafts = new List<OrderDraftDto>();

            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            foreach (var product in products)
            {
                // =================================================
                // SON 30 GÜNLÜK SATIŞ
                // =================================================

                var totalStockOut = product.StockTransactions
                    .Where(x =>
                        x.Type == TransactionType.Out &&
                        x.TransactionDate >= thirtyDaysAgo)
                    .Sum(x => x.Quantity);

                // =================================================
                // GÜNLÜK ORTALAMA TÜKETİM
                // =================================================

                var dailyAverageConsumption =
                    totalStockOut / 30.0;

                // =================================================
                // 7 GÜNLÜK GÜVENLİK STOĞU
                // =================================================

                var safetyStock =
                    (int)Math.Ceiling(
                        dailyAverageConsumption * 7);

                // =================================================
                // 30 GÜNLÜK STOK DEVİR ORANI
                // =================================================
                //
                // Eski hesap:
                //
                // Son 30 Gün Satış / Mevcut Stok * 100
                //
                // Stok çok azaldığında %500, %1000 gibi
                // aşırı yüksek değerler oluşabiliyordu.
                //
                // Yeni hesap:
                //
                // Satış / (Satış + Mevcut Stok) * 100
                //
                // Böylece değer doğal olarak %0 - %100
                // arasında kalır.
                // =================================================

                var stockBase =
                    totalStockOut + product.CurrentStock;

                var turnoverRate =
                    stockBase > 0
                        ? (totalStockOut /
                           (double)stockBase) * 100
                        : 0;

                // =================================================
                // STOK KAÇ GÜN YETER?
                // =================================================

                var daysRemaining =
                    dailyAverageConsumption > 0
                        ? product.CurrentStock /
                          dailyAverageConsumption
                        : 999;

                // =================================================
                // SON STOK GİRİŞİ / SON SİPARİŞ
                // =================================================

                var lastOrder = product.StockTransactions
                    .Where(x =>
                        x.Type == TransactionType.In)
                    .OrderByDescending(x =>
                        x.TransactionDate)
                    .FirstOrDefault();

                var lastOrderDate =
                    lastOrder?.TransactionDate;

                var daysSinceLastOrder =
                    lastOrderDate == null
                        ? 999
                        : (DateTime.Today -
                           lastOrderDate.Value.Date).Days;

                // =================================================
                // AKILLI ERP SKORLAMA
                // =================================================

                var score = 0;

                var reasons =
                    new List<string>();

                var isOutOfStock =
                    product.CurrentStock == 0;

                var isUnderMinStock =
                    product.CurrentStock <=
                    product.MinimumStock;

                // -------------------------------------------------
                // 1. STOK DURUMU - MAKSİMUM 50 PUAN
                // -------------------------------------------------

                if (isOutOfStock)
                {
                    score += 50;

                    reasons.Add(
                        "Stok tamamen tükendi");
                }
                else if (isUnderMinStock)
                {
                    score += 35;

                    reasons.Add(
                        "Minimum stok altında");
                }
                else if (daysRemaining <= 7)
                {
                    score += 20;

                    reasons.Add(
                        "Stok 7 günden az yetecek");
                }

                // -------------------------------------------------
                // 2. SATIŞ YOĞUNLUĞU - MAKSİMUM 30 PUAN
                // -------------------------------------------------

                if (dailyAverageConsumption >= 5)
                {
                    score += 30;

                    reasons.Add(
                        "Çok yüksek günlük satış");
                }
                else if (dailyAverageConsumption >= 2)
                {
                    score += 20;

                    reasons.Add(
                        "Yoğun satış");
                }
                else if (dailyAverageConsumption > 0)
                {
                    score += 10;

                    reasons.Add(
                        "Düzenli satış var");
                }

                // -------------------------------------------------
                // 3. 30 GÜNLÜK DEVİR ORANI - MAKSİMUM 20 PUAN
                // -------------------------------------------------
                //
                // Yeni devir oranı 0-100 arasında olduğu için
                // eşikler buna göre düzenlenmiştir.
                //
                // %70 ve üzeri  -> yüksek devir
                // %40 - %69.9   -> orta devir
                // %40 altı      -> ek puan yok
                // -------------------------------------------------

                if (turnoverRate >= 70)
                {
                    score += 20;

                    reasons.Add(
                        "Yüksek stok devir oranı");
                }
                else if (turnoverRate >= 40)
                {
                    score += 10;

                    reasons.Add(
                        "Orta düzey stok devir oranı");
                }

                // =================================================
                // ÖNERİLEN SİPARİŞ MİKTARI
                // =================================================

                // Minimum stok
                // + 7 günlük güvenlik stoğu
                // + 15 günlük beklenen tüketim

                var targetStock =
                    product.MinimumStock +
                    safetyStock +
                    (int)Math.Ceiling(
                        dailyAverageConsumption * 15);

                var suggestedOrder = 0;

                if (product.CurrentStock < targetStock ||
                    isUnderMinStock)
                {
                    suggestedOrder =
                        Math.Max(
                            0,
                            targetStock -
                            product.CurrentStock);

                    // Eczane siparişlerini
                    // 5'in katına yuvarla
                    if (suggestedOrder > 0 &&
                        suggestedOrder % 5 != 0)
                    {
                        suggestedOrder =
                            ((suggestedOrder / 5) + 1) * 5;
                    }
                }

                // =================================================
                // SİPARİŞ GEREKMİYORSA
                // =================================================

                if (suggestedOrder == 0)
                {
                    score =
                        Math.Min(score, 30);

                    reasons.Clear();

                    reasons.Add(
                        "Stok seviyesi yeterli");
                }

                // =================================================
                // ÖNCELİK
                // =================================================

                var priority = score switch
                {
                    >= 75 => "Acil",
                    >= 50 => "Sipariş Ver",
                    >= 30 => "Takip Et",
                    _ => "Sipariş Verme"
                };

                // Sipariş miktarı 0 ise kullanıcının gereksiz
                // sipariş vermesini engelle.
                if (suggestedOrder == 0)
                {
                    priority =
                        "Sipariş Verme";
                }

                // =================================================
                // DTO
                // =================================================

                orderDrafts.Add(
                    new OrderDraftDto
                    {
                        ProductId =
                            product.Id,

                        ProductName =
                            product.Name,

                        CurrentStock =
                            product.CurrentStock,

                        MinimumStock =
                            product.MinimumStock,

                        TurnoverRate =
                            Math.Round(
                                turnoverRate,
                                1),

                        SuggestedOrderQuantity =
                            suggestedOrder,

                        Priority =
                            priority,

                        Reason =
                            string.Join(
                                ", ",
                                reasons),

                        PriorityScore =
                            score,

                        Last30DaysSales =
                            totalStockOut,

                        DailyAverageConsumption =
                            Math.Round(
                                dailyAverageConsumption,
                                2),

                        SafetyStock =
                            safetyStock,

                        DaysRemaining =
                            Math.Round(
                                daysRemaining,
                                1),

                        LastOrderDate =
                            lastOrderDate,

                        DaysSinceLastOrder =
                            daysSinceLastOrder
                    });
            }

            return orderDrafts
                .OrderByDescending(x =>
                    x.SuggestedOrderQuantity > 0)
                .ThenByDescending(x =>
                    x.PriorityScore)
                .ThenBy(x =>
                    x.DaysRemaining)
                .ToList();
        }

        // =====================================================
        // EXCEL EXPORT
        // =====================================================

        public async Task<byte[]> ExportExcelAsync()
        {
            var drafts =
                await GetOrderDraftsAsync();

            using var workbook =
                new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add(
                    "Sipariş Taslağı");

            worksheet.Cell(1, 1).Value =
                "İlaç Adı";

            worksheet.Cell(1, 2).Value =
                "Mevcut Stok";

            worksheet.Cell(1, 3).Value =
                "Minimum Stok";

            worksheet.Cell(1, 4).Value =
                "30 Günlük Devir Oranı (%)";

            worksheet.Cell(1, 5).Value =
                "Önerilen Adet";

            worksheet.Cell(1, 6).Value =
                "Öncelik";

            worksheet.Cell(1, 7).Value =
                "Gerekçe";

            var headerRange =
                worksheet.Range("A1:G1");

            headerRange.Style.Font.Bold =
                true;

            headerRange.Style.Fill
                .BackgroundColor =
                XLColor.FromHtml(
                    "#2A3F54");

            headerRange.Style.Font
                .FontColor =
                XLColor.White;

            var row = 2;

            foreach (var item in drafts)
            {
                worksheet.Cell(row, 1).Value =
                    item.ProductName;

                worksheet.Cell(row, 2).Value =
                    item.CurrentStock;

                worksheet.Cell(row, 3).Value =
                    item.MinimumStock;

                worksheet.Cell(row, 4).Value =
                    item.TurnoverRate;

                worksheet.Cell(row, 5).Value =
                    item.SuggestedOrderQuantity;

                worksheet.Cell(row, 6).Value =
                    item.Priority;

                worksheet.Cell(row, 7).Value =
                    item.Reason;

                row++;
            }

            worksheet.Columns()
                .AdjustToContents();

            using var stream =
                new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        // =====================================================
        // PDF EXPORT
        // =====================================================

        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License =
                LicenseType.Community;

            var allDrafts =
                await GetOrderDraftsAsync();

            var drafts =
                allDrafts
                    .Where(x =>
                        x.SuggestedOrderQuantity > 0)
                    .ToList();

            var document =
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);

                        page.Margin(
                            1.5f,
                            Unit.Centimetre);

                        page.DefaultTextStyle(x =>
                            x.FontSize(10));

                        // =========================================
                        // HEADER
                        // =========================================

                        page.Header()
                            .Row(row =>
                            {
                                row.RelativeItem()
                                    .Column(col =>
                                    {
                                        col.Item()
                                            .Text(
                                                "ECZANE STOK YÖNETİMİ")
                                            .Bold()
                                            .FontSize(14)
                                            .FontColor(
                                                Colors.Blue.Darken4);

                                        col.Item()
                                            .Text(
                                                "Akıllı Karar Destek Sistemi Sipariş Raporu")
                                            .FontSize(9)
                                            .FontColor(
                                                Colors.Grey.Darken1);
                                    });

                                row.ConstantItem(100)
                                    .AlignRight()
                                    .Text(
                                        DateTime.Now.ToString(
                                            "dd.MM.yyyy"))
                                    .FontSize(9)
                                    .FontColor(
                                        Colors.Grey.Darken1);
                            });

                        // =========================================
                        // TABLE
                        // =========================================

                        page.Content()
                            .PaddingTop(
                                0.5f,
                                Unit.Centimetre)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(
                                    columns =>
                                    {
                                        columns.RelativeColumn(2.5f);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(1.5f);
                                    });

                                table.Header(header =>
                                {
                                    HeaderCell(
                                        header.Cell(),
                                        "İlaç");

                                    HeaderCell(
                                        header.Cell(),
                                        "Stok");

                                    HeaderCell(
                                        header.Cell(),
                                        "Min");

                                    HeaderCell(
                                        header.Cell(),
                                        "30 Gün Devir");

                                    HeaderCell(
                                        header.Cell(),
                                        "Öneri");

                                    HeaderCell(
                                        header.Cell(),
                                        "Öncelik");
                                });

                                foreach (var item in drafts)
                                {
                                    BodyCell(
                                        table,
                                        item.ProductName,
                                        false);

                                    BodyCell(
                                        table,
                                        item.CurrentStock.ToString());

                                    BodyCell(
                                        table,
                                        item.MinimumStock.ToString());

                                    BodyCell(
                                        table,
                                        $"%{item.TurnoverRate}");

                                    BodyCell(
                                        table,
                                        item.SuggestedOrderQuantity.ToString());

                                    BodyCell(
                                        table,
                                        item.Priority);
                                }
                            });

                        // =========================================
                        // FOOTER
                        // =========================================

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.CurrentPageNumber();

                                x.Span(" / ");

                                x.TotalPages();
                            });
                    });
                });

            using var stream =
                new MemoryStream();

            document.GeneratePdf(stream);

            return stream.ToArray();
        }

        // =====================================================
        // TEDARİKÇİYE GÖNDER
        // =====================================================

        public async Task<bool> SendDraftToSupplierAsync(
            int supplierId)
        {
            var supplier =
                await _unitOfWork.Suppliers
                    .GetByIdAsync(supplierId);

            if (supplier == null ||
                string.IsNullOrWhiteSpace(
                    supplier.Email))
            {
                return false;
            }

            var drafts =
                await GetOrderDraftsAsync();

            var orderItems =
                drafts
                    .Where(x =>
                        x.SuggestedOrderQuantity > 0)
                    .Select(x =>
                        new PurchaseOrderItemDto
                        {
                            ProductId =
                                x.ProductId,

                            Quantity =
                                x.SuggestedOrderQuantity,

                            UnitPrice =
                                10m
                        })
                    .ToList();

            // Eski davranışı koruyoruz.
            if (!orderItems.Any() &&
                drafts.Any())
            {
                var fallbackProduct =
                    drafts.First();

                orderItems.Add(
                    new PurchaseOrderItemDto
                    {
                        ProductId =
                            fallbackProduct.ProductId,

                        Quantity =
                            5,

                        UnitPrice =
                            10m
                    });
            }

            if (orderItems.Any())
            {
                try
                {
                    await _purchaseOrderService
                        .CreateOrderFromDraftAsync(
                            supplierId,
                            orderItems);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Sipariş DB kayıt hatası: {ex.Message}");
                }
            }

            // =================================================
            // PDF
            // =================================================

            var pdfBytes =
                await ExportPdfAsync();

            var fileName =
                $"Siparis_Taslagi_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            var subject =
                $"Sipariş Talebi - {DateTime.Now:dd.MM.yyyy}";

            // =================================================
            // MAIL BODY
            // =================================================

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            color: #333333;
            line-height: 1.6;
            background-color: #f4f6f9;
            padding: 20px;
        }}

        .container {{
            max-width: 600px;
            margin: 0 auto;
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            background-color: #ffffff;
            overflow: hidden;
        }}

        .header {{
            background-color: #111c44;
            color: #ffffff;
            padding: 25px;
            text-align: center;
        }}

        .header h1 {{
            margin: 0;
            font-size: 22px;
            font-weight: 600;
        }}

        .content {{
            padding: 25px;
        }}

        .details {{
            background-color: #f7fafc;
            padding: 15px;
            border-radius: 6px;
            margin: 20px 0;
            border-left: 4px solid #3182ce;
        }}

        .footer {{
            text-align: center;
            padding: 15px;
            font-size: 12px;
            color: #777777;
            border-top: 1px solid #e0e0e0;
            background-color: #fafafa;
        }}
    </style>
</head>

<body>

    <div class='container'>

        <div class='header'>
            <h1>Sipariş Talebi</h1>
        </div>

        <div class='content'>

            <p>
                Sayın Yetkili
                (<b>{supplier.Name}</b>),
            </p>

            <p>
                Eczanemiz tarafından karar destek sistemi analiziyle
                hazırlanan akıllı sipariş taslağı ekte PDF dosyası
                olarak tarafınıza sunulmuştur.
            </p>

            <div class='details'>

                <strong>Sipariş Detayları:</strong>
                <br>

                📅 <b>Tarih:</b>
                {DateTime.Now:dd.MM.yyyy HH:mm}

                <br>

                🏢 <b>Gönderen:</b>
                Pharmacy Stock Management System

                <br>

                📎 <b>Ekli Dosya:</b>
                {fileName}

            </div>

            <p>
                Sipariş kalemlerinin kontrol edilerek
                faturalandırılmasını ve teslimat işlemlerinin
                en kısa sürede başlatılmasını rica ederiz.
            </p>

            <p>
                İyi çalışmalar dileriz.
            </p>

        </div>

        <div class='footer'>
            Bu e-posta
            <b>Pharmacy Stock Management System (ERP)</b>
            tarafından otomatik olarak oluşturulmuştur.
        </div>

    </div>

</body>
</html>";

            await _emailService
                .SendEmailWithAttachmentAsync(
                    supplier.Email,
                    subject,
                    htmlBody,
                    pdfBytes,
                    fileName);

            return true;
        }

        // =====================================================
        // PDF HELPERS
        // =====================================================

        private static void HeaderCell(
            IContainer container,
            string text)
        {
            container
                .Background(Colors.Blue.Darken4)
                .Padding(5)
                .AlignCenter()
                .Text(text)
                .Bold()
                .FontColor(Colors.White);
        }

        private static void BodyCell(
            TableDescriptor table,
            string text,
            bool center = true)
        {
            IContainer cell =
                table.Cell()
                    .BorderBottom(0.5f)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Padding(5);

            if (center)
            {
                cell
                    .AlignCenter()
                    .Text(text);
            }
            else
            {
                cell.Text(text);
            }
        }
    }
}