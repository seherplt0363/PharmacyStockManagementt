using System.ComponentModel.DataAnnotations;
using PharmacyStock.Entities.Enum;

namespace PharmacyStock.DTO.PurchaseOrderDTO
{
    public class PurchaseOrderStatusUpdateDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public OrderStatus Status { get; set; }
    }
}