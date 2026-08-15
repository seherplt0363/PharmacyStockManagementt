using PharmacyStock.DTO.DashboardDTO;

namespace PharmacyStock.Business.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
    }
}