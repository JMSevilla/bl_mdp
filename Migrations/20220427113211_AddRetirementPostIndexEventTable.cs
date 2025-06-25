using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddRetirementPostIndexEventTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RetirementPostIndexEvent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessGroup = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    CaseNumber = table.Column<int>(type: "integer", nullable: false),
                    BatchNumber = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetirementPostIndexEvent", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RetirementPostIndexEvent");
        }
    }
}
