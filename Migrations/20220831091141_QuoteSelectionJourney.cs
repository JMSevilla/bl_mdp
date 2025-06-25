using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class QuoteSelectionJourney : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AnswerKey",
                table: "QuestionForm",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25);

            migrationBuilder.AddColumn<long>(
                name: "QuoteSelectionJourneyId",
                table: "JourneyBranch",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuoteSelectionJourney",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessGroup = table.Column<string>(type: "text", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteSelectionJourney", x => x.Id);
                    table.UniqueConstraint("AK_QuoteSelectionJourney_BusinessGroup_ReferenceNumber", x => new { x.BusinessGroup, x.ReferenceNumber });
                });

            migrationBuilder.CreateIndex(
                name: "IX_JourneyBranch_QuoteSelectionJourneyId",
                table: "JourneyBranch",
                column: "QuoteSelectionJourneyId");

            migrationBuilder.AddForeignKey(
                name: "FK_JourneyBranch_QuoteSelectionJourney_QuoteSelectionJourneyId",
                table: "JourneyBranch",
                column: "QuoteSelectionJourneyId",
                principalTable: "QuoteSelectionJourney",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JourneyBranch_QuoteSelectionJourney_QuoteSelectionJourneyId",
                table: "JourneyBranch");

            migrationBuilder.DropTable(
                name: "QuoteSelectionJourney");

            migrationBuilder.DropIndex(
                name: "IX_JourneyBranch_QuoteSelectionJourneyId",
                table: "JourneyBranch");

            migrationBuilder.DropColumn(
                name: "QuoteSelectionJourneyId",
                table: "JourneyBranch");

            migrationBuilder.AlterColumn<string>(
                name: "AnswerKey",
                table: "QuestionForm",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);
        }
    }
}
