using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using pharmacystock.Models;
using pharmacystock.Models.ViewModels;
using pharmacystock.Services.Interfaces;
using PharmacyStock.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace pharmacystock.Services.Implementations
{
    public class OrderDraftService : IOrderDraftService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IPurchaseOrderService _purchaseOrderService;

        public OrderDraftService(
            ApplicationDbContext context,
            IEmailService emailService,
            IPurchaseOrderService purchaseOrderService)
        {
            _context = context;
            _emailService = emailService;
            _purchaseOrderService = purchaseOrderService;
        }

        public async Task<List<OrderDraftViewModel>> GetOrderDraftsAsync()
        {
            var products = await _context.Products
                .Include(p => p.StockTransactions)
                .ToListAsync();

            var orderDrafts = new List<OrderDraftViewModel>();
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            foreach (var product in products)
            {
                // Son 30 günlük çıkış (Satış) miktarı
                int totalStockOut = product.StockTransactions
                    .Where(x => x.Type == TransactionType.Out &&
                                x.TransactionDate >= thirtyDaysAgo)
                    .Sum(x => x.Quantity);

                // Günlük ortalama tüketim
                double dailyAverageConsumption = totalStockOut / 30.0;

                // Güvenlik stoğu (Eczane için 7 günlük tampon stok)
                int safetyStock = (int)Math.Ceiling(dailyAverageConsumption * 7);

                // Stok devir hızı (%) -> ERP Standartı: (30 Günlük Satış / Mevcut Stok) * 100
                // Stok 0 ise MinStok baz alınır ki tan tanımsızlık (0'a bölme) olmasın.
                double baseStockForTurnover = product.CurrentStock > 0 ? product.CurrentStock : Math.Max(product.MinimumStock, 1);
                double turnoverRate = Math.Min((totalStockOut / baseStockForTurnover) * 100, 1000); // Max %1000 ile sınırla

                // Mevcut stok kaç gün yeter?
                double daysRemaining = dailyAverageConsumption > 0
                    ? product.CurrentStock / dailyAverageConsumption
                    : 999;

                // Son sipariş tarihi
                var lastOrder = product.StockTransactions
                    .Where(x => x.Type == TransactionType.In)
                    .OrderByDescending(x => x.TransactionDate)
                    .FirstOrDefault();

                DateTime? lastOrderDate = lastOrder?.TransactionDate;
                int daysSinceLastOrder = lastOrderDate == null
                    ? 999
                    : (DateTime.Today - lastOrderDate.Value).Days;

                // ==========================================
                // AKILLI ERP SKORLAMA VE HESAPLAMA ALGORİTMASI
                // ==========================================
                int score = 0;
                var reasons = new List<string>();

                bool isOutOfStock = product.CurrentStock == 0;
                bool isUnderMinStock = product.CurrentStock <= product.MinimumStock;

                // 1. Stok Durumu Bazlı Puanlama (Ağırlık: %50)
                if (isOutOfStock)
                {
                    score += 50;
                    reasons.Add("Stok tamamen tükendi");
                }
                else if (isUnderMinStock)
                {
                    score += 35;
                    reasons.Add("Minimum stok altında");
                }
                else if (daysRemaining <= 7)
                {
                    score += 20;
                    reasons.Add("Stok 7 günden az yetecek");
                }

                // 2. Tüketim Hızı / Satış Yoğunluğu Bazlı Puanlama (Ağırlık: %30)
                if (dailyAverageConsumption >= 5)
                {
                    score += 30;
                    reasons.Add("Çok yüksek günlük satış");
                }
                else if (dailyAverageConsumption >= 2)
                {
                    score += 20;
                    reasons.Add("Yoğun satış");
                }
                else if (dailyAverageConsumption > 0)
                {
                    score += 10;
                    reasons.Add("Düzenli satış var");
                }

                // 3. Devir Hızı Bonusu (Ağırlık: %20)
                if (turnoverRate >= 150)
                {
                    score += 20;
                    reasons.Add("Yüksek stok devir hızı");
                }
                else if (turnoverRate >= 80)
                {
                    score += 10;
                }

                // ==========================================
                // SİPARİŞ ÖNERİ MİKTARI HESABI
                // ==========================================
                // Hedef Stok = Minimum Stok + Güvenlik Stoğu + (15 Günlük Satış İhtiyacı)
                int targetStock = product.MinimumStock + safetyStock + (int)Math.Ceiling(dailyAverageConsumption * 15);
                int suggestedOrder = 0;

                // Sipariş verilmesi gerekiyor mu? (Stok min seviyenin altındaysa veya yetersizse)
                if (product.CurrentStock < targetStock || isUnderMinStock)
                {
                    suggestedOrder = Math.Max(0, targetStock - product.CurrentStock);

                    // Eczane depoları için 5'in katlarına yuvarla (Örn: 12 -> 15 kutu)
                    if (suggestedOrder > 0 && suggestedOrder % 5 != 0)
                    {
                        suggestedOrder = ((suggestedOrder / 5) + 1) * 5;
                    }
                }

                // Sipariş ihtiyacı yoksa skoru düşür ve önceliği güncelle
                if (suggestedOrder == 0)
                {
                    score = Math.Min(score, 30); // Max 30 puan alabilir
                    reasons.Clear();
                    reasons.Add("Stok seviyesi yeterli");
                }

                // Öncelik Belirleme
                string priority = score switch
                {
                    >= 75 => "Acil",
                    >= 50 => "Sipariş Ver",
                    >= 30 => "Takip Et",
                    _ => "Sipariş Verme"
                };

                if (suggestedOrder == 0)
                {
                    priority = "Sipariş Verme";
                }

                orderDrafts.Add(new OrderDraftViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    CurrentStock = product.CurrentStock,
                    MinimumStock = product.MinimumStock,
                    TurnoverRate = Math.Round(turnoverRate, 1),
                    SuggestedOrderQuantity = suggestedOrder,
                    Priority = priority,
                    Reason = string.Join(", ", reasons),
                    PriorityScore = score,
                    Last30DaysSales = totalStockOut,
                    DailyAverageConsumption = Math.Round(dailyAverageConsumption, 2),
                    SafetyStock = safetyStock,
                    DaysRemaining = Math.Round(daysRemaining, 1),
                    LastOrderDate = lastOrderDate,
                    DaysSinceLastOrder = daysSinceLastOrder
                });
            }

            return orderDrafts
                .OrderByDescending(x => x.SuggestedOrderQuantity > 0) // Önce sipariş önerisi olanlar
                .ThenByDescending(x => x.PriorityScore)              // Sonra en acil olanlar
                .ThenBy(x => x.DaysRemaining)                       // Kalan günü az olanlar üstte
                .ToList();
        }

        public async Task<byte[]> ExportExcelAsync()
        {
            var drafts = await GetOrderDraftsAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sipariş Taslağı");

            worksheet.Cell(1, 1).Value = "İlaç Adı";
            worksheet.Cell(1, 2).Value = "Mevcut Stok";
            worksheet.Cell(1, 3).Value = "Minimum Stok";
            worksheet.Cell(1, 4).Value = "Stok Devir Hızı (%)";
            worksheet.Cell(1, 5).Value = "Önerilen Adet";
            worksheet.Cell(1, 6).Value = "Öncelik";
            worksheet.Cell(1, 7).Value = "Gerekçe";

            var headerRange = worksheet.Range("A1:G1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2A3F54");
            headerRange.Style.Font.FontColor = XLColor.White;

            int row = 2;
            foreach (var item in drafts)
            {
                worksheet.Cell(row, 1).Value = item.ProductName;
                worksheet.Cell(row, 2).Value = item.CurrentStock;
                worksheet.Cell(row, 3).Value = item.MinimumStock;
                worksheet.Cell(row, 4).Value = item.TurnoverRate;
                worksheet.Cell(row, 5).Value = item.SuggestedOrderQuantity;
                worksheet.Cell(row, 6).Value = item.Priority;
                worksheet.Cell(row, 7).Value = item.Reason;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var allDrafts = await GetOrderDraftsAsync();
            var drafts = allDrafts.Where(x => x.SuggestedOrderQuantity > 0).ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);

                    page.DefaultTextStyle(x =>
                        x.FontSize(10)
                         .FontFamily("Arial"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("ECZANE STOK YÖNETİMİ")
                                .Bold()
                                .FontSize(14)
                                .FontColor(Colors.Blue.Darken4);

                            col.Item().Text("Akıllı Karar Destek Sistemi Sipariş Raporu")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(100)
                            .AlignRight()
                            .Text(DateTime.Now.ToString("dd.MM.yyyy"))
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingTop(0.5f, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
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
                                header.Cell().Background(Colors.Blue.Darken4).Padding(5).Text("İlaç").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken4).Padding(5).AlignCenter().Text("Stok").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken4).Padding(5).AlignCenter().Text("Min").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken4).Padding(5).AlignCenter().Text("Devir").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken4).Padding(5).AlignCenter().Text("Öneri").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken4).Padding(5).AlignCenter().Text("Öncelik").Bold().FontColor(Colors.White);
                            });

                            foreach (var item in drafts)
                            {
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.ProductName);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text(item.CurrentStock.ToString());
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text(item.MinimumStock.ToString());
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text($"%{item.TurnoverRate}");
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text(item.SuggestedOrderQuantity.ToString());
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text(item.Priority);
                            }
                        });

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

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        public async Task<bool> SendDraftToSupplierAsync(int supplierId)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier == null || string.IsNullOrEmpty(supplier.Email))
                return false;

            var drafts = await GetOrderDraftsAsync();

            var orderItems = drafts
                .Where(x => x.SuggestedOrderQuantity > 0)
                .Select(x => (productId: x.ProductId, quantity: x.SuggestedOrderQuantity, unitPrice: 10m))
                .ToList();

            if (!orderItems.Any() && drafts.Any())
            {
                var fallbackProduct = drafts.First();
                orderItems.Add((productId: fallbackProduct.ProductId, quantity: 5, unitPrice: 10m));
            }

            if (orderItems.Any())
            {
                try
                {
                    await _purchaseOrderService.CreateOrderFromDraftAsync(supplierId, orderItems);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Sipariş DB kayıt hatası: {ex.Message}");
                }
            }

            byte[] pdfBytes = await ExportPdfAsync();
            string fileName = $"Siparis_Taslagi_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            string subject = $"Sipariş Talebi - {DateTime.Now:dd.MM.yyyy}";

            string htmlBody = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <style>
            body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color: #333333; line-height: 1.6; background-color: #f4f6f9; padding: 20px; }}
            .container {{ max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; background-color: #ffffff; overflow: hidden; }}
            .header {{ background-color: #111c44; color: #ffffff; padding: 25px; text-align: center; }}
            .header h1 {{ margin: 0; font-size: 22px; font-weight: 600; }}
            .content {{ padding: 25px; }}
            .details {{ background-color: #f7fafc; padding: 15px; border-radius: 6px; margin: 20px 0; border-left: 4px solid #3182ce; }}
            .footer {{ text-align: center; padding: 15px; font-size: 12px; color: #777777; border-top: 1px solid #e0e0e0; background-color: #fafafa; }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <h1>Sipariş Talebi</h1>
            </div>
            <div class='content'>
                <p>Sayın Yetkili (<b>{supplier.Name}</b>),</p>
                <p>Eczanemiz tarafından karar destek sistemi analiziyle hazırlanan akıllı sipariş taslağı ekte PDF dosyası olarak tarafınıza sunulmuştur.</p>
                <div class='details'>
                    <strong>Sipariş Detayları:</strong><br>
                    📅 <b>Tarih:</b> {DateTime.Now:dd.MM.yyyy HH:mm}<br>
                    🏢 <b>Gönderen:</b> Pharmacy Stock Management System<br>
                    📎 <b>Ekli Dosya:</b> {fileName}
                </div>
                <p>Sipariş kalemlerinin kontrol edilerek faturalandırılmasını ve teslimat işlemlerinin en kısa sürede başlatılmasını rica ederiz.</p>
                <p>İyi çalışmalar dileriz.</p>
            </div>
            <div class='footer'>
                Bu e-posta <b>Pharmacy Stock Management System (ERP)</b> tarafından otomatik olarak oluşturulmuştur.
            </div>
        </div>
    </body>
    </html>";

            await _emailService.SendEmailWithAttachmentAsync(supplier.Email, subject, htmlBody, pdfBytes, fileName);

            return true;
        }
    }
}