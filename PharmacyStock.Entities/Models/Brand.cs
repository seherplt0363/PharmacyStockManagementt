using System.ComponentModel.DataAnnotations;
using PharmacyStock.Entities.Common;

namespace PharmacyStock.Entities.Models
{
    public class Brand : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}