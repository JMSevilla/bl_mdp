using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WTW.MdpService.Domain.Common.Journeys;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class UpdateUploadedDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BGROUP",
                table: "UploadedDocument",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "REFNO",
                table: "UploadedDocument",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "UploadedDocument",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("DELETE FROM \"UploadedDocument\" where \"Id\" NOT IN (SELECT \"UploadedDocumentId\" FROM \"TransferJourneyDocument\");");
            migrationBuilder.Sql($"UPDATE \"UploadedDocument\" SET \"Type\" = 'Transfer2';");
            migrationBuilder.Sql("UPDATE \"UploadedDocument\" ud SET \"REFNO\"= tj.\"ReferenceNumber\", \"BGROUP\"= tj.\"BusinessGroup\" FROM \"TransferJourneyDocument\" tjd inner join \"TransferJourney\" tj on tjd.\"TransferJourneyId\" = tj.\"Id\" where ud.\"Id\" = tjd.\"UploadedDocumentId\";");

            migrationBuilder.DropTable(
                name: "TransferJourneyDocument");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BGROUP",
                table: "UploadedDocument");

            migrationBuilder.DropColumn(
                name: "REFNO",
                table: "UploadedDocument");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "UploadedDocument");

            migrationBuilder.CreateTable(
                name: "TransferJourneyDocument",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UploadedDocumentId = table.Column<long>(type: "bigint", nullable: false),
                    TransferJourneyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferJourneyDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferJourneyDocument_TransferJourney_TransferJourneyId",
                        column: x => x.TransferJourneyId,
                        principalTable: "TransferJourney",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransferJourneyDocument_UploadedDocument_UploadedDocumentId",
                        column: x => x.UploadedDocumentId,
                        principalTable: "UploadedDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransferJourneyDocument_TransferJourneyId",
                table: "TransferJourneyDocument",
                column: "TransferJourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferJourneyDocument_UploadedDocumentId",
                table: "TransferJourneyDocument",
                column: "UploadedDocumentId");
        }
    }
}
