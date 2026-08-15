using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PharmacyStock.Entities.Common;
using PharmacyStock.Entities.Enum;

namespace PharmacyStock.Entities.Models
{
    public class PurchaseOrder : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string OrderCode { get; set; } = string.Empty;

        [Required]
        public int SupplierId { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier? Supplier { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public DateTime? DeliveryDate { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Draft;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public virtual ICollection<PurchaseOrderItem> OrderItems { get; set; }
            = new List<PurchaseOrderItem>();
    }
}