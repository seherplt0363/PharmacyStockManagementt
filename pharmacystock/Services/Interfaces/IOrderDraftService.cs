using pharmacystock.Models.ViewModels;

namespace pharmacystock.Services.Interfaces
{
    public interface IOrderDraftService
    {
        Task<List<OrderDraftViewModel>> GetOrderDraftsAsync();

        Task<byte[]> ExportExcelAsync();

        Task<byte[]> ExportPdfAsync();

        Task<bool> SendDraftToSupplierAsync(int supplierId);
    }
}