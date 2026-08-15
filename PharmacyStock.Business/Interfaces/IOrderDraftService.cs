using PharmacyStock.DTO.OrderDraftDTO;

namespace PharmacyStock.Business.Interfaces
{
    public interface IOrderDraftService
    {
        Task<List<OrderDraftDto>> GetOrderDraftsAsync();

        Task<byte[]> ExportExcelAsync();

        Task<byte[]> ExportPdfAsync();

        Task<bool> SendDraftToSupplierAsync(int supplierId);
    }
}