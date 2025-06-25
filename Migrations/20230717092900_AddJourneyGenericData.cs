using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddJourneyGenericData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JourneyGenericData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GenericDataJson = table.Column<string>(type: "text", nullable: false),
                    FormKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    JourneyStepId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JourneyGenericData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JourneyGenericData_JourneyStep_JourneyStepId",
                        column: x => x.JourneyStepId,
                        principalTable: "JourneyStep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JourneyGenericData_JourneyStepId",
                table: "JourneyGenericData",
                column: "JourneyStepId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JourneyGenericData");
        }
    }
}
