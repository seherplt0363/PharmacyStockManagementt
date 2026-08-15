using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pharmacystock.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(150)]
        [Display(Name = "Ürün Adı")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Barkod zorunludur.")]
        [StringLength(
            13,
            MinimumLength = 13,
            ErrorMessage = "Barkod 13 haneli olmalıdır."
        )]
        public string Barcode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual Category? Category { get; set; }

        [Required]
        [Display(Name = "Marka")]
        public int BrandId { get; set; }

        [ForeignKey(nameof(BrandId))]
        public virtual Brand? Brand { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Satış Fiyatı")]
        [Range(
            0.01,
            1000000,
            ErrorMessage = "Fiyat 0'dan büyük olmalıdır."
        )]
        public decimal Price { get; set; }

        [Range(
            0,
            1000000,
            ErrorMessage = "Stok negatif olamaz."
        )]
        [Display(Name = "Mevcut Stok")]
        public int CurrentStock { get; set; } = 0;

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Son kullanma tarihi zorunludur.")]
        [Display(Name = "Son Kullanma Tarihi")]
        [DataType(DataType.Date)]
        public DateTime ExpirationDate { get; set; }

        [Display(Name = "Minimum Stok")]
        [Range(
            0,
            1000000,
            ErrorMessage = "Minimum stok negatif olamaz."
        )]
        public int MinimumStock { get; set; } = 10;

        public virtual ICollection<StockTransaction> StockTransactions
        {
            get;
            set;
        } = new List<StockTransaction>();
    }
}