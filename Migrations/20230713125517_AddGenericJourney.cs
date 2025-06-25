using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddGenericJourney : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "JourneyId",
                table: "JourneyBranch",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Journeys",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessGroup = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SubmitDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journeys", x => x.Id);
                    table.UniqueConstraint("AK_Journeys_BusinessGroup_ReferenceNumber_Type", x => new { x.BusinessGroup, x.ReferenceNumber, x.Type });
                });

            migrationBuilder.CreateIndex(
                name: "IX_JourneyBranch_JourneyId",
                table: "JourneyBranch",
                column: "JourneyId");

            migrationBuilder.AddForeignKey(
                name: "FK_JourneyBranch_Journeys_JourneyId",
                table: "JourneyBranch",
                column: "JourneyId",
                principalTable: "Journeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JourneyBranch_Journeys_JourneyId",
                table: "JourneyBranch");

            migrationBuilder.DropTable(
                name: "Journeys");

            migrationBuilder.DropIndex(
                name: "IX_JourneyBranch_JourneyId",
                table: "JourneyBranch");

            migrationBuilder.DropColumn(
                name: "JourneyId",
                table: "JourneyBranch");
        }
    }
}
