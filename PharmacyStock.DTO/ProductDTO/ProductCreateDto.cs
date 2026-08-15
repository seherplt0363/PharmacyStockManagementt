using System.ComponentModel.DataAnnotations;

namespace PharmacyStock.DTO.ProductDTO
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Barkod zorunludur.")]
        [StringLength(
            13,
            MinimumLength = 13,
            ErrorMessage = "Barkod 13 haneli olmalıdır."
        )]
        public string Barcode { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int BrandId { get; set; }

        [Required]
        [Range(
            0.01,
            1000000,
            ErrorMessage = "Fiyat 0'dan büyük olmalıdır."
        )]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Son kullanma tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime ExpirationDate { get; set; }

        [Range(
            0,
            1000000,
            ErrorMessage = "Minimum stok negatif olamaz."
        )]
        public int MinimumStock { get; set; } = 10;
    }
}