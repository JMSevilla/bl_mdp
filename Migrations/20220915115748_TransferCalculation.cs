using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class TransferCalculation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransferQuoteJson",
                table: "Calculation");

            migrationBuilder.CreateTable(
                name: "TransferCalculation",
                columns: table => new
                {
                    BusinessGroup = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    TransferQuoteJson = table.Column<string>(type: "text", nullable: true),
                    HasLockedInTransferQuote = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferCalculation", x => new { x.BusinessGroup, x.ReferenceNumber });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferCalculation");

            migrationBuilder.AddColumn<string>(
                name: "TransferQuoteJson",
                table: "Calculation",
                type: "text",
                nullable: true);
        }
    }
}
