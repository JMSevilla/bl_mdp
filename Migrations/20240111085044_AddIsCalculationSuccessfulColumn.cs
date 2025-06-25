using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddIsCalculationSuccessfulColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCalculationSuccessful",
                table: "Calculation",
                type: "boolean",
                nullable: true,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCalculationSuccessful",
                table: "Calculation");
        }
    }
}
