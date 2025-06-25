using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddFieldsForRetirementJourney : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AcknowledgeFinancialAdvisor",
                table: "RetirementJourney",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AcknowledgePensionWise",
                table: "RetirementJourney",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcknowledgeFinancialAdvisor",
                table: "RetirementJourney");

            migrationBuilder.DropColumn(
                name: "AcknowledgePensionWise",
                table: "RetirementJourney");
        }
    }
}
