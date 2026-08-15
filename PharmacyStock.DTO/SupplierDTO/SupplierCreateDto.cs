using System.ComponentModel.DataAnnotations;

namespace PharmacyStock.DTO.SupplierDTO
{
    public class SupplierCreateDto
    {
        [Required(ErrorMessage = "Tedarikçi adı zorunludur.")]
        [StringLength(150, ErrorMessage = "Tedarikçi adı en fazla 150 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçersiz e-posta adresi.")]
        [StringLength(150, ErrorMessage = "E-posta adresi en fazla 150 karakter olabilir.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir.")]
        public string? Phone { get; set; }
    }
}