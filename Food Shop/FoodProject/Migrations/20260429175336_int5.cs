using Microsoft.EntityFrameworkCore.Migrations;

namespace FoodProject.Migrations
{
    public partial class int5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardCVC",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CardMonth_Year",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "Payments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardCVC",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CardMonth_Year",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
