using Microsoft.EntityFrameworkCore.Migrations;

namespace PocketPermaculture.Data.Migrations
{
    public partial class UserPinTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PinType",
                table: "UserPins",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PinType",
                table: "UserPins");
        }
    }
}
