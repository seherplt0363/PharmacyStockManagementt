using PharmacyStock.DTO.PurchaseOrderDTO;
using PharmacyStock.Entities.Enum;

namespace PharmacyStock.Business.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<List<PurchaseOrderListDto>> GetAllOrdersAsync();

        Task<PurchaseOrderListDto?> GetOrderByIdAsync(int id);

        Task<PurchaseOrderListDto?> CreateOrderFromDraftAsync(
            int supplierId,
            List<PurchaseOrderItemDto> items);

        Task<bool> UpdateOrderStatusAsync(
            int orderId,
            OrderStatus newStatus);
    }
}