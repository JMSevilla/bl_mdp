using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class UpdateConfirmationAttemptCountColumnInContactConfirmation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MobileConfirmationAttemptCount",
                table: "ContactConfirmation",
                newName: "FailedConfirmationAttemptCount");
            
            migrationBuilder.AddColumn<int>(
                name: "MaximumConfirmationAttemptCount",
                table: "ContactConfirmation",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaximumConfirmationAttemptCount",
                table: "ContactConfirmation");

            migrationBuilder.RenameColumn(
                name: "FailedConfirmationAttemptCount",
                table: "ContactConfirmation",
                newName: "MobileConfirmationAttemptCount");
        }
    }
}
