namespace pharmacystock.Models.ViewModels
{
    public class StockTurnoverViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int InitialStock { get; set; }

        public int TotalStockIn { get; set; }

        public int TotalStockOut { get; set; }

        public int CurrentStock { get; set; }

        public double TurnoverRate { get; set; }

        public string Status { get; set; }

        public DateTime? LastStockOutDate { get; set; }
    }
}