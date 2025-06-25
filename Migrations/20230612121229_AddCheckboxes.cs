using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class AddCheckboxes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckboxesList",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CheckboxesListKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    JourneyStepId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckboxesList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckboxesList_JourneyStep_JourneyStepId",
                        column: x => x.JourneyStepId,
                        principalTable: "JourneyStep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Checkbox",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AnswerValue = table.Column<bool>(type: "boolean", nullable: false),
                    CheckboxesListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkbox", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Checkbox_CheckboxesList_CheckboxesListId",
                        column: x => x.CheckboxesListId,
                        principalTable: "CheckboxesList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Checkbox_CheckboxesListId",
                table: "Checkbox",
                column: "CheckboxesListId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckboxesList_JourneyStepId",
                table: "CheckboxesList",
                column: "JourneyStepId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Checkbox");

            migrationBuilder.DropTable(
                name: "CheckboxesList");
        }
    }
}
