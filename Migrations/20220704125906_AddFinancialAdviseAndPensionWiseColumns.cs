using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddFinancialAdviseAndPensionWiseColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FinancialAdviseDate",
                table: "RetirementJourney",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PensionWiseDate",
                table: "RetirementJourney",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinancialAdviseDate",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "PensionWiseDate",
                table: "RetirementJourney");
        }
    }
}
