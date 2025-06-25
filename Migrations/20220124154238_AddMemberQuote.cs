using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddMemberQuote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MemberQuote_Label",
                table: "RetirementJourney",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_LumpSumFromDb",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_LumpSumFromDc",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_MaximumLumpSum",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_MinimumLumpSum",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MemberQuote_SearchedRetirementDate",
                table: "RetirementJourney",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero));

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_SmallPotLumpSum",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_TaxFreeUfpls",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_TaxableUfpls",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_TotalLumpSum",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_TotalPension",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_TotalSpousePension",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_TotalUfpls",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_TransferValueOfDc",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberQuote_TrivialCommutationLumpSum",
                table: "RetirementJourney",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberQuote_Label",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_LumpSumFromDb",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_LumpSumFromDc",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_MaximumLumpSum",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_MinimumLumpSum",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_SearchedRetirementDate",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_SmallPotLumpSum",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_TaxFreeUfpls",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_TaxableUfpls",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_TotalLumpSum",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_TotalPension",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_TotalSpousePension",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_TotalUfpls",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_TransferValueOfDc",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_TrivialCommutationLumpSum",
                table: "RetirementJourney");
        }
    }
}