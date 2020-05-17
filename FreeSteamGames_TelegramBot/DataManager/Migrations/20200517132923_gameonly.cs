using Microsoft.EntityFrameworkCore.Migrations;

namespace DataManager.Migrations
{
    public partial class gameonly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "wantsGameInfo",
                table: "subscribers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "wantsGameInfo",
                table: "subscribers");
        }
    }
}
