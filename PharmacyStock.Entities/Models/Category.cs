using System.ComponentModel.DataAnnotations;
using PharmacyStock.Entities.Common;

namespace PharmacyStock.Entities.Models
{
    public class Category : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        public virtual ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}