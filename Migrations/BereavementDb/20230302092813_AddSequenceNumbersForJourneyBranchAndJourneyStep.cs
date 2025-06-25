using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations.BereavementDb
{
    public partial class AddSequenceNumbersForJourneyBranchAndJourneyStep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SequenceNumber",
                table: "JourneyStep",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SequenceNumber",
                table: "JourneyBranch",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                table: "JourneyStep");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                table: "JourneyBranch");
        }
    }
}
