using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddColumnsQuoteStatusAndExpiryDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GuaranteedQuote",
                table: "Calculation",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QuoteExpiryDate",
                table: "Calculation",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuaranteedQuote",
                table: "Calculation");

            migrationBuilder.DropColumn(
                name: "QuoteExpiryDate",
                table: "Calculation");
        }
    }
}
