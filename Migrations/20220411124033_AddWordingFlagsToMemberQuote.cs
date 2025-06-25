using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddWordingFlagsToMemberQuote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MemberQuote_WordingFlags",
                table: "RetirementJourney",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberQuote_WordingFlags",
                table: "RetirementJourney");
        }
    }
}
