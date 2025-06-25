using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class MemberQuoteData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MemberQuote_DateOfLeaving",
                table: "RetirementJourney",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MemberQuote_DatePensionableServiceCommenced",
                table: "RetirementJourney",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MemberQuote_EarliestRetirementAge",
                table: "RetirementJourney",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_FinalPensionableSalary",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MemberQuote_HasAvcs",
                table: "RetirementJourney",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_LtaPercentage",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MemberQuote_NormalRetirementAge",
                table: "RetirementJourney",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemberQuote_TotalPensionableService",
                table: "RetirementJourney",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemberQuote_TransferInService",
                table: "RetirementJourney",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberQuote_DateOfLeaving",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_DatePensionableServiceCommenced",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_EarliestRetirementAge",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_FinalPensionableSalary",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_HasAvcs",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_LtaPercentage",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_NormalRetirementAge",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_TotalPensionableService",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_TransferInService",
                table: "RetirementJourney");
        }
    }
}
