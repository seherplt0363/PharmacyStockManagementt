namespace PharmacyStock.DTO.AnalysisDTO
{
    public class ABCAnalysisDto
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int TotalStockOut { get; set; }

        public decimal AnnualValue { get; set; }

        public int CurrentStock { get; set; }

        public double Percentage { get; set; }

        public double CumulativePercentage { get; set; }

        public string ABCClass { get; set; } = string.Empty;

        public string ActionRecommendation { get; set; } = string.Empty;

        public bool IsStockCritical { get; set; }
    }
}