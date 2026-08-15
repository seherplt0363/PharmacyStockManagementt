using System.ComponentModel.DataAnnotations;

namespace PharmacyStock.DTO.BrandDTO
{
    public class BrandCreateDto
    {
        [Required(ErrorMessage = "Marka adı zorunludur.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}