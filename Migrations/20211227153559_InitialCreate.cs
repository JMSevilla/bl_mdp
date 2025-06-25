using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RetirementJourney",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessGroup = table.Column<string>(type: "text", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetirementJourney", x => x.Id);
                    table.UniqueConstraint("AK_RetirementJourney_BusinessGroup_ReferenceNumber", x => new { x.BusinessGroup, x.ReferenceNumber });
                });

            migrationBuilder.CreateTable(
                name: "JourneyBranch",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RetirementJourneyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JourneyBranch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JourneyBranch_RetirementJourney_RetirementJourneyId",
                        column: x => x.RetirementJourneyId,
                        principalTable: "RetirementJourney",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JourneyStep",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrentPageKey = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    NextPageKey = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    SubmitDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    JourneyBranchId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JourneyStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JourneyStep_JourneyBranch_JourneyBranchId",
                        column: x => x.JourneyBranchId,
                        principalTable: "JourneyBranch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionForm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionKey = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    AnswerKey = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    JourneyStepId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionForm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionForm_JourneyStep_JourneyStepId",
                        column: x => x.JourneyStepId,
                        principalTable: "JourneyStep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JourneyBranch_RetirementJourneyId",
                table: "JourneyBranch",
                column: "RetirementJourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_JourneyStep_JourneyBranchId",
                table: "JourneyStep",
                column: "JourneyBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionForm_JourneyStepId",
                table: "QuestionForm",
                column: "JourneyStepId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionForm");

            migrationBuilder.DropTable(
                name: "JourneyStep");

            migrationBuilder.DropTable(
                name: "JourneyBranch");

            migrationBuilder.DropTable(
                name: "RetirementJourney");
        }
    }
}