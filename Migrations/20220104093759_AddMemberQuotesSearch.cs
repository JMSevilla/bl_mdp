using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddMemberQuotesSearch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberQuotesSearch",
                columns: table => new
                {
                    ReferenceNumber = table.Column<string>(type: "text", nullable: false),
                    BusinessGroup = table.Column<string>(type: "text", nullable: false),
                    LatestCalculationRetirementDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberQuotesSearch", x => new { x.BusinessGroup, x.ReferenceNumber });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberQuotesSearch");
        }
    }
}