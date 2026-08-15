using PharmacyStock.DTO.AnalysisDTO;

namespace PharmacyStock.Business.Interfaces
{
    public interface IStockTurnoverService
    {
        Task<List<StockTurnoverDto>> GetAnalysisAsync();
    }
}