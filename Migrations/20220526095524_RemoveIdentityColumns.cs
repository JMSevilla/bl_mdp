using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class RemoveIdentityColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityDocumentFile",
                table: "RetirementPostIndexEvent");

            migrationBuilder.DropColumn(
                name: "IdentityDocumentImageId",
                table: "RetirementPostIndexEvent");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityDocumentFile",
                table: "RetirementPostIndexEvent",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdentityDocumentImageId",
                table: "RetirementPostIndexEvent",
                type: "integer",
                nullable: true);
        }
    }
}
