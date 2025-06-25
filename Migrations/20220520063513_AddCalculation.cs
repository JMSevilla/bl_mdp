using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddCalculation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Calculation",
                columns: table => new
                {
                    ReferenceNumber = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    BusinessGroup = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    RetirementDatesAgesJson = table.Column<string>(type: "text", nullable: false),
                    RetirementJson = table.Column<string>(type: "text", nullable: false),
                    RetirementJourneyId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calculation", x => new { x.BusinessGroup, x.ReferenceNumber });
                    table.ForeignKey(
                        name: "FK_Calculation_RetirementJourney_RetirementJourneyId",
                        column: x => x.RetirementJourneyId,
                        principalTable: "RetirementJourney",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Calculation_RetirementJourneyId",
                table: "Calculation",
                column: "RetirementJourneyId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Calculation");
        }
    }
}
