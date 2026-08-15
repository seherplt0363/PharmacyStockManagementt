using System.ComponentModel.DataAnnotations;

namespace pharmacystock.Models
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Marka adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Marka adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Marka Adı")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
