using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddPwToTransferJourney : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FinancialAdviseDate",
                table: "TransferJourney",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PensionWiseDate",
                table: "TransferJourney",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinancialAdviseDate",
                table: "TransferJourney");

            migrationBuilder.DropColumn(
                name: "PensionWiseDate",
                table: "TransferJourney");
        }
    }
}
