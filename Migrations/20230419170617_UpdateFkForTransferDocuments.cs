using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class UpdateFkForTransferDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TransferJourneyDocument",
                table: "TransferJourneyDocument");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "TransferJourneyDocument",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransferJourneyDocument",
                table: "TransferJourneyDocument",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TransferJourneyDocument_TransferJourneyId",
                table: "TransferJourneyDocument",
                column: "TransferJourneyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TransferJourneyDocument",
                table: "TransferJourneyDocument");

            migrationBuilder.DropIndex(
                name: "IX_TransferJourneyDocument_TransferJourneyId",
                table: "TransferJourneyDocument");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TransferJourneyDocument");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransferJourneyDocument",
                table: "TransferJourneyDocument",
                column: "TransferJourneyId");
        }
    }
}
