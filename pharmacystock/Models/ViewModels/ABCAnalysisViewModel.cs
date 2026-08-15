namespace pharmacystock.Models.ViewModels;

public class ABCAnalysisViewModel
{
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int TotalStockOut { get; set; }

    public decimal AnnualValue { get; set; }

    public double Percentage { get; set; }

    public double CumulativePercentage { get; set; }

    public string ABCClass { get; set; } = "";

    public int CurrentStock { get; set; }             // Mevcut Stok Miktarı

    public string ActionRecommendation { get; set; } = string.Empty; // Önerilen Aksiyon Mesajı

    public bool IsStockCritical { get; set; }         // Kritik Stok Bayrağı
}