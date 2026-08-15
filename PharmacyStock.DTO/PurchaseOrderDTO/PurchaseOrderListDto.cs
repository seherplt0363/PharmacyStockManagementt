using PharmacyStock.Entities.Enum;

namespace PharmacyStock.DTO.PurchaseOrderDTO
{
    public class PurchaseOrderListDto
    {
        public int Id { get; set; }

        public string OrderCode { get; set; } = string.Empty;

        public int SupplierId { get; set; }

        public string SupplierName { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public OrderStatus Status { get; set; }

        public decimal TotalAmount { get; set; }

        public List<PurchaseOrderItemDto> OrderItems { get; set; }
            = new();
    }
}