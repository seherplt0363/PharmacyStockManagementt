using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pharmacystock.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStockTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PerformedBy",
                table: "StockTransactions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PerformedBy",
                table: "StockTransactions");
        }
    }
}
