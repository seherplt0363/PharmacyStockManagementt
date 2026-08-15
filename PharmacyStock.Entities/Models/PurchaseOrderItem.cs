using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PharmacyStock.Entities.Common;

namespace PharmacyStock.Entities.Models
{
    public class PurchaseOrderItem : BaseEntity
    {
        [Required]
        public int PurchaseOrderId { get; set; }

        [ForeignKey(nameof(PurchaseOrderId))]
        public virtual PurchaseOrder? PurchaseOrder { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product? Product { get; set; }

        [Required]
        [Range(1, 100000, ErrorMessage = "Miktar en az 1 olmalıdır.")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 1000000, ErrorMessage = "Birim fiyat 0'dan büyük olmalıdır.")]
        public decimal UnitPrice { get; set; }
    }
}