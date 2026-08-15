using PharmacyStock.Entities.Enum;

namespace PharmacyStock.DTO.StockTransactionDTO
{
    public class StockTransactionListDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public TransactionType Type { get; set; }

        public int Quantity { get; set; }

        public DateTime TransactionDate { get; set; }

        public string? SerialNumbers { get; set; }

        public string? Notes { get; set; }

        public string? PerformedBy { get; set; }
    }
}