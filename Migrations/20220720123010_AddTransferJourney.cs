using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddTransferJourney : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "RetirementJourneyId",
                table: "JourneyBranch",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "TransferJourneyId",
                table: "JourneyBranch",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TransferJourney",
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
                    table.PrimaryKey("PK_TransferJourney", x => x.Id);
                    table.UniqueConstraint("AK_TransferJourney_BusinessGroup_ReferenceNumber", x => new { x.BusinessGroup, x.ReferenceNumber });
                });

            migrationBuilder.CreateIndex(
                name: "IX_JourneyBranch_TransferJourneyId",
                table: "JourneyBranch",
                column: "TransferJourneyId");

            migrationBuilder.AddForeignKey(
                name: "FK_JourneyBranch_TransferJourney_TransferJourneyId",
                table: "JourneyBranch",
                column: "TransferJourneyId",
                principalTable: "TransferJourney",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JourneyBranch_TransferJourney_TransferJourneyId",
                table: "JourneyBranch");

            migrationBuilder.DropTable(
                name: "TransferJourney");

            migrationBuilder.DropIndex(
                name: "IX_JourneyBranch_TransferJourneyId",
                table: "JourneyBranch");

            migrationBuilder.DropColumn(
                name: "TransferJourneyId",
                table: "JourneyBranch");

            migrationBuilder.AlterColumn<long>(
                name: "RetirementJourneyId",
                table: "JourneyBranch",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
