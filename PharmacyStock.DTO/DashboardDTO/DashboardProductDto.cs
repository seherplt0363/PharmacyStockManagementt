namespace PharmacyStock.DTO.DashboardDTO
{
    public class DashboardProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Barcode { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string BrandName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int CurrentStock { get; set; }

        public int MinimumStock { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}