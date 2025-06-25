using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class MoreMemberQuoteData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MemberQuote_CalculationType",
                table: "RetirementJourney",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MemberQuote_PensionOptionNumber",
                table: "RetirementJourney",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberQuote_CalculationType",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "MemberQuote_PensionOptionNumber",
                table: "RetirementJourney");
        }
    }
}
