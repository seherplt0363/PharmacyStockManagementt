using System.Collections.Generic;
using System.Threading.Tasks;
using pharmacystock.Models;

namespace pharmacystock.Services.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<List<PurchaseOrder>> GetAllOrdersAsync();
        Task<PurchaseOrder?> GetOrderByIdAsync(int id);
        Task<PurchaseOrder> CreateOrderFromDraftAsync(int supplierId, List<(int productId, int quantity, decimal unitPrice)> items);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
    }
}