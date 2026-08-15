using System.ComponentModel.DataAnnotations;

namespace PharmacyStock.DTO.PurchaseOrderDTO
{
    public class PurchaseOrderCreateDto
    {
        [Required(ErrorMessage = "Tedarikçi seçimi zorunludur.")]
        public int SupplierId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public DateTime? DeliveryDate { get; set; }

        public List<PurchaseOrderItemDto> OrderItems { get; set; }
            = new List<PurchaseOrderItemDto>();
    }
}