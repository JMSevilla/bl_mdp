using Microsoft.EntityFrameworkCore.Migrations;
using WTW.MdpService.Domain.Mdp;

#nullable disable

namespace WTW.MdpService.Migrations
{
    public partial class UpdateBereavementContactConfirmation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("alter table \"BereavementContactConfirmation\" alter column \"Contact\" TYPE VARCHAR(100);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
