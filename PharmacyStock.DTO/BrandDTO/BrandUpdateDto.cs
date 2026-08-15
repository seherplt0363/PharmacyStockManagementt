using System.ComponentModel.DataAnnotations;

namespace PharmacyStock.DTO.BrandDTO
{
    public class BrandUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Marka adı zorunludur.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}