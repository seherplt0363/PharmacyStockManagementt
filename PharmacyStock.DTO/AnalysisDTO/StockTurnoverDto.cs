namespace PharmacyStock.DTO.AnalysisDTO
{
    public class StockTurnoverDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int TotalStockIn { get; set; }

        public int TotalStockOut { get; set; }

        public int CurrentStock { get; set; }

        public double TurnoverRate { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? LastStockOutDate { get; set; }
    }
}