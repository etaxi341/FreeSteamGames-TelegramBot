using Microsoft.EntityFrameworkCore.Migrations;

namespace DataManager.Migrations
{
    public partial class first : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subscribers",
                columns: table => new
                {
                    chatID = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    wantsDlcInfo = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscribers", x => x.chatID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subscribers");
        }
    }
}
