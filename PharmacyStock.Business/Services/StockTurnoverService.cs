using Microsoft.EntityFrameworkCore;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.DataAccess.Repositories.Interfaces;
using PharmacyStock.DTO.AnalysisDTO;
using PharmacyStock.Entities.Enum;

namespace PharmacyStock.Business.Services
{
    public class StockTurnoverService : IStockTurnoverService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StockTurnoverService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<StockTurnoverDto>> GetAnalysisAsync()
        {
            var products = await _unitOfWork.Products
                .GetAll()
                .AsNoTracking()
                .Include(x => x.StockTransactions)
                .ToListAsync();

            var result = new List<StockTurnoverDto>();

            foreach (var product in products)
            {
                var stockIn = product.StockTransactions
                    .Where(x => x.Type == TransactionType.In)
                    .Sum(x => x.Quantity);

                var stockOut = product.StockTransactions
                    .Where(x => x.Type == TransactionType.Out)
                    .Sum(x => x.Quantity);

                var currentStock = product.CurrentStock;

                var turnoverRate = stockIn > 0
                    ? (double)stockOut / stockIn * 100
                    : 0;

                string status;

                if (turnoverRate >= 80)
                {
                    status = "Hızlı Dönen";
                }
                else if (turnoverRate >= 60)
                {
                    status = "Normal";
                }
                else if (turnoverRate >= 40)
                {
                    status = "Yavaş Dönen";
                }
                else
                {
                    status = "Hareketsiz";
                }

                var lastStockOutDate = product.StockTransactions
                    .Where(x => x.Type == TransactionType.Out)
                    .OrderByDescending(x => x.TransactionDate)
                    .Select(x => (DateTime?)x.TransactionDate)
                    .FirstOrDefault();

                result.Add(new StockTurnoverDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    TotalStockIn = stockIn,
                    TotalStockOut = stockOut,
                    CurrentStock = currentStock,
                    TurnoverRate = turnoverRate,
                    Status = status,
                    LastStockOutDate = lastStockOutDate
                });
            }

            return result
                .OrderByDescending(x => x.TurnoverRate)
                .ToList();
        }
    }
}