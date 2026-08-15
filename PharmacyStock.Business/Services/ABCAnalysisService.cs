using Microsoft.EntityFrameworkCore;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.DataAccess.Repositories.Interfaces;
using PharmacyStock.DTO.AnalysisDTO;
using PharmacyStock.Entities.Enum;

namespace PharmacyStock.Business.Services
{
    public class ABCAnalysisService : IABCAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ABCAnalysisService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ABCAnalysisDto>> GetAnalysisAsync()
        {
            var products = await _unitOfWork.Products
                .GetAll()
                .AsNoTracking()
                .Include(x => x.StockTransactions)
                .ToListAsync();

            var list = new List<ABCAnalysisDto>();

            foreach (var product in products)
            {
                // =================================================
                // TOPLAM STOK ÇIKIŞI
                // =================================================

                var totalOut = product.StockTransactions
                    .Where(x => x.Type == TransactionType.Out)
                    .Sum(x => x.Quantity);


                // =================================================
                // YILLIK DEĞER
                // =================================================

                var annualValue =
                    product.Price * totalOut;


                // =================================================
                // MEVCUT STOK
                // =================================================
                // CurrentStock doğrudan Product tablosundan alınır.
                // Çünkü başlangıç stoku + giriş - çıkış mantığını
                // StockTransactionService zaten yönetmektedir.
                // =================================================

                var currentStock =
                    product.CurrentStock;


                list.Add(new ABCAnalysisDto
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    TotalStockOut = totalOut,
                    AnnualValue = annualValue,
                    CurrentStock = currentStock
                });
            }


            // =====================================================
            // YILLIK DEĞERE GÖRE SIRALAMA
            // =====================================================

            list = list
                .OrderByDescending(x => x.AnnualValue)
                .ToList();


            var total =
                list.Sum(x => x.AnnualValue);


            double cumulative = 0;


            // =====================================================
            // ABC SINIFLANDIRMASI
            // =====================================================

            foreach (var item in list)
            {
                if (total == 0)
                {
                    item.Percentage = 0;
                    item.CumulativePercentage = 0;
                    item.ABCClass = "C";
                    item.ActionRecommendation = "Satış Yok";
                    item.IsStockCritical = false;

                    continue;
                }


                item.Percentage =
                    (double)(item.AnnualValue / total * 100);


                cumulative +=
                    item.Percentage;


                item.CumulativePercentage =
                    cumulative;


                // =================================================
                // ABC CLASS
                // =================================================

                if (cumulative <= 80)
                {
                    item.ABCClass = "A";
                }
                else if (cumulative <= 95)
                {
                    item.ABCClass = "B";
                }
                else
                {
                    item.ABCClass = "C";
                }


                // =================================================
                // AKSİYON ÖNERİSİ
                // =================================================

                switch (item.ABCClass)
                {
                    case "A":

                        if (item.CurrentStock <= 20)
                        {
                            item.ActionRecommendation =
                                "⚠️ KRİTİK: Sipariş Verilmeli";

                            item.IsStockCritical = true;
                        }
                        else
                        {
                            item.ActionRecommendation =
                                "Sıkı Takip / Günlük Stok Kontrolü";

                            item.IsStockCritical = false;
                        }

                        break;


                    case "B":

                        item.ActionRecommendation =
                            "Haftalık Periyodik Kontrol";

                        item.IsStockCritical = false;

                        break;


                    default:

                        item.ActionRecommendation =
                            "Toplu Sipariş / Stok Azaltma";

                        item.IsStockCritical = false;

                        break;
                }
            }


            return list;
        }
    }
}