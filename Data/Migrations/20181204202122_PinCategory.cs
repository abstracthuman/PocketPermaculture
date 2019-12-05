using Microsoft.EntityFrameworkCore.Migrations;

namespace PocketPermaculture.Data.Migrations
{
    public partial class PinCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PinCategory",
                table: "UserPins",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PinCategory",
                table: "UserPins");
        }
    }
}
