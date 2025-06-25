using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WTW.MdpService.Migrations.BereavementDb
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BereavementContactConfirmation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessGroup = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Token = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Contact = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedConfirmationAttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaximumConfirmationAttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BereavementContactConfirmation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BereavementJourney",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessGroup = table.Column<string>(type: "text", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SubmissionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BereavementJourney", x => x.Id);
                    table.UniqueConstraint("AK_BereavementJourney_BusinessGroup_ReferenceNumber", x => new { x.BusinessGroup, x.ReferenceNumber });
                });

            migrationBuilder.CreateTable(
                name: "JourneyBranch",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    BereavementJourneyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JourneyBranch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JourneyBranch_BereavementJourney_BereavementJourneyId",
                        column: x => x.BereavementJourneyId,
                        principalTable: "BereavementJourney",
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
                    AnswerKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AnswerValue = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                name: "IX_JourneyBranch_BereavementJourneyId",
                table: "JourneyBranch",
                column: "BereavementJourneyId");

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
                name: "BereavementContactConfirmation");

            migrationBuilder.DropTable(
                name: "QuestionForm");

            migrationBuilder.DropTable(
                name: "JourneyStep");

            migrationBuilder.DropTable(
                name: "JourneyBranch");

            migrationBuilder.DropTable(
                name: "BereavementJourney");
        }
    }
}
