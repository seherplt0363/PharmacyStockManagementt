namespace pharmacystock.Models
{
    public enum OrderStatus
    {
        Draft = 0,       // Taslak
        Ordered = 1,     // Sipariş Verildi (Mail Gönderildi)
        Delivered = 2,   // Depoya Teslim Alındı (Stok Güncellendi)
        Cancelled = 3    // İptal Edildi
    }
}
