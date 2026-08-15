using System.ComponentModel.DataAnnotations;

namespace PharmacyStock.Entities.Enum
{
    public enum TransactionType
    {
        [Display(Name = "Giriş")]
        In = 0,

        [Display(Name = "Çıkış")]
        Out = 1
    }
}