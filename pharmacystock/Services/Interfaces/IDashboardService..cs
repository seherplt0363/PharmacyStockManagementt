using pharmacystock.Models.ViewModels;

namespace pharmacystock.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
    }
}