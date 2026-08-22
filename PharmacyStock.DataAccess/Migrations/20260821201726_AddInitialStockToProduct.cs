using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyStock.DataAccess.Migrations
{
    public partial class AddInitialStockToProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InitialStock",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialStock",
                table: "Products");
        }
    }
}