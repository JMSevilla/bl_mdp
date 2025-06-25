using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class RemoveBereavmentJourneyFromMdpDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JourneyBranch_BereavementJourney_BereavementJourneyId",
                table: "JourneyBranch");

            migrationBuilder.DropTable(
                name: "BereavementContactConfirmation");

            migrationBuilder.DropTable(
                name: "BereavementJourney");

            migrationBuilder.DropIndex(
                name: "IX_JourneyBranch_BereavementJourneyId",
                table: "JourneyBranch");

            migrationBuilder.DropColumn(
                name: "BereavementJourneyId",
                table: "JourneyBranch");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BereavementJourneyId",
                table: "JourneyBranch",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BereavementContactConfirmation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessGroup = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Contact = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FailedConfirmationAttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaximumConfirmationAttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ReferenceNumber = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Token = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    ValidatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BereavementContactConfirmation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BereavementJourney",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessGroup = table.Column<string>(type: "text", nullable: false),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SubmissionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BereavementJourney", x => x.Id);
                    table.UniqueConstraint("AK_BereavementJourney_BusinessGroup_ReferenceNumber", x => new { x.BusinessGroup, x.ReferenceNumber });
                });

            migrationBuilder.CreateIndex(
                name: "IX_JourneyBranch_BereavementJourneyId",
                table: "JourneyBranch",
                column: "BereavementJourneyId");

            migrationBuilder.AddForeignKey(
                name: "FK_JourneyBranch_BereavementJourney_BereavementJourneyId",
                table: "JourneyBranch",
                column: "BereavementJourneyId",
                principalTable: "BereavementJourney",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
