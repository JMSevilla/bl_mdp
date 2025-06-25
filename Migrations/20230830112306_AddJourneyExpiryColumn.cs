using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddJourneyExpiryColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpirationDate",
                table: "Journeys",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Journeys");
        }
    }
}
