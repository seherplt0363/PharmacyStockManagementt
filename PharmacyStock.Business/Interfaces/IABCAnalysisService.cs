using PharmacyStock.DTO.AnalysisDTO;

namespace PharmacyStock.Business.Interfaces
{
    public interface IABCAnalysisService
    {
        Task<List<ABCAnalysisDto>> GetAnalysisAsync();
    }
}