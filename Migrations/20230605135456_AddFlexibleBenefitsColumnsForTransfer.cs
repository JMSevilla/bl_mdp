using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddFlexibleBenefitsColumnsForTransfer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfPayment",
                table: "TransferJourney",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameOfPlan",
                table: "TransferJourney",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeOfPayment",
                table: "TransferJourney",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfPayment",
                table: "TransferJourney");

            migrationBuilder.DropColumn(
                name: "NameOfPlan",
                table: "TransferJourney");

            migrationBuilder.DropColumn(
                name: "TypeOfPayment",
                table: "TransferJourney");
        }
    }
}
