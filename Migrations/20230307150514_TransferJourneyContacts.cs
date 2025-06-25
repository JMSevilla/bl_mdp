using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class TransferJourneyContacts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransferJourneyContact",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CompanyName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email_Address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Phone_FullNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address_StreetAddress1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address_StreetAddress2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address_StreetAddress3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address_StreetAddress4 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address_StreetAddress5 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address_Country = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Address_CountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    Address_PostCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TransferJourneyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferJourneyContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferJourneyContact_TransferJourney_TransferJourneyId",
                        column: x => x.TransferJourneyId,
                        principalTable: "TransferJourney",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransferJourneyContact_TransferJourneyId",
                table: "TransferJourneyContact",
                column: "TransferJourneyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferJourneyContact");
        }
    }
}
