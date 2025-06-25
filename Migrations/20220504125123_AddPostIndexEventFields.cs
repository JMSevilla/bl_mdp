using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddPostIndexEventFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityDocumentFile",
                table: "RetirementPostIndexEvent",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IdentityDocumentImageId",
                table: "RetirementPostIndexEvent",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetirementApplicationImageId",
                table: "RetirementPostIndexEvent",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityDocumentFile",
                table: "RetirementPostIndexEvent");

            migrationBuilder.DropColumn(
                name: "IdentityDocumentImageId",
                table: "RetirementPostIndexEvent");

            migrationBuilder.DropColumn(
                name: "RetirementApplicationImageId",
                table: "RetirementPostIndexEvent");
        }
    }
}
