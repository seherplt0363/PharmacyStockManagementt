namespace PharmacyStock.DTO.OrderDraftDTO
{
    public class OrderDraftDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int CurrentStock { get; set; }

        public int MinimumStock { get; set; }

        public double TurnoverRate { get; set; }

        public int SuggestedOrderQuantity { get; set; }

        public string Priority { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public int PriorityScore { get; set; }

        public int Last30DaysSales { get; set; }

        public double DailyAverageConsumption { get; set; }

        public int SafetyStock { get; set; }

        public double DaysRemaining { get; set; }

        public DateTime? LastOrderDate { get; set; }

        public int DaysSinceLastOrder { get; set; }
    }
}