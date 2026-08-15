using System.ComponentModel.DataAnnotations;
using PharmacyStock.Entities.Enum;

namespace PharmacyStock.DTO.StockTransactionDTO
{
    public class StockTransactionUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün seçimi zorunludur.")]
        [Display(Name = "Ürün")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "İşlem türü zorunludur.")]
        [Display(Name = "İşlem Türü")]
        public TransactionType Type { get; set; }

        [Required(ErrorMessage = "Miktar zorunludur.")]
        [Range(1, 100000, ErrorMessage = "Miktar en az 1 olmalıdır.")]
        [Display(Name = "Miktar")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "İşlem tarihi zorunludur.")]
        [Display(Name = "İşlem Tarihi")]
        public DateTime TransactionDate { get; set; }

        [Display(Name = "Seri Numaraları")]
        public string? SerialNumbers { get; set; }

        [StringLength(
            250,
            ErrorMessage = "Notlar en fazla 250 karakter olabilir."
        )]
        [Display(Name = "Notlar / Açıklama")]
        public string? Notes { get; set; }

        [Display(Name = "İşlemi Yapan")]
        public string? PerformedBy { get; set; }
    }
}