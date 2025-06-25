using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class CreateUploadedDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadedDocument",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tags = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Uuid = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsEpaOnly = table.Column<bool>(type: "boolean", nullable: false),
                    IsEdoc = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedDocument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransferJourneyDocument",
                columns: table => new
                {
                    TransferJourneyId = table.Column<long>(type: "bigint", nullable: false),
                    UploadedDocumentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferJourneyDocument", x => x.TransferJourneyId);
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
                name: "IX_TransferJourneyDocument_UploadedDocumentId",
                table: "TransferJourneyDocument",
                column: "UploadedDocumentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferJourneyDocument");

            migrationBuilder.DropTable(
                name: "UploadedDocument");
        }
    }
}
