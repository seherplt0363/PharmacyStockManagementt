using System.ComponentModel.DataAnnotations;

namespace PharmacyStock.DTO.CategoryDTO
{
    public class CategoryUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }
    }
}