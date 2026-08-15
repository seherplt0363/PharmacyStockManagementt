using System;
using System.Collections.Generic;

namespace pharmacystock.Models
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty; // Örn: PO-20260805-001

        public int SupplierId { get; set; }
        public virtual Supplier? Supplier { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? DeliveryDate { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Draft;

        public decimal TotalAmount { get; set; }

        public virtual ICollection<PurchaseOrderItem> OrderItems { get; set; } = new List<PurchaseOrderItem>();
    }
}